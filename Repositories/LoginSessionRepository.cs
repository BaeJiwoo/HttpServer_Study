using System.Collections.Concurrent;
using GameServer.Api.Models;

namespace GameServer.Api.Repositories;

public class LoginSessionRepository
{
    private readonly ConcurrentDictionary<string, LoginSession> _sessions = new();

    public void Save(
        string accessToken,
        LoginSession session
    )
    {
        _sessions[accessToken] = session;
    }

    public LoginSession? Find(string accessToken)
    {
        return _sessions.TryGetValue(
            accessToken, 
            out var session)
            ? session : null;
    }

    public bool  Remove(string accessToken)
    {
        return _sessions.TryRemove(
            accessToken,
            out _);
    }
}