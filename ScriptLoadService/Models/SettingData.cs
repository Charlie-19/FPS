using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using fpsLibrary.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace ScriptLoadService.Models
{
    public class SettingData : AzureTableEntity
    {
        public SettingData()
        {
            //  this.Archives = new List<SettingArchive>();
        }

        public SettingData(string version, string name)
        {
            this.Version = version;
            this.Name = name;

            this.SettingValue = "";
            //this.Archives = new List<SettingArchive>();
        }

        [Display(Name = "Version")]
        [Required]
        public string Version { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Setting Value")]
        [Required]
        [DataType(DataType.MultilineText)]
        public string SettingValue { get; set; }

        [Display(Name = "Recent Values")]
        // public IList<SettingArchive> Archives { get; set; }

        // For promotion of settings
        public bool HasChanges { get; set; }

        public bool IsNew { get; set; }

        [Display(Name = "QA Value")]
        public string PromotionValue { get; set; }

        public bool Promote { get; set; }
    }

    public class SettingKeys : TableStorageKeys
    {
        public SettingKeys(string version, string name)
        {
            this.PartitionKey = BuildPartitionKey(version);
            this.RowKey = BuildRowKey(name);
        }

        public static string BuildPartitionKey(string version)
        {
            return version;
        }

        public static string BuildRowKey(string name)
        {
            return name;
        }
    }

    public class SettingEntity : TableEntity
    {
        public SettingEntity() { }

        public SettingEntity(SettingKeys keys, string settingValue)
        {
            this.PartitionKey = keys.PartitionKey;
            this.RowKey = keys.RowKey;

            this.SettingValue = settingValue;
        }

        public string SettingValue { get; set; }
    }
}
