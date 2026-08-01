namespace GameServer.Api.Models;

public record PlayerData(
    int Id,
    string Username,
    string Nickname,
    int Level,
    int Gold);