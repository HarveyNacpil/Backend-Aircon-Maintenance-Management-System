using System.Text.Json.Serialization;

namespace Api.SparePartsModule
{
    public sealed class SparePart
    {
        [JsonPropertyName("part_id")]
        public int PartId { get; set; }

        [JsonPropertyName("part_name")]
        public string PartName { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public double? UnitPrice { get; set; }

        /// <summary>In Stock | Low Stock | Out of Stock</summary>
        [JsonPropertyName("stock_status")]
        public string StockStatus { get; set; } = "In Stock";
    }

    public sealed class PartUsage
    {
        [JsonPropertyName("usage_id")]
        public int UsageId { get; set; }

        [JsonPropertyName("record_id")]
        public int RecordId { get; set; }

        [JsonPropertyName("part_id")]
        public int PartId { get; set; }

        [JsonPropertyName("quantity_used")]
        public int QuantityUsed { get; set; }
    }
}
