
using Azure.Data.Tables;
using CustomerQuery.Models;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CustomerQuery.Controllers
{
    public class HomeController : Controller
    {
        // Storage account
        public static string mStorageConnectionString = ConfigurationManager.AppSettings["mStorageConnectionString"];
        //Cache
        public static string mCacheConnectionString = ConfigurationManager.AppSettings["mCacheConnectionString"];

        public ActionResult Index()
        {
            return View(new HomeViewModel());
        }

        public ActionResult SearchCustomers(HomeViewModel data)
        {
            // Connect to the table
            TableServiceClient tableServiceClient = new TableServiceClient(mStorageConnectionString);
            TableClient tableClient = tableServiceClient.GetTableClient("customers");

            // Connect to the cache
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(mCacheConnectionString);
            IDatabase db = connection.GetDatabase();

            data.MatchedCustomers.Clear();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Not using the cache
            var queryResults = tableClient.Query<TableEntity>(filter: $"Name eq '{data.SearchString}'");
            if(queryResults.Count() > 0)
            {
                foreach (var entity in queryResults)
                {
                    //TableEntity newEntity = tableClient.GetEntity<TableEntity>(entity.PartitionKey, entity.RowKey);
                    //newEntity["Value"] = ((double)entity["Value"] * 100) / 100.0;
                    //tableClient.UpdateEntity(newEntity, entity.ETag, TableUpdateMode.Merge);
                    data.TableCustomers.Add(new Customer
                    {
                        Name = data.SearchString,
                        Value = String.Format("{0:C2}", entity["Value"])
                    });
                }

                watch.Stop();
                data.TableResponseTime = watch.ElapsedMilliseconds;

                watch.Restart();

                // Using the cache
                var record = db.StringGet("cust:" + data.SearchString.Replace(' ', ':'));

                // If the customer details are already cached
                if (!record.IsNullOrEmpty)
                {
                    // Query the table using the cached partition key and row key
                    string[] parts = Encoding.ASCII.GetString(record).Split(':');
                    if (parts.Length == 2)
                    {
                        var quickQueryResults = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{parts[0]}' and RowKey eq '{parts[1]}'");
                        foreach (var entity in quickQueryResults)
                        {
                            //TableEntity newEntity = tableClient.GetEntity<TableEntity>(entity.PartitionKey, entity.RowKey);
                            //newEntity["Value"] = ((double)entity["Value"] * 100) / 100.0;
                            //tableClient.UpdateEntity(newEntity, entity.ETag, TableUpdateMode.Merge);
                            data.MatchedCustomers.Add(new Customer
                            {
                                Name = data.SearchString,
                                Value = String.Format("{0:C2}", entity["Value"])
                            });
                        }
                    }
                    watch.Stop();
                    data.CachedResponseTime = watch.ElapsedMilliseconds;
                    data.ResponseTimeDifference = (data.TableResponseTime - data.CachedResponseTime);

                    watch.Restart();
                    // Use only the cached values
                    data.CachedCustomers.Add(new Customer
                    {
                        Name = data.SearchString,
                        Value = String.Format("{0:C2}", db.SortedSetScore("customervalues", record))
                    });
                    watch.Stop();
                    data.CacheResponseTime = watch.ElapsedMilliseconds;
                    
                }
                // If the customer details aren't in the cache yet, add to the cache
                else
                {
                    foreach(var entity in queryResults)
                    {
                        string key = $"{entity.PartitionKey}:{entity.RowKey}";
                        db.SortedSetAdd("customervalues", key, (double)entity["Value"]);
                        db.StringSet("cust:" + data.SearchString.Replace(' ', ':'), key);
                        data.NotPreviouslyInCache = true;
                    }
                }
                connection.Dispose();
            }

            return View("Index", data);
        }
    }
}
