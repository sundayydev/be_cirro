using BE_CIRRO.Core.Services;
using BE_CIRRO.Shared.Common;
using BE_CIRRO.Shared.DTOs.File;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
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

        // GET: /api/files - Lấy tất cả files hoặc search files
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FileSearchDto searchDto)
        {
            try
            {
                // TODO: Implement search files logic
                // var files = await _fileService.SearchFilesAsync(searchDto);
                var files = new List<FileDto>(); // Placeholder
                return Ok(ApiResponseFactory.Success(files, "Lấy danh sách file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi lấy danh sách file: " + ex.Message));
            }
        }

        // PUT: /api/files/{id} - Cập nhật thông tin file
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FileUpdateDto dto)
        {
            try
            {
                // TODO: Implement update file logic
                // var updatedFile = await _fileService.UpdateAsync(id, dto);
                // if (updatedFile == null)
                //     return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy file với ID đã cho"));

                return Ok(ApiResponseFactory.Success<object>(null, "Cập nhật file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi cập nhật file: " + ex.Message));
            }
        }

        // POST: /api/files/{id}/move - Di chuyển file sang folder khác
        [HttpPost("{id}/move")]
        public async Task<IActionResult> MoveFile(Guid id, [FromBody] FileMoveDto dto)
        {
            try
            {
                // TODO: Implement move file logic
                // var success = await _fileService.MoveFileAsync(id, dto.TargetFolderId);
                // if (!success)
                //     return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy file hoặc folder đích"));

                return Ok(ApiResponseFactory.Success<object>(null, "Di chuyển file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi di chuyển file: " + ex.Message));
            }
        }

        // POST: /api/files/{id}/copy - Sao chép file sang folder khác
        [HttpPost("{id}/copy")]
        public async Task<IActionResult> CopyFile(Guid id, [FromBody] FileCopyDto dto)
        {
            try
            {
                // TODO: Implement copy file logic
                // var copiedFile = await _fileService.CopyFileAsync(id, dto.TargetFolderId, dto.NewName);
                // if (copiedFile == null)
                //     return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy file hoặc folder đích"));

                return Ok(ApiResponseFactory.Success<object>(null, "Sao chép file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi sao chép file: " + ex.Message));
            }
        }

        // GET: /api/files/search - Tìm kiếm files với các tiêu chí
        [HttpGet("search")]
        public async Task<IActionResult> SearchFiles([FromQuery] FileSearchDto searchDto)
        {
            try
            {
                // TODO: Implement search files logic
                // var files = await _fileService.SearchFilesAsync(searchDto);
                var files = new List<FileDto>(); // Placeholder
                return Ok(ApiResponseFactory.Success(files, "Tìm kiếm file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi tìm kiếm file: " + ex.Message));
            }
        }

        // GET: /api/files/user/{userId} - Lấy tất cả files của một user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            try
            {
                // TODO: Implement get files by user logic
                // var files = await _fileService.GetByUserAsync(userId);
                var files = new List<FileDto>(); // Placeholder
                return Ok(ApiResponseFactory.Success(files, $"Lấy danh sách file của user {userId} thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi lấy danh sách file của user: " + ex.Message));
            }
        }

        // GET: /api/files/{id}/versions - Lấy danh sách versions của file
        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetFileVersions(Guid id)
        {
            try
            {
                // TODO: Implement get file versions logic
                // var versions = await _fileService.GetFileVersionsAsync(id);
                var versions = new List<object>(); // Placeholder
                return Ok(ApiResponseFactory.Success(versions, "Lấy danh sách phiên bản file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi lấy danh sách phiên bản file: " + ex.Message));
            }
        }

        // POST: /api/files/{id}/share - Chia sẻ file với user khác
        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareFile(Guid id, [FromBody] FileShareDto dto)
        {
            try
            {
                // TODO: Implement share file logic
                // var success = await _fileService.ShareFileAsync(id, dto.UserIds, dto.PermissionType);
                // if (!success)
                //     return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy file hoặc user"));

                return Ok(ApiResponseFactory.Success<object>(null, "Chia sẻ file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi chia sẻ file: " + ex.Message));
            }
        }

        // GET: /api/files/shared - Lấy danh sách files được chia sẻ với user hiện tại
        [HttpGet("shared")]
        public async Task<IActionResult> GetSharedFiles()
        {
            try
            {
                // Lấy user ID từ JWT token (giả sử có authentication)
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized(ApiResponseFactory.Unauthorized("Token không hợp lệ"));

                // TODO: Implement get shared files logic
                // var files = await _fileService.GetSharedFilesAsync(userId);
                var files = new List<FileDto>(); // Placeholder
                return Ok(ApiResponseFactory.Success(files, "Lấy danh sách file được chia sẻ thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseFactory.ServerError("Lỗi khi lấy danh sách file được chia sẻ: " + ex.Message));
            }
        }

        // GET: api/files/stream/{id}
        [HttpGet("stream/{id}")]
        [Authorize] // Bạn NÊN thêm [Authorize] ở đây và ở 'Download' để bảo mật
        public async Task<IActionResult> StreamFile(Guid id)
        {
            try
            {
                // 1. Lấy thông tin file từ DB
                var file = await _fileService.GetByIdAsync(id);
                if (file == null)
                {
                    return NotFound(ApiResponseFactory.NotFound<Object>($"File với Id '{id}' không tìm thấy."));
                }

                // 2. Lấy stream từ S3
                var stream = await _fileService.GetFileStreamAsync(id);
                if (stream == null)
                {
                    return NotFound(ApiResponseFactory.NotFound<Object>("Không tìm thấy File trong storage (S3)",
                        new { fileName = file.Name }));
                }

                // 3. Trả về FileStreamResult với hỗ trợ Range Processing
                // Đây là mấu chốt:
                // - Chúng ta KHÔNG truyền 'file.Name'. Điều này tránh việc
                //   trình duyệt tự động "Save As...".
                // - Chúng ta đặt 'enableRangeProcessing: true'. Điều này cho phép
                //   client (như <video>) yêu cầu các phần của file (HTTP 206 Partial Content).
                return File(
                    stream,
                    file.FileType ?? "application/octet-stream",
                    enableRangeProcessing: true // <-- Mấu chốt để streaming (video/audio)
                );
            }
            catch (Amazon.S3.AmazonS3Exception s3Ex)
            {
                // (Giữ nguyên logic bắt lỗi S3)
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "S3 storage lỗi.",
                    error = s3Ex.Message
                });
            }
            catch (IOException ioEx)
            {
                //(Giữ nguyên logic bắt lỗi IO)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "IO error while reading file.",
                    error = ioEx.Message
                });
            }
            catch (Exception ex)
            {
                // (Giữ nguyên logic bắt lỗi chung)
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Unexpected error occurred while streaming file.",
                    error = ex.Message
                });
            }
        }
    }
}
