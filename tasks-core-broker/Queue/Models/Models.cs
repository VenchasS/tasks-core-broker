namespace TaskQueue.Models
{
    public class Models
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string Result { get; set; }
        public TaskStatus Status { get; set; }
    }
}