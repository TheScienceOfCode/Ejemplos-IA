namespace TareaApi.Models;

public class Tarea
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Status Status { get; set; }
    public DateTime? DueDate { get; set; }
}
