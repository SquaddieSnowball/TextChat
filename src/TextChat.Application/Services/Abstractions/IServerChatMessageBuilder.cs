using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IServerChatMessageBuilder
{
	Task<Result<string>> Build(ServerChatMessage message);
}