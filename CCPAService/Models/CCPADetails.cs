using fpsLibrary.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CCPAService.Models
{
    public class CCPADetails : AzureTableEntity
    {
        public CCPADetails()
        { }

        [Display(Name = "FPS ID")]
        public string FpsId { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Brand")]
        public string Brand { get; set; }

        public Guid uid { get; set; }

        [Display(Name = "Activity ID")]
        public string ActivityID { get; set; }

        public string Tier { get; set; }

        [Display(Name = "Request Type")]
        public string RequestType { get; set; }

        [Display(Name = "Request Date")]
        public DateTimeOffset RequestDate { get; set; }

        [Display(Name = "Last Update")]
        public DateTimeOffset LastUpdate { get; set; }

        public string Status { get; set; }

        [Display(Name = "Agent ID")]
        public string AgentID { get; set; }

        [Display(Name = "Request Source")]
        public string RequestSource { get; set; }

        public bool isFileDeleted { get; set; }

        public bool isDetailsFlag { get; set; }
    }
    public class CCPARequestEntityKeys : TableStorageKeys
    {
        //public CCPARequestEntityKeys(string tagCountry)
        //{
        //    this.PartitionKey = BuildPartitionKey("R000001");
        //    this.RowKey = BuildRowKey(tagCountry);
        //}

        public static string BuildPartitionKey(int startValue)
        {
            startValue++;
            string seqNumber = startValue.ToString();
            //if (startValue.ToString().Length < 7)
            //{
            //    seqNumber = startValue.ToString().PadLeft(7, '0');
            //}
            return seqNumber;
            //startValue++;
            //return String.Format("00{0}", startValue.ToString());
        }

        public static string BuildRowKey(string status)
        {
            return status;
        }
    }

    public class CCPARequestEntity : TableEntity
    {
        public CCPARequestEntity()
        { }

        public CCPARequestEntity(CCPARequestEntityKeys keys)
        {
            this.PartitionKey = keys.PartitionKey;
            this.RowKey = keys.RowKey;
        }
        public string FpsId { get; set; }
        public string Brand { get; set; }
        public string Country { get; set; }
        public string Tier { get; set; }
        public string RequestType { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string Requester { get; set; }
        public string RequestSource { get; set; }
        public bool isFileDeleted { get; set; }
    }
}
