using CornerkickApp.Shared.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CornerkickApp.Services
{
  public class AmazonS3Service : IAmazonS3Service
  {
    private readonly MauiAuthenticationStateProvider _authenticationStateProvider;

    public AmazonS3Service(MauiAuthenticationStateProvider authenticationStateProvider)
    {
      _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<AmazonS3Credentials> GetAmazonS3CredentialsAsync()
    {
      var as3Credentials = new AmazonS3Credentials();
      try {
        var httpClient = HttpClientHelper.GetHttpClient();
        var weatherUrl = HttpClientHelper.AmazonS3Credentials;

        var accessTokenInfo = await _authenticationStateProvider.GetAccessTokenInfoAsync();

        if (accessTokenInfo is null) {
          throw new Exception("Could not retrieve access token to get Amazon S3 credentials.");
        }

        var token = accessTokenInfo.LoginResponse.AccessToken;
        var scheme = accessTokenInfo.LoginResponse.TokenType; //"Bearer"

        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(scheme)) {
          httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
          as3Credentials = (await httpClient.GetFromJsonAsync<AmazonS3Credentials>(weatherUrl)) ?? new AmazonS3Credentials();
        } else {
          Debug.WriteLine("Token or scheme is null or empty.");
        }
      } catch (HttpRequestException httpEx) {
        Debug.WriteLine($"HTTP Request error: {httpEx.Message}");
      } catch (Exception ex) {
        Debug.WriteLine($"An error occurred: {ex.Message}");
      }

      return as3Credentials;
    }
  }
}
