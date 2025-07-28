namespace TextChat.Domain.Entities;

public record class ServerChatMessage(string ClientIPAddress, DateTime ReceivedTimestamp, string Body);