using System.Text.Json;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ClientChatMessageBuilder : IClientChatMessageBuilder
{
	public Task<Result<string>> Build(ClientChatMessage message)
	{
		string stringMessage = JsonSerializer.Serialize(message);

		return Task.FromResult(Result<string>.Success(stringMessage));
	}
}