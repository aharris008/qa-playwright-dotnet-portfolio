using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace QaPlaywrightPortfolio.Tests.Fixtures;

public sealed class LocalTodoApi : IAsyncDisposable
{
    private readonly WebApplication _app;

    private LocalTodoApi(WebApplication app, string baseUrl)[Test]
public async Task GetUnknownTodo_ReturnsNotFound()
{
    var response = await _request.GetAsync("/todos/999");

    Assert.That(response.Status, Is.EqualTo(404));

    using var body = JsonDocument.Parse(await response.TextAsync());
    Assert.That(
        body.RootElement.GetProperty("code").GetString(),
        Is.EqualTo("todo_not_found"));
}
    {
        _app = app;
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; }

    public static async Task<LocalTodoApi> StartAsync()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            Args = [],
        });
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        var app = builder.Build();

        // Server data deliberately does not share the client's response model or expectations.
        var todos = new[]
        {
            new { id = 1, userId = 7, title = "Review API contract", completed = false },
            new { id = 2, userId = 7, title = "Document error responses", completed = true },
            new { id = 3, userId = 9, title = "Check filtering", completed = false },
        };

        app.MapGet("/todos/{id:int}", (int id) =>
        {
            var todo = todos.SingleOrDefault(item => item.id == id);
            return todo is null
                ? Results.NotFound(new { code = "todo_not_found" })
                : Results.Ok(todo);
        });
        app.MapGet("/todos", (int? userId) =>
            Results.Ok(todos.Where(item => userId is null || item.userId == userId)));

        try
        {
            await app.StartAsync();
            var server = app.Services.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            return new LocalTodoApi(app, address);
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _app.StopAsync();
        }
        finally
        {
            await _app.DisposeAsync();
        }
    }
}
