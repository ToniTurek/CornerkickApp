using CornerkickApp.Controllers;
using CornerkickApp.Shared.Services;

namespace CornerkickApp.Web.Services
{
  public class AmazonS3Service : IAmazonS3Service
  {
    public async Task<AmazonS3Credentials?> GetAmazonS3CredentialsAsync()
    {
      string? _sAwsKeyId     = Environment.GetEnvironmentVariable("ckAwsKeyId");
      string? _sAwsSecretKey = Environment.GetEnvironmentVariable("ckAwsSecretKey");

      return new AmazonS3Credentials() {
        sAwsKeyId     = _sAwsKeyId,
        sAwsSecretKey = _sAwsSecretKey
      };
    }
  }
}
