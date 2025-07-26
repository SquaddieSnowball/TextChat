using System.Text.Json;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ServerChatMessageParser : IServerChatMessageParser
{
	public Task<Result<ServerChatMessage>> Parse(string message)
	{
		ServerChatMessage? serverChatMessage = JsonSerializer.Deserialize<ServerChatMessage>(message);

		if (serverChatMessage is not null)
			return Task.FromResult(Result<ServerChatMessage>.Success(serverChatMessage));
		else
			return Task.FromResult(Result<ServerChatMessage>.Failure(new Error("", ""))); // TODO
	}
}