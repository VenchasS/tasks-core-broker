namespace RESTApiGateway.DTO
{
    public sealed class TaskDTO
    {
        public required string Id { get; set; }
        public required string Type { get; set; }
        public required int TTL { get; set; }
        public required string Data { get; set; }
    }
}