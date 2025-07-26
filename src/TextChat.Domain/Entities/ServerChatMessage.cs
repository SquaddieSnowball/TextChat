using System.Net;

namespace TextChat.Domain.Entities;

public record class ServerChatMessage(IPAddress ClientIPAddress, DateTime ReceivedTimestamp, string Body);