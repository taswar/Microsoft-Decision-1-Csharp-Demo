# Contributing

Thanks for helping improve this sample. It's a small .NET 10 console app that sends a support ticket to Microsoft-Decision-1 through the `systemone` API and prints a team choice, a severity score and a repeat-contact probability. Contributions that make it clearer, more correct, or more useful to other .NET developers are welcome.

## Ways to contribute

- **Report a bug.** The app fails, or the README doesn't match what the code does.
- **Fix the docs.** Typos, unclear steps, or a missing troubleshooting case (for example, an error code you hit that isn't listed).
- **Improve the sample.** For example, a new question on the same ticket, or a clearer way to handle a low-confidence answer.
- **Share results.** If you ran the questions against your own tickets, an issue describing what you saw helps others.

For anything larger than a small fix, open an issue first so we can agree on the approach before you write code.

## Set up

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and the Azure CLI.
2. Clone the repo and build it:

   ```bash
   git clone https://github.com/taswar/Microsoft-Decision-1-Csharp-Demo.git
   cd Microsoft-Decision-1-Csharp-Demo
   dotnet build
   ```

3. To run it, deploy Microsoft-Decision-1 on a Microsoft Foundry resource, set `AZURE_AI_ENDPOINT` (and `AZURE_AI_DEPLOYMENT` if your deployment isn't named `microsoft-decision-1`) with user secrets, and sign in with `az login`, as described in the [README](README.md#configuration). The sample uses Entra ID through `DefaultAzureCredential`, with no API key.

## Guidelines

- **Never commit secrets.** No keys, endpoints, tenant IDs or tokens in code, config files or screenshots. Settings belong in user secrets.
- **Keep `StringContent`.** The API rejects chunked requests with `400: no_content_length_header`. Don't switch the request body to `JsonContent`.
- **Keep the base URL rule.** `AZURE_AI_ENDPOINT` is the resource URL with no path. The app appends `/providers/microsoft/v1/systemone`.
- **Keep the human fallback.** The app suggests a person when the team probability is below 70%. Keep a fallback like this in any new question, or explain why it isn't needed.
- **Keep it a sample.** The code should stay short and readable in one `Program.cs`. Prefer clarity over abstraction, and add a dependency only when it clearly helps.
- **Match the existing style.** Target `net10.0`, keep nullable reference types on, and follow the naming and records already in `Program.cs`.
- **Keep the README in sync.** If you add a question, change settings or change output, update the README in the same pull request.
- **One change per pull request.** Small, focused pull requests are easier to review and merge.

## Pull request checklist

- [ ] `dotnet build` succeeds with no new warnings.
- [ ] The app runs against your own Microsoft-Decision-1 deployment, or you explain why you couldn't run it.
- [ ] No secrets, endpoints or personal data are in the diff.
- [ ] The README reflects any change in questions, settings or output.
- [ ] The pull request description says what changed and why.

The build workflow runs on every pull request and has to pass before a merge.

## Reporting a bug

Open an issue and include:

- What you expected and what happened.
- The full error message, with any endpoints removed. For a `KeyNotFoundException` or a missing answer, include the raw answer JSON the app prints.
- Your .NET SDK version (`dotnet --version`) and operating system.
- Your deployment name and deployment type (`GlobalStandard` or `DataZoneStandard`).

## Security issues

Please don't report security problems in a public issue. Use **Security → Report a vulnerability** on the repo to send a private report.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE) that covers this project.
