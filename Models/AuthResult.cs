namespace GameServer.Api.Models;

public record AuthResult(
    bool Success,
    string? AccessToken,
    int PlayerId,
    string? ErrorMessage);