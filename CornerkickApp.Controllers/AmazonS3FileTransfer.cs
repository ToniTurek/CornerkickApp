using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CornerkickApp.Shared.Models;
using SixLabors.ImageSharp;

namespace CornerkickApp.Controllers
{
  public class AmazonS3FileTransfer
  {
    private static string sBucketName = "ckamazonbucket";
    private static RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
    private static string? sAwsKeyId     = "";
    private static string? sAwsSecretKey = "";
    private static IAmazonS3? client;

    internal readonly string? sCkInstanceName;

    private readonly IConfiguration? _configuration;

    public AmazonS3FileTransfer(IConfiguration configuration)
    {
      _configuration = configuration;

      // First, get variables from environment
      sAwsKeyId     = Environment.GetEnvironmentVariable("ckAwsKeyId");
      sAwsSecretKey = Environment.GetEnvironmentVariable("ckAwsSecretKey");

      sCkInstanceName = Environment.GetEnvironmentVariable("ckInstanceName");

      // If empty, get them from appsettings.json
      if (string.IsNullOrEmpty(sAwsKeyId))     sAwsKeyId     = configuration.GetSection("ckAwsKeyId").Value;
      if (string.IsNullOrEmpty(sAwsSecretKey)) sAwsSecretKey = configuration.GetSection("ckAwsSecretKey").Value;
      if (string.IsNullOrEmpty(sAwsKeyId) || string.IsNullOrEmpty(sAwsSecretKey)) return;

      if (string.IsNullOrEmpty(sCkInstanceName)) sCkInstanceName = configuration.GetSection("ckInstanceName").Value;
      if (!string.IsNullOrEmpty(sCkInstanceName)) sCkInstanceName += "/";

      client = new AmazonS3Client(sAwsKeyId, sAwsSecretKey, bucketRegion);
    }
    public AmazonS3FileTransfer(string? _sAwsKeyId, string? _sAwsSecretKey)
    {
      if (string.IsNullOrEmpty(_sAwsKeyId))     _sAwsKeyId     = Environment.GetEnvironmentVariable("ckAwsKeyId");
      if (string.IsNullOrEmpty(_sAwsSecretKey)) _sAwsSecretKey = Environment.GetEnvironmentVariable("ckAwsSecretKey");
      if (string.IsNullOrEmpty(_sAwsKeyId) || string.IsNullOrEmpty(_sAwsSecretKey)) return;

      sAwsKeyId = _sAwsKeyId;
      sAwsSecretKey = _sAwsSecretKey;

      client = new AmazonS3Client(_sAwsKeyId, _sAwsSecretKey, bucketRegion);
    }

    public async Task<IList<S3Object>> listFilesAsync(string sPath)
    {
      string token;
      var ltObjects = new List<S3Object>();
      if (client == null) return ltObjects;

      do {
        ListObjectsRequest listRequest = new ListObjectsRequest {
          BucketName = sBucketName,
          Prefix = sPath,
        };

        try
        {
          ListObjectsResponse loResponse = await client.ListObjectsAsync(listRequest).ConfigureAwait(false);
          ltObjects.AddRange(loResponse.S3Objects);

          token = loResponse.NextMarker;
        } catch {
          break;
        }
      } while (token != null);

      return ltObjects;
    }

    public async Task uploadFileAsync(string sFile, string? sKey = null, string sContentType = "text/plain", int nRetry = 0)
    {
#if _NO_UPLOAD
      return;
#else
      if (!File.Exists(sFile)) return;

      //var credentials = new Amazon.Runtime.StoredProfileAWSCredentials("ckAwsProfile");

      //var client = new AmazonS3Client(bucketRegion);
      //string accessKey = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
      //string secretAccessKey = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];

      if (string.IsNullOrEmpty(sKey)) sKey = sFile;

      // Try to delete existing file
      try {
        await deleteFileAsync(sKey);
      } catch {
      }

      try {
        CkAppShared.ckMng.tl.writeLog("Try to upload file '" + sFile + "' to key '" + sKey + "' (Type: '" + sContentType + "')");

        PutObjectRequest putRequest = new PutObjectRequest {
          BucketName = sBucketName,
          Key = sKey,
          FilePath = sFile,
          ContentType = sContentType,
          CannedACL = S3CannedACL.PublicRead
        };

        try {
          PutObjectResponse response = await client.PutObjectAsync(putRequest);
          CkAppShared.ckMng.tl.writeLog("Status code: " + response.HttpStatusCode.ToString());
          //using (S3Response r = client.PutObject(putRequest)) { }
        } catch (AmazonS3Exception amazonS3Exception) {
          CkAppShared.ckMng.tl.writeLog("ERROR! S3 PutObjectResponse Exception Message: " + amazonS3Exception.Message, CornerkickManager.Main.sErrorFile);
        }
      } catch (AmazonS3Exception amazonS3Exception) {
        if (nRetry > 0) {
          await wait(1000);
          await Task.Run(() => uploadFileAsync(sFile, sKey: sKey, sContentType: sContentType, nRetry: nRetry - 1));
          return;
        }

        CkAppShared.ckMng.tl.writeLog("ERROR! S3 Exception Message: " + amazonS3Exception.Message, CornerkickManager.Main.sErrorFile);

        if (amazonS3Exception.ErrorCode != null) {
          CkAppShared.ckMng.tl.writeLog("Error Code: " + amazonS3Exception.ErrorCode, CornerkickManager.Main.sErrorFile);

          if (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")) {
            CkAppShared.ckMng.tl.writeLog("Check the provided AWS Credentials.", CornerkickManager.Main.sErrorFile);
          }
        }
      } catch (Exception e) {
        if (nRetry > 0) {
          await wait(1000);
          await Task.Run(() => uploadFileAsync(sFile, sKey: sKey, sContentType: sContentType, nRetry: nRetry - 1));
          return;
        }

        CkAppShared.ckMng.tl.writeLog("ERROR! Exception Message: " + e.Message + " File '" + sFile + "' not uploaded", CornerkickManager.Main.sErrorFile);
      }
#endif
    }

    private async Task wait(int iDuration)
    {
      if (iDuration <= 0) return;

      Task tkDelay = Task.Delay(iDuration);
      await tkDelay.ConfigureAwait(false);
      tkDelay.Dispose();
    }

    public async Task deleteFileAsync(string sKey)
    {
#if _NO_UPLOAD
      return;
#else
      var deleteObjectRequest = new DeleteObjectRequest {
        BucketName = sBucketName,
        Key = sKey
      };

      //Console.WriteLine("Deleting an object");
      await client.DeleteObjectAsync(deleteObjectRequest);
#endif
    }

    public string downloadFile(string sKey, string sTargetPath = "./")
    {
      //IAmazonS3 client = new AmazonS3Client(bucketRegion);
      //ReadObjectDataAsync(client, sKey).Wait();
      try {
        TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(sAwsKeyId, sAwsSecretKey, bucketRegion));

        // If target is directory: add filename
        if (Directory.Exists(sTargetPath)) sTargetPath = Path.Combine(sTargetPath, Path.GetFileName(sKey));

        // 2. Specify object key name explicitly.
        try {
          fileTransferUtility.Download(sTargetPath, sBucketName, sKey);
        } catch (AmazonS3Exception s3Exception) {
          string sRet = String.Format("AmazonS3Exception error while downloading file {0} from bucket: {1} to location: {2}. Message: {3}", sKey, sBucketName, sTargetPath, s3Exception.Message);
          CkAppShared.ckMng.tl.writeLog(sRet, sLogFile: CornerkickManager.Main.sErrorFile);
          return sRet;
        } catch (Amazon.Runtime.AmazonServiceException ase) {
          string sRet = String.Format("AmazonServiceException error while downloading file: {0} from bucket: {1} to location: {2}. Message: {3}", sKey, sBucketName, sTargetPath, ase.Message);
          CkAppShared.ckMng.tl.writeLog(sRet, sLogFile: CornerkickManager.Main.sErrorFile);
          return sRet;
        }
        CkAppShared.ckMng.tl.writeLog(String.Format("Succesfully downloaded file: {0} from bucket: {1} to location: {2}", sKey, sBucketName, sTargetPath));
      } catch (AmazonS3Exception s3Exception) {
        //Console.WriteLine(s3Exception.Message, s3Exception.InnerException);
        string sRet = String.Format("AmazonS3Exception error while downloading file {0} from bucket: {1} to location: {2}. Message: {3}", sKey, sBucketName, sTargetPath, s3Exception.Message);
        CkAppShared.ckMng.tl.writeLog(sRet, sLogFile: CornerkickManager.Main.sErrorFile);
        return sRet;
      } catch (Amazon.Runtime.AmazonServiceException ase) {
        string sRet = String.Format("AmazonServiceException error while downloading file: {0} from bucket: {1} to location: {2}. Message: {3}", sKey, sBucketName, sTargetPath, ase.Message);
        CkAppShared.ckMng.tl.writeLog(sRet, sLogFile: CornerkickManager.Main.sErrorFile);
        return sRet;
      } catch (Exception e) {
        string sRet = String.Format("Unknown error while downloading file: {0} from bucket: {1} to location: {2}. Message: {3}", sKey, sBucketName, sTargetPath, e.Message);
        CkAppShared.ckMng.tl.writeLog(sRet, sLogFile: CornerkickManager.Main.sErrorFile);
        return sRet;
      }
      return "";
    }

    public async Task downloadAllFilesAsync(string sS3SubDir, string sTargetPath = "./", string sStartsWith = null, string sEndsWith = null, bool bForce = false)
    {
      ListObjectsRequest request = new ListObjectsRequest();
      request.BucketName = sBucketName;

      do {
        ListObjectsResponse response = await client.ListObjectsAsync(sBucketName, sS3SubDir);

        // Process response
        for (int iS3 = 0; iS3 < response.S3Objects.Count; iS3++) {
          if (!string.IsNullOrEmpty(sStartsWith) && !response.S3Objects[iS3].Key.StartsWith(sStartsWith)) continue;
          if (!string.IsNullOrEmpty(sEndsWith)   && !response.S3Objects[iS3].Key.EndsWith  (sEndsWith))   continue;

          string sTargetFilename = Path.Combine(sTargetPath, response.S3Objects[iS3].Key.Replace(sCkInstanceName, ""));

          if (!bForce) {
            // Check if file already present
            if (File.Exists(sTargetFilename)) {
              if (new System.IO.FileInfo(sTargetFilename).Length == response.S3Objects[iS3].Size) continue;
            }
          }

          downloadFile(response.S3Objects[iS3].Key, sTargetFilename);
        }

        // If response is truncated, set the marker to get the next set of keys
        if (response.IsTruncated) {
          request.Marker = response.NextMarker;
        } else {
          request = null;
        }
      } while (request != null);
    }

    async Task ReadObjectDataAsync(string sKey)
    {
      string responseBody = "";

      try {
        GetObjectRequest request = new GetObjectRequest {
          BucketName = sBucketName,
          Key = sKey
        };
        using (GetObjectResponse response = await client.GetObjectAsync(request))
        using (Stream responseStream = response.ResponseStream)
        using (StreamReader reader = new StreamReader(responseStream)) {
          string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
          string contentType = response.Headers["Content-Type"];
          //Console.WriteLine("Object metadata, Title: {0}", title);
          //Console.WriteLine("Content type: {0}", contentType);

          responseBody = reader.ReadToEnd(); // Now you process the response body.
        }
      } catch (AmazonS3Exception e) {
        Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
      } catch (Exception e) {
        Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
      }
    }

  }
}
