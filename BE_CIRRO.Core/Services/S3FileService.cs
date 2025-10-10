using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Services
{
    public class S3FileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3FileService(IConfiguration config)
        {
            _bucketName = config["AWS:BucketName"]!;
            _s3Client = new AmazonS3Client(
                config["AWS:AccessKey"],
                config["AWS:SecretKey"],
                RegionEndpoint.GetBySystemName(config["AWS:Region"])
            );
        }

        //Upload file
        public async Task<string> UploadFileAsync(IFormFile file, Guid ownerId, Guid? folderId)
        {
            var key = $"{ownerId}/{folderId?.ToString() ?? "root"}/{file.FileName}";

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }

        //Stream file
        public async Task<Stream?> GetFileStreamAsync(Guid ownerId, Guid? folderId, string fileName)
        {
            var key = $"{ownerId}/{folderId?.ToString() ?? "root"}/{fileName}";

            try
            {
                var response = await _s3Client.GetObjectAsync(_bucketName, key);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        //Xóa file
        public async Task<bool> DeleteFileAsync(Guid ownerId, Guid? folderId, string fileName)
        {
            var key = $"{ownerId}/{folderId?.ToString() ?? "root"}/{fileName}";
            try
            {
                await _s3Client.DeleteObjectAsync(_bucketName, key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Liệt kê file trong thư mục
        public async Task<IEnumerable<string>> ListFilesAsync(Guid ownerId, Guid? folderId)
        {
            var prefix = $"{ownerId}/{folderId?.ToString() ?? "root"}/";
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            return response.S3Objects.Select(o => o.Key.Replace(prefix, ""));
        }
    }
}
