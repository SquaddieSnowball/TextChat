using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IChatMessageParser
{
	Task<Result<ClientChatMessage>> ParseClientMessage(string message);

	Task<Result<ServerChatMessage>> ParseServerMessage(string message);
}