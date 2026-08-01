using Gameserver.Api.Repositories;
using GameServer.Api.Models;
using GameServer.Api.Repositories;
using GameServer.Api.Services;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var sessionRepository = new LoginSessionRepository();

var playerRepository = new PlayerRepository();

var authService = new AuthService(sessionRepository);

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

    const string testPassword = "1234";

    var player =
        playerRepository.FindByUsername(request.Username);

    if (player is null || request.Password != testPassword)
    {
        return Results.Json(
            new
            {
                message = "아이디 또는 비밀번호가 올바르지 않습니다."
            },
            statusCode: StatusCodes.Status401Unauthorized);
    }
    
    var loginResult = authService.CreateSession(player.Id);

    return Results.Ok(new
    {
        message = "로그인에 성공했습니다.",
        accessToken = loginResult.AccessToken,
        tokenType = "Bearer ",
        username = request.Username,
        expiresAt = loginResult.ExpiresAt
    });

});

app.MapGet("/me", (HttpRequest request) =>
{
    var authResult = authService.Authenticate(request);

    if (!authResult.Success)
    {
        return Results.Json(new
        {
            message = authResult.ErrorMessage
        },
        statusCode: StatusCodes.Status401Unauthorized);
    }

    var player =
        playerRepository.FindById(authResult.PlayerId);

    if (player is null)
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
    var authResult = authService.Authenticate(request);

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
         var currentPlayer =
            playerRepository.FindById(authResult.PlayerId);
        if(currentPlayer is null)
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

        if(playerRepository.TryUpdate(currentPlayer, updatedPlayer))
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
    var authResult = authService.Authenticate(request);

    if (!authResult.Success)
    {
        return Unauthorized(
            authResult.ErrorMessage);
    }

    var removed = authService.Logout(authResult.AccessToken!);

    if(!removed)
    {
        return Unauthorized(
            "유효하지 않은 인증 토큰입니다."
        );
    }

    return Results.Ok(new
    {
        message = "로그아웃되었씁니다."
    });
});

app.Run();

static IResult Unauthorized(string? message)
{
    return Results.Json(
        new
        {
            message
        },
        statusCode:StatusCodes.Status401Unauthorized);
}