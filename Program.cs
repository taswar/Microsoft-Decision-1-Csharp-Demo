using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var endpoint = (config["AZURE_AI_ENDPOINT"]
    ?? throw new InvalidOperationException("Set AZURE_AI_ENDPOINT."))
    .TrimEnd('/');
var deployment = config["AZURE_AI_DEPLOYMENT"] ?? "microsoft-decision-1";

var credential = new DefaultAzureCredential();
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

var decision = await DecideAsync(
    "Checkout is down again for all EU customers. This is my third request since 09:00.");

Console.WriteLine($"Team:     {decision.Team.Choice}  (p={decision.Team.Probability:P0})");
Console.WriteLine($"Severity: {decision.Severity.Score:0.00}  level={decision.Severity.Level}");
Console.WriteLine($"Repeat:   {decision.Repeat.Probability:P0}");

if (decision.Team.Probability < 0.70)
{
    Console.WriteLine("Top label is weak — send this ticket to a person.");
}

async Task<TicketDecision> DecideAsync(string state)
{
    var token = await credential.GetTokenAsync(
        new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]),
        CancellationToken.None);

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"{endpoint}/providers/microsoft/v1/systemone");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    var json = JsonSerializer.Serialize(new
    {
        model = deployment,
        state,
        questions = new
        {
            team = new
            {
                type = "choice",
                instructions = "Which team should handle this request? Pick the primary problem.",
                criteria = new Dictionary<string, string>
                {
                    ["billing"] = "Charges, invoices, refunds, or subscription payments",
                    ["engineering"] = "Bugs, errors, outages, or integration failures",
                    ["support"] = "How-to questions, passwords, and account access"
                }
            },
            severity = new
            {
                type = "score",
                instructions = "How severe is this incident?",
                criteria = new[] { "Cosmetic", "Minor", "Major", "Critical" }
            },
            repeat_contact = new
            {
                type = "noul",
                instructions = "Has the customer contacted support about this before?",
                criteria = new Dictionary<string, string>
                {
                    ["true"] = "The state says this is a repeat or follow-up",
                    ["false"] = "Nothing in the state indicates an earlier contact"
                }
            }
        }
    });
    // StringContent sets a fixed Content-Length; JsonContent uses chunked encoding.
    request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    using var response = await http.SendAsync(request);
    var body = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException(
            $"Decision API returned {(int)response.StatusCode}: {body}");
    }

    using var doc = JsonDocument.Parse(body);
    var answers = doc.RootElement.GetProperty("answers");
    return new TicketDecision(
        ReadChoice(answers.GetProperty("team")),
        ReadScore(answers.GetProperty("severity")),
        ReadNoul(answers.GetProperty("repeat_contact")));
}

static ChoiceAnswer ReadChoice(JsonElement answer)
{
    var probs = answer.GetProperty("probabilities");
    var choice = answer.GetProperty("choice").GetString()
        ?? throw new InvalidOperationException("Missing choice.");
    return new ChoiceAnswer(choice, probs.GetProperty(choice).GetDouble(), probs);
}

static ScoreAnswer ReadScore(JsonElement answer) =>
    new(answer.GetProperty("score").GetDouble(),
        answer.TryGetProperty("level", out var level) ? level.GetString() : null);

static NoulAnswer ReadNoul(JsonElement answer) =>
    new(GetRequiredDouble(answer, "noul"));

// Fails with the raw answer JSON so the real response shape is visible.
static double GetRequiredDouble(JsonElement answer, string name)
{
    if (answer.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
    {
        return value.GetDouble();
    }
    throw new InvalidOperationException(
        $"Answer has no numeric '{name}'. Raw answer: {answer.GetRawText()}");
}

record ChoiceAnswer(string Choice, double Probability, JsonElement Probabilities);
record ScoreAnswer(double Score, string? Level);
record NoulAnswer(double Probability);
record TicketDecision(ChoiceAnswer Team, ScoreAnswer Severity, NoulAnswer Repeat);