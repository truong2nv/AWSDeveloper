using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace S3Operations
{
    class CreateBucketTask
    {
        public async Task Run()
        {
            Console.WriteLine("\nStart of create bucket task");

            Console.WriteLine("\nReading configuration for bucket name...");
            var configSettings = ConfigSettingsReader<S3ConfigSettings>.Read("S3");

            try
            {
				IAmazonS3 s3Client = new AmazonS3Client();

                Console.WriteLine("Verifying that the bucket name is valid...");
                // Verify that the bucket exists. The code will exit
                // if the name is not valid for a new bucket.
                await VerifyBucketName(s3Client, configSettings.BucketName);

                // Create the notes bucket
                await CreateBucket(s3Client, configSettings.BucketName);

                // Pause until the the bucket is in the account
                Console.WriteLine("\nConfirm that the bucket exists...");
                await VerifyBucket (s3Client, configSettings.BucketName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("\nEnd of create bucket task");
        }

        async Task VerifyBucketName(IAmazonS3 s3Client, string bucketName)
        {
			bool exists = false;
            exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);


            if (exists)
			{
				// DoesS3BucketExistV2Async returns true if the bucket exists, but that does not
				// necessarily mean it belongs to your account as the method catches AccessDenied
				// and other exceptions. 
				// See https://github.com/aws/aws-sdk-net/blob/bd7ccb4f12feab0499d454371b6623c7798e371f/sdk/src/Services/S3/Custom/Util/AmazonS3Util.cs#L604
				Console.WriteLine("This bucket already exists in your, or someone else's, account. Exiting because there is nothing further to do!");
				Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("The bucket does not exist.");
			}

        }

        async Task CreateBucket(IAmazonS3 s3Client, string bucketName)
        {
            var region = s3Client.Config.RegionEndpoint;

            Console.WriteLine($"\nCreating {bucketName} in {region.DisplayName} ({region.SystemName})");

            var request = new PutBucketRequest
            {
                BucketName = bucketName,

                UseClientRegion = true
            };

            await s3Client.PutBucketAsync(request);


            Console.WriteLine("Success!");
        }

        async Task VerifyBucket(IAmazonS3 s3Client, string bucketName)
        {
            bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            while (!bucketExists)
            {
                System.Threading.Thread.Sleep(500);
                bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            }

            Console.WriteLine($"The bucket: {bucketName} is now available.");
        }
    }
}
