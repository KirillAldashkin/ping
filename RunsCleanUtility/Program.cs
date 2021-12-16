var repo = args.Length > 0 ? args[0] : Console.ReadLine();
var token = args.Length > 1 ? args[1] : Console.ReadLine();

string runsUrl(int page) => $"https://api.github.com/repos/{repo}/actions/runs?per_page=100&page={page}";
string deleteUrl(int id) => $"https://api.github.com/repos/{repo}/actions/runs/{id}/logs";

HttpClient client = new();
client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
client.DefaultRequestHeaders.Add("User-Agent", "Actions cleanup utility");

static async Task<T> Get<T>(HttpClient client, string url) =>
    System.Text.Json.JsonSerializer.Deserialize<T>(await client.GetStringAsync(url));

int page = 0;
while (true)
{
    var workflows = await Get<Workflows>(client, runsUrl(++page));
    if (workflows.workflow_runs.Length == 0) break;
    Console.WriteLine($">>>> [Page {page}] {workflows.total_count} workflow runs remains");
    foreach(var run in workflows.workflow_runs)
    {
        Console.WriteLine($"<< Deleting: {run}");
        var response = await client.DeleteAsync(deleteUrl(run.id));
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($">> Code {(int)response.StatusCode}: {content}");
    }
}

record struct Workflows(int total_count, Workflow[] workflow_runs);
record struct Workflow(int id, string name, string created_at);