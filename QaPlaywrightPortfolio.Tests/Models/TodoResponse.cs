namespace QaPlaywrightPortfolio.Tests.Models;

public sealed record TodoResponse
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string Title { get; init; }
    public required bool Completed { get; init; }
}
