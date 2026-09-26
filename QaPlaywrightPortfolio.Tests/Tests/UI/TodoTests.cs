using System.Text.RegularExpressions;
using QaPlaywrightPortfolio.Tests.Configuration;
using QaPlaywrightPortfolio.Tests.Fixtures;
using QaPlaywrightPortfolio.Tests.Pages;

namespace QaPlaywrightPortfolio.Tests.UI;

[TestFixture]
[Category("UI")]
public class TodoTests : UiTestBase
{
    private TodoPage _todoPage = null!;

    [SetUp]
    public async Task OpenTodoApp()
    {
        _todoPage = new TodoPage(Page, TestSettings.UiBaseUrl);
        await _todoPage.NavigateAsync();
    }

    [Test]
    [Category("Smoke")]
    public async Task AddTodo_DisplaysNewItem()
    {
        const string todoText = "Review Playwright locators";

        await _todoPage.AddTodoAsync(todoText);

        await Expect(_todoPage.TodoItems).ToHaveCountAsync(1);
        await Expect(_todoPage.TodoTitle(todoText)).ToBeVisibleAsync();
        await Expect(_todoPage.RemainingCount).ToHaveTextAsync("1 item left");
    }

    [Test]
    [Category("Regression")]
    public async Task CompleteTodo_UpdatesStatusAndRemainingCount()
    {
        const string completedTodo = "Review regression results";
        const string activeTodo = "Document failure diagnostics";

        await _todoPage.AddTodosAsync(completedTodo, activeTodo);
        await _todoPage.CompleteTodoAsync(completedTodo);

        await Expect(_todoPage.TodoItem(completedTodo))
            .ToHaveClassAsync(new Regex(@"\bcompleted\b"));
        await Expect(_todoPage.TodoCheckbox(completedTodo)).ToBeCheckedAsync();
        await Expect(_todoPage.TodoCheckbox(activeTodo)).Not.ToBeCheckedAsync();
        await Expect(_todoPage.RemainingCount).ToHaveTextAsync("1 item left");
    }

    [Test]
    [Category("Regression")]
    public async Task CompletedFilter_ShowsOnlyCompletedTodos()
    {
        const string completedTodo = "Verify completed filter";
        const string activeTodo = "Verify active filter";

        await _todoPage.AddTodosAsync(completedTodo, activeTodo);
        await _todoPage.CompleteTodoAsync(completedTodo);
        await _todoPage.SelectFilterAsync("Completed");

        await Expect(_todoPage.TodoItems).ToHaveCountAsync(1);
        await Expect(_todoPage.TodoTitle(completedTodo)).ToBeVisibleAsync();
        await Expect(_todoPage.TodoTitle(activeTodo)).ToBeHiddenAsync();
    }

    [Test]
    [Category("Boundary")]
    [Category("Negative")]
    public async Task EmptyTodo_IsNotAdded()
    {
        await _todoPage.SubmitTodoAsync(string.Empty);

        await Expect(_todoPage.TodoItems).ToHaveCountAsync(0);
        await Expect(_todoPage.NewTodoInput).ToBeEmptyAsync();
    }
}
