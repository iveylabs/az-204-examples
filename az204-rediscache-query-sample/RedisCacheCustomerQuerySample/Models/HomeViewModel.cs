using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomerQuery.Models
{
    public class HomeViewModel
    {
        [Display(Name="Customer name")]
        public string SearchString { get; set; }
        [Display(Name = "Response time with cache")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public long CachedResponseTime { get; set; }
        [Display(Name = "Response time cache only")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public long CacheResponseTime { get; set; }
        [Display(Name="Response time without cache")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public long TableResponseTime { get; set; }
        [Display(Name = "Time saved by cache")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public long ResponseTimeDifference { get; set; }
        public bool NotPreviouslyInCache {  get; set; }
        public List<Customer> MatchedCustomers { get; set; }
        public List<Customer> TableCustomers { get; set; }
        public List<Customer> CachedCustomers { get; set; }
        public HomeViewModel()
        {
            MatchedCustomers = new List<Customer>();
            TableCustomers = new List<Customer>();
            CachedCustomers = new List<Customer>();
        }
    }
    public class Customer: TableEntity
    {
        public string Id { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }
        public DateTime ContractDate { get; set; }
    }
}