using System.Collections.Concurrent;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var loginSessions = new ConcurrentDictionary<string, LoginSession>();

var players = new ConcurrentDictionary<int, PlayerData>();

players[1] = new PlayerData(
    Id: 1,
    Username: "player1234",
    Nickname: "knight",
    Level: 1,
    Gold: 1000
);

app.MapGet("/", () =>
{
    return Results.Ok(
        new
        {
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
            statusCode: StatusCodes.Status401Unauthorized);
    }

    var accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    var expiresAt = DateTime.UtcNow.AddMinutes(10);

    var session = new LoginSession(PlayerId: testPlayerId, ExpiresAt: expiresAt);

    loginSessions[accessToken] = session;

    return Results.Ok(new
    {
        message = "로그인에 성공했습니다.",
        accessToken,
        username = request.Username,
        expiresAt
    });

});

app.MapGet("/me", (HttpRequest request) =>
{
    var authResult = Authenticate(request);

    if (!authResult.Success)
    {
        return Results.Json(new
        {
            message = authResult.ErrorMessage
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    if (!players.TryGetValue(authResult.PlayerId, out var player))
    {
        return Results.NotFound(new
        {
            message = "플레이어 정보를 찾을 수 없습니다."
        });
    }

    return Results.Ok(new
    {
        id = player.Id,
        username = player.Username,
        nickname = player.Nickname,
        level = player.Level,
        gold = player.Gold,
    });
});

app.MapPost("/stages/{stageId:int}/complete", (int stageId, HttpRequest request) =>
{
    var authResult = Authenticate(request);

    if (!authResult.Success)
    {
        return Results.Json(new
        {
            message = authResult.ErrorMessage
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    var rewardGold = stageId switch
    {
        1 => 100,
        2 => 200,
        3 => 300,
        _ => 0
    };

    if (rewardGold == 0)
    {
        return Results.NotFound(new
        {
            message = "존재하지 않는 스테이지입니다."
        });
    }

    PlayerData updatedPlayer;
    
    while(true)
    {
        if(!players.TryGetValue(authResult.PlayerId, out var currentPlayer))
        {
            return Results.NotFound( new
            {
                message = "플레이어 정보를 찾을 수 없습니다."
            });
        }

        updatedPlayer = currentPlayer with
        {
            Gold = checked(currentPlayer.Gold + rewardGold)
        };

        if(players.TryUpdate(currentPlayer.Id, updatedPlayer, currentPlayer))
        {
            break;
        }
    }

    return Results.Ok(new
    {
        message = "스테이지 보상을 받았습니다.",
        stageId,
        rewardGold,
        currentGold = updatedPlayer.Gold
    });
});

app.MapPost("/logout", (HttpRequest request) =>
{
    var authResult = Authenticate(request);

    if (!authResult.Success)
    {
        return Results.Json(new
        {
            message = authResult.ErrorMessage
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    loginSessions.TryRemove(authResult.AccessToken!, out _);

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

    if (!authorization.StartsWith(bearerPrefix,
    StringComparison.OrdinalIgnoreCase))
    {
        return new TokenResult(
            Success: false,
            AccessToken: null,
            ErrorMessage: "인증 토큰이 필요합니다."
        );
    }

    var accessToken = authorization[bearerPrefix.Length..].Trim();

    if (string.IsNullOrWhiteSpace(accessToken))
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

AuthResult Authenticate(HttpRequest request)
{
    var tokenResult = GetAccessToken(request);

    if (!tokenResult.Success)
    {
        return new AuthResult(
            Success: false,
            AccessToken: null,
            PlayerId: 0,
            ErrorMessage: "인증 토큰이 필요합니다."
        );
    }

    var accessToken = tokenResult.AccessToken!;

    if (!loginSessions.TryGetValue(accessToken, out var session))
    {
        return new AuthResult(
            Success: false,
            AccessToken: null,
            PlayerId: 0,
            ErrorMessage: "유효하지 않은 인증 토큰입니다."
        );
    }

    if (session.ExpiresAt <= DateTime.UtcNow)
    {
        loginSessions.TryRemove(accessToken, out _);

        return new AuthResult(
            Success: false,
            AccessToken: accessToken,
            PlayerId: 0,
            ErrorMessage: "인증 토큰이 만료되었습니다."
        );
    }

    return new AuthResult(
        Success: true,
        AccessToken: accessToken,
        PlayerId: session.PlayerId,
        ErrorMessage: null
    );
}

record LoginRequest(
    string Username,
    string Password);

record LoginSession(
    int PlayerId,
    DateTime ExpiresAt
);

record PlayerData(
    int Id,
    string Username,
    string Nickname,
    int Level,
    int Gold
);

record TokenResult(
    bool Success,
    string? AccessToken,
    string? ErrorMessage
);

record AuthResult(
    bool Success,
    string? AccessToken,
    int PlayerId,
    string? ErrorMessage
);