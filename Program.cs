using Amazon.Runtime;
using Amazon.S3;
using System.Net;

namespace ConsoleApp1;

internal class Program
{
    static async Task Main(string[] args)
    {
        Amazon.AWSConfigs.HttpClientFactory = new CustomHttpClientFactory();

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        var s3Config = new AmazonS3Config
        {
            //EndpointProvider = new EndpointProvider(),
            ServiceURL = "https://localhost:8081",
            UseAccelerateEndpoint = false,
            UseHttp = true, 
            ForcePathStyle = true,
            SignatureVersion = "V4", // or "V2" if your local S3 service uses it
            //RegionEndpoint = RegionEndpoint.USWest2,
            MaxErrorRetry = 0,
            //HttpClientFactory = new CustomHttpClientFactory()
        };

        using var s3Client = new AmazonS3Client(s3Config);
        const int TEST_FILE_SIZE = 32 * 1024 * 1024 * 10;
        var tempFilePath = CreateRandomFile(TEST_FILE_SIZE);
        await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = "kmalhar-test",
            Key = "test-file-3",
            FilePath = tempFilePath
        });

    }

    static string CreateRandomFile(int size)
    {
        string tempFilePath = Path.GetTempFileName();

        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
        {
            var rng = new Random();
            var buffer = new byte[4096];

            while (size > 0)
            {
                int remaining = Math.Min(4096, size);
                rng.NextBytes(buffer);
                fileStream.Write(buffer, 0, remaining);
                size -= remaining;
            }
        }

        return tempFilePath;
    }
}

internal class CustomHttpClientFactory : Amazon.Runtime.HttpClientFactory
{
    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {

        // Create a new HttpClientHandler
        HttpClientHandler handler = new HttpClientHandler();

        // Disable certificate validation
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        var httpClient = new HttpClient(handler);

        return httpClient;
    }
}

