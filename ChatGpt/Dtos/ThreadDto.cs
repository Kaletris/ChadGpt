namespace ChatGpt.Dtos;

public class ThreadDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required UserDto Owner { get; set; }
}