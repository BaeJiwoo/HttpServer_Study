using Microsoft.AspNetCore.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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

    return Results.Ok( new
    {
        message = "로그인에 성공했습니다.",
        playerId = 1,
        username = request.Username
    });

});

app.Run();

record LoginRequest (
    string Username,
    string Password);