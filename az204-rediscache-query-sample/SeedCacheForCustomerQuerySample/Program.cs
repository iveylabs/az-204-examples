using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using StackExchange.Redis;

namespace SeedCacheForCustomerQuerySample
{
    class Program
    {
        public static string mStorageConnectionString = ConfigurationManager.AppSettings["mStorageConnectionString"];
        
        static void Main(string[] args)
        {
            generateCustomerRecords();
            Console.WriteLine("Seeding completed successfully. Press ENTER to exit...");
            Console.ReadLine();
        }

        static void generateCustomerRecords()
        {
            if (!confirmOperation("YOUR CUSTOMER TABLE WILL BE RECREATED! ALL EXISTING DATA WILL BE LOST! Are you sure?"))
                return;
           CloudStorageAccount account = CloudStorageAccount.Parse(mStorageConnectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("customers");
            table.DeleteIfExists();
            while (true)
            {
                try
                {
                    table.CreateIfNotExists();
                    break;
                }

                catch
                {
                    Console.WriteLine("RETRY TABLE CREATION");
                    Thread.Sleep(5000);
                }
            }
            int count = 0;
            int totalRecords = 50000;
            string[] firstNames = File.ReadAllLines("CSV_Database_of_First_Names.csv");
            string[] lastNames = File.ReadAllLines("CSV_Database_of_Last_Names.csv");
            Random rand = new Random();
            Dictionary<string, TableBatchOperation> batches = new Dictionary<string, TableBatchOperation>();
            while (count < totalRecords)
            {
                count++;
                var company = "Company " + rand.Next(1, 11);
                if (!batches.ContainsKey(company))
                    batches.Add(company, new TableBatchOperation());

                Customer cust = new Customer(company, count.ToString())
                {
                    Value = (rand.NextDouble() - 0.5) * 99999.0,
                    ContractDate = DateTime.Now,
                    Name = firstNames[rand.Next(0, firstNames.Length)] + " " + lastNames[rand.Next(0, lastNames.Length)]
                };
                batches[company].Insert(cust);
                if (batches[company].Count() >= 100)
                {
                    Console.WriteLine("Committing " + batches[company].Count + " records to " + company);
                    table.ExecuteBatch(batches[company]);
                    batches[company].Clear();
                }
            }
            foreach(var batch in batches.Values)
            {
                if (batch.Count > 0)
                {
                    Console.WriteLine("Committing " + batch.Count + "records...");
                    table.ExecuteBatch(batch);
                }
            }
        }
        static bool confirmOperation(string message) 
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n\n\n" + message + "\n\n\nPlease confirm (Y/N): ");
            Console.ForegroundColor = color;
            string input = Console.ReadLine();
            if (input != "Y" && input != "y")
                return false;
            else
                return true;
        }
    }
    public class Customer : TableEntity
    {
        public string Id { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Comment { get; set; }
        public DateTime ContractDate { get; set; }
        public Customer(string company, string id)
        {
            PartitionKey = company;
            RowKey = id;
            Company = company;
            Id = id; 
        }
        public Customer()
        {
        }
    }
}
