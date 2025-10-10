using BE_CIRRO.Core.Services;
using BE_CIRRO.Shared.Common;
using BE_CIRRO.Shared.DTOs.File;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_CIRRO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly IMapper _mapper;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
            _mapper = new Mapper();
        }

        // GET: api/files/folder/{folderId}
        [HttpGet("folder/{folderId}")]
        public async Task<IActionResult> GetByFolder(Guid folderId)
        {
            try
            {
                var files = await _fileService.GetByFolderAsync(folderId);

                if (files == null || !files.Any())
                    return NotFound(ApiResponseFactory.NotFound<Object>("Không tìm thấy file nào trong Folder: " + folderId));

                return Ok(ApiResponseFactory.Success(files, "Lấy tất cả File có trong Folder: " + folderId));
            }
            catch (ArgumentException ex)
            {
                // lỗi do dữ liệu đầu vào sai
                return BadRequest(ApiResponseFactory.ValidationError<Object>($"Tham số không hợp lệ: {ex.Message}"));
            }
            catch (KeyNotFoundException ex)
            {
                // lỗi không tìm thấy folder
                return NotFound(ApiResponseFactory.NotFound<Object>($"Không tìm thấy Folder: {ex.Message}"));
            }
            catch (Exception ex)
            {
                // lỗi không xác định
                // nếu có logger thì log lại
                //_logger.LogError(ex, "Lỗi khi lấy danh sách file theo folderId {FolderId}", folderId);
                return StatusCode(500, ApiResponseFactory.ServerError("Đã xảy ra lỗi khi lấy danh sách file: " + ex.Message));
            }
        }

        //GET: api/files/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var file = await _fileService.GetByIdAsync(id);
            if (file == null) return StatusCode(StatusCodes.Status404NotFound, ApiResponseFactory.NotFound<Object>("Không tìm thấy File"));

            return Ok(ApiResponseFactory.Success(file.Adapt<FileDto>()));
        }

        //POST: api/files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] FileCreateDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is empty");

            var newFile = await _fileService.UploadAsync(dto.File, dto.OwnerId, dto.FolderId);
            return Ok(ApiResponseFactory.Success(newFile.Adapt<FileDto>()));
        }

        // GET: api/files/download/{id}
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                //Lấy thông tin file từ DB
                var file = await _fileService.GetByIdAsync(id);
                if (file == null)
                {
                    return NotFound(ApiResponseFactory.NotFound<Object>($"File với Id '{id}' không tìm thấy."));
                }

                //Lấy stream từ S3
                var stream = await _fileService.GetFileStreamAsync(id);
                if (stream == null)
                {
                    return NotFound(ApiResponseFactory.NotFound<Object>("Không tìm thấy File trong storage (S3)",
                        new { fileName = file.Name }));
                }

                //Trả file stream về client
                return File(stream, file.FileType ?? "application/octet-stream", file.Name);
            }
            catch (Amazon.S3.AmazonS3Exception s3Ex)
            {
                //Lỗi từ S3 (ví dụ quyền, bucket không tồn tại, timeout,…)
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "S3 storage lỗi.",
                    error = s3Ex.Message
                });
            }
            catch (IOException ioEx)
            {
                //Lỗi stream hoặc file access
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "IO error while reading file.",
                    error = ioEx.Message
                });
            }
            catch (Exception ex)
            {
                //Lỗi không xác định
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Unexpected error occurred while downloading file.",
                    error = ex.Message
                });
            }
        }

        //POST: /api/files/version

        //DELETE: /api/files/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _fileService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy File với ID đã cho."));
            return Ok(ApiResponseFactory.Deleted("Xóa File thành công."));
        }
    }
}
