using System;
using Azure.Data.Tables;

namespace Models
{
    public class product : ITableEntity
    {
        public string RowKey { get; set; } = default!;
        public string PartitionKey { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public bool Sale { get; set; } = default!;
        public Azure.ETag ETag { get; set; } = default!;
        public System.DateTimeOffset? Timestamp { get; set; } = default!;
    }
}