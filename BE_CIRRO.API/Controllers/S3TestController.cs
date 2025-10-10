using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_CIRRO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3TestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public S3TestController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var accessKey = _config["AWS:AccessKey"];
                var secretKey = _config["AWS:SecretKey"];
                var region = Amazon.RegionEndpoint.GetBySystemName(_config["AWS:Region"]);

                using var s3 = new AmazonS3Client(accessKey, secretKey, region);
                var response = await s3.ListBucketsAsync();

                var buckets = response.Buckets.Select(b => b.BucketName).ToList();

                return Ok(new
                {
                    message = "Kết nối AWS S3 thành công!",
                    bucketCount = buckets.Count,
                    buckets
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Kết nối AWS S3 thất bại",
                    error = ex.Message
                });
            }
        }
    }
}
