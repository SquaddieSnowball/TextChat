namespace TextChat.Domain.Entities;

public record class ClientChatMessage(DateTime SentTimestamp, string Body);