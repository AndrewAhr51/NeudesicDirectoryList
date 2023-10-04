
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace NeudesicDirectoryList.Models
{
    internal class NeudesicDirectoryItem : TableEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("office")]
        public string Office { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }

    internal class CreateNeudesicDirectoryItem
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Office { get; set; }
        public string Region { get; set; }
    }

    internal class UpdateNeudesicDirectoryItem
    {
        public int id { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string Office { get; set; }

    }

    internal class DeleteNeudesicDirectoryItem
    {
        public string id { get; set; }

        public string region { get; set; }

    }

}
