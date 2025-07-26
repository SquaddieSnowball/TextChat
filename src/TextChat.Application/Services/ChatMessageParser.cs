using System.Text.Json;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ChatMessageParser : IChatMessageParser
{
	public Task<Result<ClientChatMessage>> ParseClientMessage(string message)
	{
		ClientChatMessage? clientChatMessage = JsonSerializer.Deserialize<ClientChatMessage>(message);

		if (clientChatMessage is not null)
			return Task.FromResult(Result<ClientChatMessage>.Success(clientChatMessage));
		else
			return Task.FromResult(Result<ClientChatMessage>.Failure(new Error("", ""))); // TODO
	}

	public Task<Result<ServerChatMessage>> ParseServerMessage(string message)
	{
		ServerChatMessage? serverChatMessage = JsonSerializer.Deserialize<ServerChatMessage>(message);

		if (serverChatMessage is not null)
			return Task.FromResult(Result<ServerChatMessage>.Success(serverChatMessage));
		else
			return Task.FromResult(Result<ServerChatMessage>.Failure(new Error("", ""))); // TODO
	}
}