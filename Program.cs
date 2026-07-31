using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var loginSessions = new ConcurrentDictionary<string, LoginSession>();

app.MapGet("/", () => 
{
    return Results.Ok(
        new {
            message = "Game Server is running",
            serverTime = DateTime.UtcNow
        }
    );
});

app.MapPost("/login", (LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new
        {
            message = "아이디와 비밀번호를 모두 입력해야 합니다."
        });
    }

    const string testUsername = "player1234";
    const string testPassword = "1234";
    const int testPlayerId = 1;

    if (request.Username != testUsername ||
        request.Password != testPassword)
    {
        return Results.Json(
            new
            {
                message = "아이디 또는 비밀번호가 올바르지 않습니다."
            },
            statusCode:StatusCodes.Status401Unauthorized);
    }

    var accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    var expiresAt = DateTime.UtcNow.AddMinutes(10);

    var session = new LoginSession(playerId: testPlayerId, ExpiresAt: expiresAt);

    loginSessions[accessToken] = session;

    return Results.Ok( new
    {
        message = "로그인에 성공했습니다.",
        accessToken,
        username = request.Username,
        expiresAt
    });

});

app.MapGet("/me", (HttpRequest request) =>
{
    

    var tokenResult = GetAccessToken(request);

    if(!tokenResult.Success)
    {
        return Results.Json(new
        {
            message = tokenResult.ErrorMessage
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    var accessToken = tokenResult.AccessToken!;

    if(!loginSessions.TryGetValue(
        accessToken,
        out var session))
    {
        return Results.Json(
            new
            {
                message = "유효하지 않은 인증 토큰입니다."
            },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    if(session.ExpiresAt <= DateTime.UtcNow)
    {
        loginSessions.TryRemove(accessToken, out _);

        return Results.Json(new
        {
            message = "인증 토큰이 만료되었습니다."
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.Ok(new
    {
        id = session.playerId,
        username = "player1234",
        nickname = "Knight",
        level = 1,
        gold = 1000,
        tokenExpiresAt = session.ExpiresAt
    });
});

app.MapPost("/logout", (HttpRequest request) =>
{
    var tokenResult = GetAccessToken(request);

    if(!tokenResult.Success)
    {
        return Results.Json(new
        {
            message = tokenResult.ErrorMessage
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    var accessToken = tokenResult.AccessToken!;

    if(!loginSessions.TryRemove(accessToken, out _))
    {
        return Results.Json(
            new
            {
                message = "유효하지 않은 인증 토큰입니다."
            },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.Ok(new
    {
        message = "로그아웃되었씁니다."
    });
});

app.Run();

static TokenResult GetAccessToken(HttpRequest request)
{
    var authorization = request.Headers.Authorization.ToString();

    const string bearerPrefix = "Bearer ";

    if(!authorization.StartsWith(bearerPrefix, 
    StringComparison.OrdinalIgnoreCase))
    {
        return new TokenResult(
            Success: false,
            AccessToken: null,
            ErrorMessage: "인증 토큰이 필요합니다."
        );
    }

    var accessToken = authorization[bearerPrefix.Length..].Trim();

    if(string.IsNullOrWhiteSpace(accessToken))
    {
        return new TokenResult(
            Success: false,
            AccessToken: null,
            ErrorMessage: "인증 토큰이 비어 있습니다."
        );
    }

    return new TokenResult(
        Success: true,
        AccessToken: accessToken,
        ErrorMessage: null
    );
}

record LoginRequest (
    string Username,
    string Password);

record LoginSession (
    int playerId,
    DateTime ExpiresAt
);

record TokenResult(
    bool Success,
    string? AccessToken,
    string? ErrorMessage
);