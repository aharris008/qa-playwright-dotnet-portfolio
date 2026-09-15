using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace QaPlaywrightPortfolio.Tests.Pages;

public sealed class TodoPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public TodoPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public ILocator NewTodoInput =>
        _page.GetByPlaceholder("What needs to be done?");

    public ILocator TodoItems => _page.GetByTestId("todo-item");

    public ILocator RemainingCount => _page.GetByTestId("todo-count");

    public Task NavigateAsync() => _page.GotoAsync(_baseUrl);

    public Task AddTodoAsync(string todoText) => SubmitTodoAsync(todoText);

    public async Task AddTodosAsync(params string[] todoTexts)
    {
        foreach (var todoText in todoTexts)
        {
            await AddTodoAsync(todoText);
        }
    }

    public async Task SubmitTodoAsync(string todoText)
    {
        await NewTodoInput.FillAsync(todoText);
        await NewTodoInput.PressAsync("Enter");
    }

    public Task CompleteTodoAsync(string todoText) =>
        TodoCheckbox(todoText).CheckAsync();

    public Task SelectFilterAsync(string filterName) =>
        _page.GetByRole(AriaRole.Link, new PageGetByRoleOptions
        {
            Exact = true,
            Name = filterName,
        }).ClickAsync();

    public ILocator TodoItem(string todoText) =>
        TodoItems.Filter(new LocatorFilterOptions { Has = TodoTitle(todoText) });

    public ILocator TodoTitle(string todoText) =>
        _page.GetByTestId("todo-title").Filter(new LocatorFilterOptions
        {
            HasTextRegex = new Regex($"^{Regex.Escape(todoText)}$"),
        });

    public ILocator TodoCheckbox(string todoText) =>
        TodoItem(todoText).GetByRole(AriaRole.Checkbox);
}
