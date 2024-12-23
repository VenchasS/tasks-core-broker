using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace TaskQueue.Dto;

public class TaskDto
{
    [Required(ErrorMessage = "Type is required.")]
    public TaskType Type { get; set; }

    [Required(ErrorMessage = "Data is required.")]
    public string Data { get; set; }

    public int Ttl { get; set; } = 0;
}