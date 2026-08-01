namespace GameServer.Api.Models;

public record LoginSession(
    int PlayerId,
    DateTime ExpiresAt);