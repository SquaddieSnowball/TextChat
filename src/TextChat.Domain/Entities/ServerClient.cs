using System.Net;
using System.Net.Sockets;

namespace TextChat.Domain.Entities;

public record class ServerClient(
	Guid Id,
	TcpClient TcpClient,
	IPEndPoint Endpoint,
	StreamReader StreamReader,
	StreamWriter StreamWriter);