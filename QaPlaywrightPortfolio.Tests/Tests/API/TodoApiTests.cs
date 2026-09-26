using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using QaPlaywrightPortfolio.Tests.Fixtures;
using QaPlaywrightPortfolio.Tests.Models;

namespace QaPlaywrightPortfolio.Tests.API;

[TestFixture]
[Category("API")]
public class TodoApiTests : PlaywrightTest
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private LocalTodoApi _api = null!;
    private IAPIRequestContext _request = null!;

    [OneTimeSetUp]
    public async Task StartApi() => _api = await LocalTodoApi.StartAsync();

    [SetUp]
    public async Task CreateRequestContext() =>
        _request = await Playwright.APIRequest.NewContextAsync(new() { BaseURL = _api.BaseUrl });

    [TearDown]
    public async Task DisposeRequestContext() => await _request.DisposeAsync();

    [OneTimeTearDown]
    public async Task StopApi()
    {
        if (_api is not null)
        {
            await _api.DisposeAsync();
        }
    }

    [Test]
    public async Task GetTodo_ReturnsExpectedResource()
    {
        var response = await _request.GetAsync("/todos/1");

        Assert.That(response.Status, Is.EqualTo(200));
        Assert.That(response.Headers["content-type"], Does.StartWith("application/json"));
        var todo = JsonSerializer.Deserialize<TodoResponse>(await response.TextAsync(), JsonOptions);
        Assert.That(todo, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(todo!.Id, Is.EqualTo(1));
            Assert.That(todo.UserId, Is.EqualTo(7));
            Assert.That(todo.Title, Is.EqualTo("Review API contract"));
            Assert.That(todo.Completed, Is.False);
        }
    }

    [Test]
    public async Task GetTodos_FilterByUser_ReturnsOnlyMatchingResources()
    {
        var response = await _request.GetAsync("/todos?userId=7");

        Assert.That(response.Status, Is.EqualTo(200));
        var todos = JsonSerializer.Deserialize<TodoResponse[]>(await response.TextAsync(), JsonOptions);
        Assert.That(todos, Is.Not.Null.And.Not.Empty);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(todos!.Select(todo => todo.Id), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(todos.All(todo => todo.UserId == 7), Is.True);
        }
    }

    [Test]
    public async Task GetUnknownTodo_ReturnsNotFound()
    {
        var response = await _request.GetAsync("/todos/999");

        Assert.That(response.Status, Is.EqualTo(404));

        using var body = JsonDocument.Parse(await response.TextAsync());
        Assert.That(
            body.RootElement.GetProperty("code").GetString(),
            Is.EqualTo("todo_not_found"));
    }
}
