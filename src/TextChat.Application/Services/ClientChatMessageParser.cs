using System.Text.Json;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ClientChatMessageParser : IClientChatMessageParser
{
	public Task<Result<ClientChatMessage>> Parse(string message)
	{
		ClientChatMessage? clientChatMessage = JsonSerializer.Deserialize<ClientChatMessage>(message);

		if (clientChatMessage is not null)
			return Task.FromResult(Result<ClientChatMessage>.Success(clientChatMessage));
		else
			return Task.FromResult(Result<ClientChatMessage>.Failure(new Error("", ""))); // TODO
	}
}