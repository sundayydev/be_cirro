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
        //GET: /api/fileversion/file/{fileId}
        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetByFile(Guid fileId)
        {
            var versions = await _fileVersionService.GetByFileAsync(fileId);
            return Ok(ApiResponseFactory.Success(versions.Adapt<IEnumerable<FileVersionDto>>()));
        }

        //GET: /api/fileversion/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var version = await _fileVersionService.GetByIdAsync(id);
            if (version == null) return NotFound();

            return Ok(ApiResponseFactory.Success(version.Adapt<FileVersionDto>()));
        }

        //POST: /api/fileversion/upload
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

        //GET: /api/fileversion/download/{id}
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var stream = await _fileVersionService.GetFileVersionStreamAsync(id);
            if (stream == null) return NotFound();

            var version = await _fileVersionService.GetByIdAsync(id);
            var fileName = $"{version!.FileId}_{version.VersionNumber}";
            return File(stream, "application/octet-stream", fileName);
        }
    }
}
