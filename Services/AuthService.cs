using System.Security.Cryptography;
using GameServer.Api.Models;
using GameServer.Api.Repositories;

namespace GameServer.Api.Services;

public class AuthService
{
    private readonly LoginSessionRepository _sessionRepository;

    public AuthService(
        LoginSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public (string AccessToken, DateTime ExpiresAt) CreateSession(
        int playerId)
    {
        var accessToken = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var session = new LoginSession(
            PlayerId: playerId,
            ExpiresAt: expiresAt);

        _sessionRepository.Save(
            accessToken,
            session);

        return (
            AccessToken: accessToken,
            ExpiresAt: expiresAt);
    }

    public AuthResult Authenticate(HttpRequest request)
    {
        var authorization = request.Headers.Authorization.ToString();

        const string bearerPrefix = "Bearer ";

        if(!authorization.StartsWith(
            bearerPrefix,
            StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                "인증 토큰이 필요합니다.");
        }

        var accessToken = authorization[bearerPrefix.Length..].Trim();

        if(string.IsNullOrWhiteSpace(accessToken))
        {
            return Failure(
                "인증 토큰이 비어 있습니다.");
        }

        var session = _sessionRepository.Find(accessToken)!;
        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            _sessionRepository.Remove(accessToken);

            return Failure(
                "인증 토큰이 만료되었습니다.",
                accessToken);
        }

        return new AuthResult(
            Success: true,
            AccessToken: accessToken,
            PlayerId: session.PlayerId,
            ErrorMessage: null);
    }

    public bool Logout(string accessToken)
    {
        return _sessionRepository.Remove(accessToken);
    }

    private static AuthResult Failure(
        string errorMessage,
        string? accessToken = null)
    {
        return new AuthResult(
            Success: false,
            AccessToken: accessToken,
            PlayerId: 0,
            ErrorMessage: errorMessage);
    }

}