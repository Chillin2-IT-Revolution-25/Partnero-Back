using Amazon.S3;
using Amazon.S3.Transfer;

namespace PartneroBackend.Services
{
    public class S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        private readonly string _bucketName = configuration["AWS:BucketName"]!;

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file.Length == 0)
                throw new ArgumentException("File is empty");
            
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds the limit of 5 MB");
            
            var key = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            string url = $"https://{_bucketName}.s3.amazonaws.com/{key}";

            return url;
        }
    }
}
