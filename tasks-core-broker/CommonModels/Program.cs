namespace Shared.Enums
{
    public enum TaskType
    {
        Addition = 0,
        Subtraction = 1,
        Multiplication = 2,
        Division = 3
    }

    public enum TaskStatus
    {
        New = 0,
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Failed = 4
    }
}

namespace Shared.Models
{
    public class TaskMessage
    {
        public string TaskId { get; set; }
        public string TaskType { get; set; }
        public string TaskData { get; set; }
        public int TTL { get; set; }

        public TaskMessage(string taskId, string taskType, string taskData, int ttl)
        {
            TaskId = taskId;
            TaskType = taskType;
            TaskData = taskData;
            TTL = ttl;
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public Enums.TaskType Type { get; set; }
        public string Data { get; set; }
        public int Ttl { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public string Result { get; set; }
        
    }
}
