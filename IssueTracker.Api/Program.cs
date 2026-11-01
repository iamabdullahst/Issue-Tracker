using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<IssueDb>(
    options => options.UseSqlite("Data Source=issues.db"));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IssueDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/issues", async (IssueDb db) =>
    await db.Issues.ToListAsync());

app.MapGet("/issues/{id:int}", async (int id, IssueDb db) =>
{
    var issue = await db.Issues.FindAsync(id);
    return issue is null ? Results.NotFound() : Results.Ok(issue);
});

app.MapPost("/issues", async (NewIssue request, IssueDb db) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest("Title is required.");

    var issue = new Issue { Title = request.Title.Trim() };
    db.Issues.Add(issue);
    await db.SaveChangesAsync();

    return Results.Created($"/issues/{issue.Id}", issue);
});

app.MapPatch("/issues/{id:int}/close", async (int id, IssueDb db) =>
{
    var issue = await db.Issues.FindAsync(id);
    if (issue is null)
        return Results.NotFound();

    issue.Status = "Closed";
    await db.SaveChangesAsync();
    return Results.Ok(issue);
});

app.Run();

record NewIssue(string Title);

public class Issue
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Open";
}

public class IssueDb : DbContext
{
    public IssueDb(DbContextOptions<IssueDb> options) : base(options) { }
    public DbSet<Issue> Issues => Set<Issue>();
}