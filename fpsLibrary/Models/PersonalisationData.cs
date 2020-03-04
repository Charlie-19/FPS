using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fpsLibrary.Models
{
    public class PersonalisationData
    {
    }

    public class PersonalizationKeys : TableStorageKeys
    {
        public const string DELIMITER_KEY = "__";
        public const string DELIMITER_SUFFIX = ">>";

        public PersonalizationKeys(string userGUID, string brand, string country, string psKey, string value, string suffix)
        {
            this.PartitionKey = BuildPartitionKey(userGUID, brand, country);
            this.RowKey = BuildRowKey(psKey, value, suffix);
        }

        public static string BuildPartitionKey(string userGUID, string brand, string country)
        {
            return userGUID + DELIMITER_KEY + brand + DELIMITER_KEY + country;
        }

        public static string BuildRowKey(string psKey, string value, string suffix)
        {
            // Alpha sort keys whose names begin with an underscore (_), concatenate values
            // Putting the psKey in the RowKey allows batch inserts per user
            // TODO: Eval batch insert per user for perf gain
            // TODO: Check property value non empty

            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(value);
            }
            catch (Exception ex)
            {
                var intermediateJson = JsonConvert.DeserializeObject<string>(value);
                jsonObject = JObject.Parse(intermediateJson);
                Console.WriteLine("Removed double stringified" + ex.ToString());
            }
            IList<string> keys = jsonObject.Properties().Where(p => p.Name.StartsWith("_")).OrderBy(p => p.Name).Select(p => p.Name).ToList();
            StringBuilder strBuilder = new StringBuilder(psKey);
            foreach (var key in keys)
            {
                strBuilder.Append(DELIMITER_KEY).Append(jsonObject[key].ToString());
            }
            if (suffix != null && suffix != "")
            {
                strBuilder.Append(DELIMITER_SUFFIX).Append(suffix);
            }
            CleanKey(strBuilder);

            return strBuilder.ToString();
        }

        public static string ExtractSuffix(string rowKey)
        {
            var suffix = "";

            if (rowKey != null && rowKey != "")
            {
                suffix = rowKey.Substring(rowKey.LastIndexOf(PersonalizationKeys.DELIMITER_SUFFIX) + PersonalizationKeys.DELIMITER_SUFFIX.Length);
            }

            return suffix;
        }
    }
    public class PersonalizationEntity : TableEntity
    {
        public PersonalizationEntity() { }

        public PersonalizationEntity(PersonalizationKeys keys, string value)
        {
            this.PartitionKey = keys.PartitionKey;
            this.RowKey = keys.RowKey;

            this.Json = value;
            this.Active = true;
            this.Count = 1;
            this.Score = 0;
            this.Referrer = string.Empty;
            this.Url = string.Empty;
            this.CreatedOn = DateTimeOffset.Now;
            this.SetOn = DateTimeOffset.Now;
        }

        public string Json { get; set; }

        public bool Active { get; set; }

        public int Count { get; set; }

        public int Score { get; set; }

        public string Referrer { get; set; }

        public string Url { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset SetOn { get; set; }
    }
}
