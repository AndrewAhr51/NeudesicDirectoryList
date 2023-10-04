using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NeudesicDirectoryList.Models;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace NeudesicDirectoryList
{
    public class NeudesicDirectoryApi
    {
        private readonly CosmosClient _cosmosClient;
        private Container documentContainer;

        public NeudesicDirectoryApi(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            documentContainer = cosmosClient.GetContainer("neudesicdirectorydb", "neudesicdirectorycontainer");
        }
       
        [FunctionName("GetNeudesicDirectoryItems")]
        public async Task<IActionResult> GetNeudesicDirectoryItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "neudesicdirectory")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting all Neudesic Directory Items.");
            List<NeudesicDirectoryItem> neudesicDirectoryItem = new();
                        
            var items = documentContainer.GetItemQueryIterator<NeudesicDirectoryItem>();

            return new OkObjectResult((await items.ReadNextAsync()).ToList());
        }

        [FunctionName("GetNeudesicDirectoryItemById")]
        public async Task<IActionResult> GetNeudesicDirectoryItemById(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "neudesicdirectory/{id}/{region}")]
           HttpRequest req, ILogger log, string id, string region)
        {
            
            log.LogInformation($"Retrieving a record from the Neudesic Directory with id: {id}");

            var item = await documentContainer.ReadItemAsync<NeudesicDirectoryItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(region));

            try
            {
                if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFoundResult();
                }
            }
            catch(CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            } 
            
            return new OkObjectResult(item.Resource);
        }

        [FunctionName("CreateNeudesicDirectoryItem")]
        public async Task<IActionResult> CreateNeudesicDirectoryItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "neudesicdirectory")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a Neudesic Directory Item");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreateNeudesicDirectoryItem>(requestData);

            var item = new NeudesicDirectoryItem
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                Office = data.Office,
                Region = data.Region,
                PartitionKey = data.Region
            };

            await documentContainer.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(item.Region));
                        
            return new OkObjectResult(item);
        }


        [FunctionName("PutNeudesicDirectoryItem")]
        public async Task<IActionResult> PutNeudesicDirectoryItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "neudesicdirectory/{id}/{region}")] HttpRequest req,
            ILogger log, string id, string region)
        {
            log.LogInformation($"Updating the Neudesic Directory Item with Id: {id}");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateNeudesicDirectoryItem>(requestData);

            var item = await documentContainer.ReadItemAsync<NeudesicDirectoryItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(region));

            if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }

            item.Resource.FirstName = data.FirstName;
            item.Resource.LastName = data.LastName;
            item.Resource.Office = data.Office;

            await documentContainer.UpsertItemAsync(item.Resource);

            return new OkObjectResult(item.Resource);
        }

        [FunctionName("PatchNeudesicDirectoryItem")]
        public async Task<IActionResult> PatchNeudesicDirectoryItem(
           [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "neudesicdirectory/{id}/{region}")] HttpRequest req,
           ILogger log, string id, string region)
        {
            log.LogInformation($"Updating the Neudesic Directory Item with Id: {id}");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateNeudesicDirectoryItem>(requestData);

            var item = await documentContainer.ReadItemAsync<NeudesicDirectoryItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(region));
            var columnName = "";
            var columnValue = "";
            if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }

            if (data.FirstName is not null) 
            { 
                data.FirstName = data.FirstName.Trim();
                item.Resource.FirstName = data.FirstName;
                columnName = "/firstname";
                columnValue = data.FirstName;
            }

            if (data.LastName is not null)
            {
                data.LastName = data.LastName.Trim();
                item.Resource.LastName = data.LastName;
                columnName = "/lastname";
                columnValue = data.LastName;

            }

            if (data.Office is not null) 
            {
                data.Office = data.Office.Trim();
                item.Resource.Office = data.Office;
                columnName = "/office";
                columnValue = data.Office;


            }

            var itemPartionKey = item.Resource.Region;
            id = item.Resource.Id;

            await documentContainer.PatchItemAsync<NeudesicDirectoryItem>(
                id: id,
                partitionKey: new PartitionKey(itemPartionKey),
                patchOperations: new[]
                    { PatchOperation.Replace(columnName, columnValue )
                }
            );
    
            return new OkObjectResult(item.Resource);
        }


        [FunctionName("DeleteNeudesicDirectoryItem")]
        public async Task<IActionResult> DeleteNeudesicDirectoryItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "neudesicdirectory/{id}/{region}")] HttpRequest req,
            ILogger log, string id, string region)
        {
            log.LogInformation("Deleting the record form the Neudesic Directory.");

            var item = await documentContainer.ReadItemAsync<NeudesicDirectoryItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(region));

            try
            {
                if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFoundResult();
                }
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }

            await documentContainer.DeleteItemAsync<DeleteNeudesicDirectoryItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(region));

            return new OkResult();
        }
    }
}

