namespace ChatGpt.Dtos;

public class MessageDto
{
    public int Id { get; init; }
    public required string Text { get; set; }
    public DateTime Time { get; set; }
    public required UserDto Sender { get; set; }
}