
using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models;

namespace MyIOTDevice.Controllers
{
    [ApiController]
    [Route("Controller")]
    public class TableStorageController : ControllerBase
    {
        private readonly string connectionString;
        TableServiceClient tableServiceClient;

        public TableStorageController(IConfiguration _configuration)
        {
            connectionString = _configuration.GetValue<string>("ConnectionStrings:TableConnectionString");
            tableServiceClient = new TableServiceClient(connectionString);
        }

        [HttpPost("CreateTable")]
        public async Task<IActionResult> CreateTableAsync(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException($"'{nameof(tableName)}' cannot be null or empty.", nameof(tableName));
            }
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
            try
            {
                await tableClient.CreateIfNotExistsAsync();
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Unable to create table due to : {0}", ex);
            }
            return Created($"{0} Table created Successfuly", tableName);
        }

        /*Create an item
The easiest way to create a new item in a table is to create a class that implements the ITableEntity interface. 
You can then add your own properties to the class to populate columns of data in that table row.*/

        [HttpPost("AddItem")]
        public async Task<IActionResult> AddItemAsync(string tableName)
        {


            // New instance of TableClient class referencing the server-side table
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
            try
            {
                var prod = new product()
                {
                    RowKey = "100",
                    PartitionKey = "mangokey",
                    Name = "Mango",
                    Quantity = 8,
                    Sale = true
                };
                // Add new item to server-side table
                await tableClient.AddEntityAsync<product>(prod);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create Item due to : {0}", ex);
            }

            return Ok(" Created successfully");
        }

        /*Get an item
     You can retrieve a specific item from a table using the TableEntity.GetEntityAsync<T> method.
     Provide the partitionKey and rowKey as parameters to identify the correct row
    to perform a quick point read of that item.*/
        /// Read a single item from container
        [HttpGet("GetItem")]
        public async Task<IActionResult> GetItemAsync(string tableName, string RowKey, string PartitionKey)
        {
            // New instance of TableClient class referencing the server-side table
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
            Azure.Response<product> product;
            try
            {
                // Read a single item from container
                product = await tableClient.GetEntityAsync<product>(
                    rowKey: RowKey,
                    partitionKey: PartitionKey
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to Get Item due to : {0}", ex);
            }

            return Ok(" Item Retrived Successfully!! Product Name - " + product.Value.Name);
        }

         //Delete an Azure table
    //Individual tables can be deleted from the service.

    [HttpDelete("DeleteTable")]
    public async Task<IActionResult> DeleteTableAsync(string tableName)
    {
        // New instance of TableClient class referencing the server-side table
        TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
        try
        {
            await tableServiceClient.DeleteTableAsync(tableName);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to delete table due to exception: {0}", ex);
        }
        return Ok("Table " + tableName + " deleted Successfully!! ");
    }

    //Delete table entities
    //If we no longer need our new table entity, it can be deleted.
    [HttpDelete("DeleteEntities")]
    public async Task<IActionResult> DeleteEntitiesAsync(string tableName, string RowKey, string PartitionKey)
    {
        // New instance of TableClient class referencing the server-side table
        TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);

        try
        {
            // Delete the entity given the partition and row key.
            await tableClient.DeleteEntityAsync(PartitionKey, RowKey);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to delete Entities due to : {0}", ex);
        }
        return Ok("Entity deleted Successfully!! ");
    }

    }
}