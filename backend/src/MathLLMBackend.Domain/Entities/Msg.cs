namespace MathLLMBackend.Domain.Entities;
public enum MsgType
{
    user,
    assistant,
    system
}
public class Msg
{
    public Msg(long chatId, string message, MsgType mtype)
    {
        ChatId = chatId;
        Message = message;
        MType = mtype.ToString();
    }
    public Msg() { }
    public long Id { get; set; }
    public long ChatId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MType { get; set; }
}
