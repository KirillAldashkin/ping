var repo = args.Length > 0 ? args[0] : Console.ReadLine();
var token = args.Length > 1 ? args[1] : Console.ReadLine();

var runsUrl = $"https://api.github.com/repos/{repo}/actions/runs?per_page=100";
string deleteUrl(int id) => $"https://api.github.com/repos/{repo}/actions/runs/{id}";

HttpClient client = new();
client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
client.DefaultRequestHeaders.Add("User-Agent", "Actions cleanup utility");

static async Task<T> Get<T>(HttpClient client, string url) =>
    System.Text.Json.JsonSerializer.Deserialize<T>(await client.GetStringAsync(url));

while (true)
{
    bool any = false;
    var workflows = await Get<Workflows>(client, runsUrl);
    Console.WriteLine($">>>> {workflows.total_count} workflow runs remains");
    foreach(var run in workflows.workflow_runs)
    {
        if (run.status != "completed")
            Console.WriteLine($"<< Skipping: {run}");
        else
        {
            any = true;
            Console.WriteLine($"<< Deleting: {run}");
            var response = await client.DeleteAsync(deleteUrl(run.id));
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($">> Code {(int)response.StatusCode}: {content}");
            if (((int)response.StatusCode) / 100 != 2) throw new("Response error");
        }
    }
    if (!any) break;
}

record struct Workflows(int total_count, Workflow[] workflow_runs);
record struct Workflow(int id, string name, string created_at, string status);