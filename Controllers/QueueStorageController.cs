using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Controllers
{
    [ApiController]
    [Route("Controller")]
    public class QueueStorageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public string connectionString;
        public QueueStorageController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetValue<string>("ConnectionStrings:StorageConnectionString");
        }

        [HttpPost("CreateQueue")]
        public IActionResult CreateQueue(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("Queue Name cannot be empty - " + queueName);
            }
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(this.connectionString, queueName);
            try
            {
                queueClient.CreateIfNotExistsAsync();
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Unable to create Queue due to : {0}", ex);
            }
            return Created($"Queue created successfully. Queue Name - {0}", queueClient.Uri);
        }

        //-------------------------------------------------
        // Insert a message into a queue
        //-------------------------------------------------
        [HttpPost("InsertMessage")]
        public void InsertMessage(string queueName, string message)
        {
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(this.connectionString, queueName);

            // Create the queue if it doesn't already exist
            queueClient.CreateIfNotExists();

            if (queueClient.Exists())
            {
                // Send a message to the queue
                queueClient.SendMessage(message);
            }

            Console.WriteLine($"Inserted: {message}");
        }

        //-------------------------------------------------
        // Peek at a message in the queue
        //-------------------------------------------------
        [HttpGet]
        public IActionResult PeekMessage(string queueName)
        {
            // Get the connection string from app settings
            //string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(this.connectionString, queueName);

            if (queueClient.Exists())
            {
                // Peek at the next message
                PeekedMessage[] peekedMessage = queueClient.PeekMessages();

                // Display the message
                //Console.WriteLine($"Peeked message: '{peekedMessage[0].Body}'");
                return Ok(peekedMessage[0].Body.ToString());
            }
            else{
            return new EmptyResult();
            }
        }

        //-------------------------------------------------
        // Update an existing message in the queue
        //-------------------------------------------------
        [HttpPut]
        public void UpdateMessage(string queueName)
        {
            // Get the connection string from app settings
            //string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(this.connectionString, queueName);

            if (queueClient.Exists())
            {
                // Get the message from the queue
                QueueMessage[] message = queueClient.ReceiveMessages();

                // Update the message contents
                queueClient.UpdateMessage(message[0].MessageId,
                        message[0].PopReceipt,
                        "Updated contents",
                        TimeSpan.FromSeconds(60.0)  // Make it invisible for another 60 seconds
                    );
            }
        }

        //-------------------------------------------------
        // Process and remove a message from the queue
        //-------------------------------------------------
        [HttpDelete]
        //-------------------------------------------------
        // Delete the queue
        //-------------------------------------------------
        public void DeleteQueue(string queueName)
        {
            // Get the connection string from app settings
            //string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(this.connectionString, queueName);

            if (queueClient.Exists())
            {
                // Delete the queue
                queueClient.Delete();
            }

            Console.WriteLine($"Queue deleted: '{queueClient.Name}'");
        }
    }

}