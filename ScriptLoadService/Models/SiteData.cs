using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using fpsLibrary.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace ScriptLoadService.Models
{
    public class SiteData : AzureTableEntity
    {
        public SiteData()
        { }

        public SiteData(string tagBrand, string tagCountry)
        {
            this.Brand = tagBrand;
            this.Country = tagCountry;

            this.PersonalizationActive = true;
            this.CookieDaysToLive = 365;
        }

        [Display(Name = "Brand")]
        [Required]
        public string Brand { get; set; }

        [Display(Name = "Country")]
        [Required]
        public string Country { get; set; }

        [Display(Name = "Personalization Active")]
        [Required]
        public bool PersonalizationActive { get; set; }

        [Display(Name = "Cookie Days To Live")]
        [Required]
        public int CookieDaysToLive { get; set; }

        [Display(Name = "Site domain")]
        public string Domain { get; set; }

        [Display(Name = "FPSXDC Service Url")]
        public string DomainPath { get; set; }

        [Display(Name = "Default Path")]
        public string XdcPath { get; set; }

        [Display(Name = "Site OnLoad Script")]
        [DataType(DataType.MultilineText)]
        public string SiteOnloadScript { get; set; }
    }
    public class SiteEntityKeys : TableStorageKeys
    {
        public SiteEntityKeys(string tagBrand, string tagCountry)
        {
            this.PartitionKey = BuildPartitionKey(tagBrand);
            this.RowKey = BuildRowKey(tagCountry);
        }

        public static string BuildPartitionKey(string tagBrand)
        {
            return tagBrand;
        }

        public static string BuildRowKey(string tagCountry)
        {
            return tagCountry;
        }
    }

    public class SiteEntity : TableEntity
    {
        public SiteEntity() { }

        public SiteEntity(SiteEntityKeys keys)
        {
            this.PartitionKey = keys.PartitionKey;
            this.RowKey = keys.RowKey;

            this.PersonalizationActive = true;
            this.CookieDaysToLive = 365;
        }

        public bool PersonalizationActive { get; set; }

        public int CookieDaysToLive { get; set; }

        public string Domain { get; set; }

        public string DomainPath { get; set; }

        public string XdcPath { get; set; }

        public string SiteOnloadScript { get; set; }
    }
}
