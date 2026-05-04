namespace CornerkickApp.Shared.Services
{
  public interface IAmazonS3Service
  {
    Task<AmazonS3Credentials> GetAmazonS3CredentialsAsync();
  }

  public class AmazonS3Credentials
  {
    public string? sAwsKeyId { get; set; }
    public string? sAwsSecretKey { get; set; }
  }
}
