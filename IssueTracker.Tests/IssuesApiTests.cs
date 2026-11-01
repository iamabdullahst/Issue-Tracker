using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IssueTracker.Tests;

public class IssuesApiTests
{
    [Fact]
    public async Task Blank_title_is_rejected_without_creating_an_issue()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"issue-tracker-test-{Guid.NewGuid():N}.db");
        try
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                    builder.UseSetting("ConnectionStrings:Issues", $"Data Source={dbPath}"));
            using var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/issues", new { title = "  " });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var issues = await client.GetFromJsonAsync<List<Issue>>("/issues");
            Assert.NotNull(issues);
            Assert.Empty(issues);
        }
        finally
        {
            File.Delete(dbPath);
        }
    }
    [Fact]
    public async Task Issue_can_be_created_read_and_closed()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"issue-tracker-test-{Guid.NewGuid():N}.db");
        try
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                    builder.UseSetting("ConnectionStrings:Issues", $"Data Source={dbPath}"));
            using var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/issues", new { title = "  Fix login  " });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<Issue>();
            Assert.NotNull(created);
            Assert.Equal("Fix login", created.Title);
            Assert.Equal("Open", created.Status);

            var found = await client.GetFromJsonAsync<Issue>($"/issues/{created.Id}");
            Assert.NotNull(found);
            Assert.Equal(created.Id, found.Id);

            var closeResponse = await client.PatchAsync($"/issues/{created.Id}/close", null);
            Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);

            var closed = await client.GetFromJsonAsync<Issue>($"/issues/{created.Id}");
            Assert.NotNull(closed);
            Assert.Equal("Closed", closed.Status);
        }
        finally
        {
            File.Delete(dbPath);
        }
    }
}
