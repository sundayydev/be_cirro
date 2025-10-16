using BE_CIRRO.Core.Services;
using BE_CIRRO.Shared.Common;
using BE_CIRRO.Shared.DTOs.FileVersion;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_CIRRO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileVersionController : ControllerBase
    {
        private readonly FileVersionService _fileVersionService;
        public FileVersionController(FileVersionService fileVersionService)
        {
            _fileVersionService = fileVersionService;
        }
        //GET: /api/fileversion/file/{fileId} - Lấy tất cả phiên bản của một file cụ thể
        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetByFile(Guid fileId)
        {
            var versions = await _fileVersionService.GetByFileAsync(fileId);
            return Ok(ApiResponseFactory.Success(versions.Adapt<IEnumerable<FileVersionDto>>()));
        }

        //GET: /api/fileversion/{id} - Lấy chi tiết một phiên bản file cụ thể
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var version = await _fileVersionService.GetByIdAsync(id);
            if (version == null) return NotFound();

            return Ok(ApiResponseFactory.Success(version.Adapt<FileVersionDto>()));
        }

        //POST: /api/fileversion/upload - Upload phiên bản mới của file
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] FileVersionCreateDto dto)
        {
            try
            {
                var version = await _fileVersionService.UploadVersionAsync(dto);
                return Ok(version.Adapt<FileVersionDto>());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //GET: /api/fileversion/download/{id} - Tải xuống một phiên bản file cụ thể
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var stream = await _fileVersionService.GetFileVersionStreamAsync(id);
            if (stream == null) return NotFound();

            var version = await _fileVersionService.GetByIdAsync(id);
            var fileName = $"{version!.FileId}_{version.VersionNumber}";
            return File(stream, "application/octet-stream", fileName);
        }

        //GET: /api/fileversion - Lấy tất cả các phiên bản file
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var versions = await _fileVersionService.GetAllAsync();
                return Ok(ApiResponseFactory.Success(versions.Adapt<IEnumerable<FileVersionDto>>()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve file versions", error = ex.Message });
            }
        }

        //DELETE: /api/fileversion/{id} - Xóa một phiên bản file cụ thể
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _fileVersionService.DeleteAsync(id);
                if (!success) return NotFound(new { message = "File version not found" });
                return Ok(new { message = "File version deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete file version", error = ex.Message });
            }
        }

        //PUT: /api/fileversion/{id} - Cập nhật thông tin phiên bản file
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FileVersionUpdateDto dto)
        {
            try
            {
                var updated = await _fileVersionService.UpdateAsync(id, dto);
                if (updated == null) return NotFound(new { message = "File version not found" });
                return Ok(ApiResponseFactory.Success(updated.Adapt<FileVersionDto>()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update file version", error = ex.Message });
            }
        }

        //GET: /api/fileversion/latest/{fileId} - Lấy phiên bản mới nhất của file
        [HttpGet("latest/{fileId}")]
        public async Task<IActionResult> GetLatestVersion(Guid fileId)
        {
            try
            {
                var latestVersion = await _fileVersionService.GetLatestVersionAsync(fileId);
                if (latestVersion == null) return NotFound(new { message = "No versions found for this file" });
                return Ok(ApiResponseFactory.Success(latestVersion.Adapt<FileVersionDto>()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve latest version", error = ex.Message });
            }
        }

        //POST: /api/fileversion/{id}/restore - Khôi phục file về phiên bản cụ thể
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreVersion(Guid id)
        {
            try
            {
                var success = await _fileVersionService.RestoreVersionAsync(id);
                if (!success) return NotFound(new { message = "File version not found" });
                return Ok(new { message = "File restored to selected version successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to restore file version", error = ex.Message });
            }
        }
    }
}
