using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IServerChatMessageParser
{
	Task<Result<ServerChatMessage>> Parse(string message);
}