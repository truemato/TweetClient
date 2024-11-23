using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

public class TwitterAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _callbackUrl;

    public TwitterAuthClient(string clientId, string clientSecret, string callbackUrl)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _callbackUrl = callbackUrl;
        _httpClient = new HttpClient();
    }

    public (string Url, string CodeVerifier) GenerateAuthUrl()
    {
        // PKCEのcode_verifierを生成
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string state = Guid.NewGuid().ToString();

        var url = "https://twitter.com/i/oauth2/authorize" +
                 $"?response_type=code" +
                 $"&client_id={Uri.EscapeDataString(_clientId)}" +
                 $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
                 $"&scope={Uri.EscapeDataString("tweet.read tweet.write users.read offline.access")}" +
                 $"&state={Uri.EscapeDataString(state)}" +
                 $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                 $"&code_challenge_method=S256";

        return (url, codeVerifier);
    }

    public async Task<string> GetAccessTokenAsync(string code, string codeVerifier)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", _callbackUrl),
            new KeyValuePair<string, string>("code_verifier", codeVerifier)
        });

        var response = await _httpClient.PostAsync("https://api.twitter.com/2/oauth2/token", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get access token. Status: {response.StatusCode}, Response: {responseBody}");
        }

        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
        return json.GetProperty("access_token").GetString();
    }

    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(challengeBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}