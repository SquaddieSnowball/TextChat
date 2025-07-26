using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IClientChatMessageBuilder
{
	Task<Result<string>> Build(ClientChatMessage message);
}