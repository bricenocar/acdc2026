using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MinecraftFunctions.Helpers;
using MinecraftFunctions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MinecraftFunctions.Services;

public class DataverseService
{
    private static readonly HttpClient HttpClient = new();

    private readonly ILogger _logger;

    private readonly string _dataverseUrl;
    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _clientSecret;

    private string? _accessToken;
    private DateTime _tokenExpiry;

    public DataverseService(ILogger logger)
    {
        _logger = logger;

        _dataverseUrl = Environment.GetEnvironmentVariable("DATAVERSE_URL");
        _tenantId = Environment.GetEnvironmentVariable("DV_TENANT_ID");
        _clientId = Environment.GetEnvironmentVariable("DV_CLIENT_ID");
        _clientSecret = Environment.GetEnvironmentVariable("DV_CLIENT_SECRET");
    }

    public async Task UpdateDataverseIfNeeded(GameEvent e)
    {
        switch (e.EventType)
        {
            case "MobKilled":
            case "PlayerKilled":
            case "PlayerDied":
            case "BlockPlaced":
            case "BlockBroken":
                await UpdatePlayerStatsAsync(e);
                break;
        }
    }

    private async Task UpdatePlayerStatsAsync(GameEvent e)
    {
        await EnsureAccessTokenAsync();
        SetDataverseHeaders();

        string playerName = e.PlayerName.ToLowerInvariant();
        string playerId = await GetOrCreatePlayerAsync(playerName);

        string queryUrl =
            $"{_dataverseUrl}/api/data/v9.2/cred7_playerstatses" +
            $"?$select=cred7_playerstatsid,cred7_mc_kills,cred7_mc_mobskilled,cred7_mc_playerskilled,cred7_mc_blocksplaced,cred7_mc_credits,cred7_mc_deaths,_cred7_mc_player_value,_cred7_mc_match_value" +
            $"&$filter=_cred7_mc_player_value eq {playerId}";

        var resp = await HttpClient.GetAsync(queryUrl);
        await ValidateResponse(resp, "GET player stats");

        JObject json = JObject.Parse(await resp.Content.ReadAsStringAsync());

        JObject entity;
        string statsId;

        if (json["value"]!.Any())
        {
            entity = (JObject)json["value"]![0]!;
            statsId = entity["cred7_playerstatsid"]!.ToString();
        }
        else
        {
            string? activeMatchId = await GetFirstActiveMatchAsync();

            entity = new JObject
            {
                ["cred7_mc_player@odata.bind"] = $"/cred7_players({playerId})",
                ["cred7_mc_kills"] = e.EventType == "MobKilled" ? 1 : 0,
                ["cred7_mc_mobskilled"] = e.EventType == "MobKilled" ? 1 : 0,
                ["cred7_mc_playerskilled"] = e.EventType == "PlayerKilled" ? 1 : 0,
                ["cred7_mc_blocksplaced"] = e.EventType == "BlockPlaced" ? 1 : 0,
                ["cred7_mc_credits"] = CreditCalculator.CalculateCredits(e),
                ["cred7_mc_deaths"] = e.EventType == "PlayerDied" ? 1 : 0
            };

            if (!string.IsNullOrEmpty(activeMatchId))
                entity["cred7_mc_match@odata.bind"] = $"/cred7_matchs({activeMatchId})";

            var createResp = await HttpClient.PostAsync(
                $"{_dataverseUrl}/api/data/v9.2/cred7_playerstatses",
                new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json"));

            await ValidateResponse(createResp, "POST create player stats");

            statsId = createResp.Headers.Location!.Segments.Last();
        }

        bool hasMatch = entity["_cred7_mc_match_value"] != null &&
                        entity["_cred7_mc_match_value"]!.Type == JTokenType.String &&
                        !string.IsNullOrWhiteSpace(entity["_cred7_mc_match_value"]!.ToString());

        if (!hasMatch)
        {
            string? matchId = await GetFirstActiveMatchAsync();
            if (!string.IsNullOrEmpty(matchId))
            {
                JObject matchPatch = new()
                {
                    ["cred7_mc_match@odata.bind"] = $"/cred7_matchs({matchId})"
                };

                var matchResp = await HttpClient.PatchAsync(
                    $"{_dataverseUrl}/api/data/v9.2/cred7_playerstatses({statsId})",
                    new StringContent(JsonConvert.SerializeObject(matchPatch), Encoding.UTF8, "application/json"));

                await ValidateResponse(matchResp, "PATCH match stats");
            }
        }

        JObject patch = ApplyGameLogic(entity, e);

        var patchResp = await HttpClient.PatchAsync(
            $"{_dataverseUrl}/api/data/v9.2/cred7_playerstatses({statsId})",
            new StringContent(JsonConvert.SerializeObject(patch), Encoding.UTF8, "application/json"));

        await ValidateResponse(patchResp, "PATCH update stats");
    }

    private async Task<string> GetOrCreatePlayerAsync(string playerName)
    {
        await EnsureAccessTokenAsync();
        SetDataverseHeaders();

        string queryUrl =
            $"{_dataverseUrl}/api/data/v9.2/cred7_players" +
            $"?$select=cred7_playerid&$filter=cred7_mc_username eq '{playerName}'";

        var resp = await HttpClient.GetAsync(queryUrl);
        await ValidateResponse(resp, "GET player");

        JObject json = JObject.Parse(await resp.Content.ReadAsStringAsync());

        if (json["value"]!.Any())
            return json["value"]![0]!["cred7_playerid"]!.ToString();

        JObject newPlayer = new()
        {
            ["cred7_mc_username"] = playerName
        };

        var createResp = await HttpClient.PostAsync(
            $"{_dataverseUrl}/api/data/v9.2/cred7_players",
            new StringContent(JsonConvert.SerializeObject(newPlayer), Encoding.UTF8, "application/json"));

        await ValidateResponse(createResp, "POST create player");

        return createResp.Headers.Location!.Segments.Last();
    }

    private JObject ApplyGameLogic(JObject entity, GameEvent e)
    {
        int kills = JsonHelper.GetInt(entity, "cred7_mc_kills");
        int mobKills = JsonHelper.GetInt(entity, "cred7_mc_mobskilled");
        int playerKills = JsonHelper.GetInt(entity, "cred7_mc_playerskilled");
        int blocks = JsonHelper.GetInt(entity, "cred7_mc_blocksplaced");
        int credits = JsonHelper.GetInt(entity, "cred7_mc_credits");
        int deaths = JsonHelper.GetInt(entity, "cred7_mc_deaths");

        switch (e.EventType)
        {
            case "MobKilled":
                kills++;
                mobKills++;
                credits += CreditCalculator.CalculateCredits(e);
                break;

            case "PlayerKilled":
                kills++;
                playerKills++;
                credits += CreditCalculator.CalculateCredits(e);
                break;

            case "BlockPlaced":
                blocks++;
                credits += CreditCalculator.CalculateCredits(e);
                break;

            case "PlayerDied":
                deaths++;
                break;
        }

        return new JObject
        {
            ["cred7_mc_kills"] = kills,
            ["cred7_mc_mobskilled"] = mobKills,
            ["cred7_mc_playerskilled"] = playerKills,
            ["cred7_mc_blocksplaced"] = blocks,
            ["cred7_mc_credits"] = credits,
            ["cred7_mc_deaths"] = deaths
        };
    }

    private async Task<string?> GetFirstActiveMatchAsync()
    {
        await EnsureAccessTokenAsync();
        SetDataverseHeaders();

        string query =
            $"{_dataverseUrl}/api/data/v9.2/cred7_matchs" +
            $"?$select=cred7_matchid&$filter=cred7_mc_status eq 684770001&$top=1";

        var resp = await HttpClient.GetAsync(query);
        await ValidateResponse(resp, "GET first active match");

        JObject json = JObject.Parse(await resp.Content.ReadAsStringAsync());

        if (!json["value"]!.Any())
            return null;

        return json["value"]![0]!["cred7_matchid"]!.ToString();
    }

    private async Task EnsureAccessTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return;

        var app = ConfidentialClientApplicationBuilder
            .Create(_clientId)
            .WithClientSecret(_clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{_tenantId}")
            .Build();

        var result = await app
            .AcquireTokenForClient(new[] { $"{_dataverseUrl}/.default" })
            .ExecuteAsync();

        _accessToken = result.AccessToken;
        _tokenExpiry = result.ExpiresOn.UtcDateTime;
    }

    private void SetDataverseHeaders()
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);

        HttpClient.DefaultRequestHeaders.Remove("OData-MaxVersion");
        HttpClient.DefaultRequestHeaders.Remove("OData-Version");
        HttpClient.DefaultRequestHeaders.Remove("Accept");

        HttpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
        HttpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private async Task ValidateResponse(HttpResponseMessage resp, string action)
    {
        if (!resp.IsSuccessStatusCode)
        {
            string content = await resp.Content.ReadAsStringAsync();
            _logger.LogError("{Action} failed: {Status} - {Content}", action, resp.StatusCode, content);
            throw new Exception($"{action} failed: {resp.StatusCode} - {content}");
        }
    }
}
