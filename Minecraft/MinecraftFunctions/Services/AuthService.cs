using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace MinecraftFunctions.Services;

public class AuthService
{
    private string? _accessToken;
    private DateTime _expiry;

    private readonly string _tenantId = Environment.GetEnvironmentVariable("DV_TENANT_ID")!;
    private readonly string _clientId = Environment.GetEnvironmentVariable("DV_CLIENT_ID")!;
    private readonly string _clientSecret = Environment.GetEnvironmentVariable("DV_CLIENT_SECRET")!;
    private readonly string _dataverseUrl = Environment.GetEnvironmentVariable("DATAVERSE_URL")!;

    public async Task EnsureAccessTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _expiry.AddMinutes(-5))
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
        _expiry = result.ExpiresOn.UtcDateTime;
    }

    public void SetHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }
}
