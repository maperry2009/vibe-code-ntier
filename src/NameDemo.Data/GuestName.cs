namespace NameDemo.Data;

public class GuestName
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
