using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IChatMessageBuilder
{
	Task<Result<string>> BuildClientMessage(ClientChatMessage message);

	Task<Result<string>> BuildServerMessage(ServerChatMessage message);
}