using System.Collections.Concurrent;
using GameServer.Api.Models;

namespace Gameserver.Api.Repositories;

public class PlayerRepository
{
    private readonly ConcurrentDictionary<int, PlayerData> _players = new();

    public PlayerRepository()
    {
        _players[1] = new PlayerData(
            Id: 1,
            Username: "player1234",
            Nickname: "knight",
            Level: 1,
            Gold: 1000);
    }
    
    public PlayerData? FindById(int playerId)
    {
        return _players.TryGetValue(playerId, out var palyer)
            ? palyer : null;
    }

    public PlayerData? FindByUsername(string username)
    {
        return _players.Values.FirstOrDefault(
            player => player.Username == username);
    }

    public bool TryUpdate(
        PlayerData currentPlayer,
        PlayerData updatedPlayer)
    {
        return _players.TryUpdate(
            currentPlayer.Id,
            updatedPlayer,
            currentPlayer);
    }
}