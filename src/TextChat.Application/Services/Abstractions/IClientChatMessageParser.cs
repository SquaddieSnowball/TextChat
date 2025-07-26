using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IClientChatMessageParser
{
	Task<Result<ClientChatMessage>> Parse(string message);
}