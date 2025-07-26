using System.Text.Json;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ChatMessageBuilder : IChatMessageBuilder
{
	public Task<Result<string>> BuildClientMessage(ClientChatMessage message)
	{
		string stringMessage = JsonSerializer.Serialize(message);

		return Task.FromResult(Result<string>.Success(stringMessage));
	}

	public Task<Result<string>> BuildServerMessage(ServerChatMessage message)
	{
		string stringMessage = JsonSerializer.Serialize(message);

		return Task.FromResult(Result<string>.Success(stringMessage));
	}
}