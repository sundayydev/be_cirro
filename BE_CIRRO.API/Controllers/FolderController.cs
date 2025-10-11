using BE_CIRRO.Core.Services;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.Common;
using BE_CIRRO.Shared.DTOs.Folder;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BE_CIRRO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly FolderService _folderService;
        private readonly FileService _fileService;
        private readonly ILogger<FolderController> _logger;
        private readonly IMapper _mapper;
        public FolderController(FolderService folderService, ILogger<FolderController> logger, IMapper mapper, FileService fileService)
        {
            _folderService = folderService;
            _fileService = fileService;
            _logger = logger;
            _mapper = mapper;

        }

        // GET: api/folder
        [HttpGet]
        [SwaggerOperation(Summary = "Lấy tất cả các thư mục")]
        public async Task<IActionResult> GetAllFolders()
        {
            var folders = await _folderService.GetAllFoldersAsync();
            return Ok(ApiResponseFactory.Success(folders, "Lấy tất cả các thư mục thành công."));
        }
        // GET: api/folder/{id}
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Lấy thông tin thư mục theo ID")]
        public async Task<IActionResult> GetFolderById(Guid id)
        {
            var folder = await _folderService.GetFolderByIdAsync(id);
            if (folder == null)
                return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy thư mục với ID đã cho."));
            return Ok(ApiResponseFactory.Success(folder, "Lấy thông tin thư mục thành công."));
        }

        // POST: api/folder
        [HttpPost]
        [SwaggerOperation(Summary = "Tạo một thư mục mới")]
        public async Task<IActionResult> CreateFolder([FromBody] FolderCreateDto folder)
        {
            if (folder == null)
                return BadRequest(ApiResponseFactory.ValidationError<object>("Dữ liệu không được để trống."));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponseFactory.ValidationError<object>("Dữ liệu không hợp lệ", ModelState));

            try
            {
                var createdFolder = await _folderService.CreateFolderAsync(folder);
                return CreatedAtAction(nameof(GetFolderById), new { id = createdFolder.FolderId },
                    ApiResponseFactory.Created(createdFolder, "Tạo thư mục thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thư mục mới.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponseFactory.ServerError("Đã xảy ra lỗi khi tạo thư mục."));
            }
        }


        // PUT: api/folder/{id}
        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin thư mục")]
        public async Task<IActionResult> UpdateFolder(Guid id, [FromBody] FolderUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseFactory.ValidationError<object>("Dữ liệu không hợp lệ", ModelState));
            var updateFolder = _mapper.Map<Folder>(dto);
            var folder = await _folderService.UpdateFolderAsync(id, updateFolder);
            if (folder == null)
                return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy thư mục với ID đã cho."));
            return Ok(ApiResponseFactory.Updated(folder, "Cập nhật thư mục thành công."));
        }
        // DELETE: api/folder/{id}
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Xóa một thư mục")]
        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            var success = await _folderService.DeleteFolderAsync(id);
            if (!success)
                return NotFound(ApiResponseFactory.NotFound<object>("Không tìm thấy thư mục với ID đã cho."));
            return Ok(ApiResponseFactory.Deleted("Xóa thư mục thành công."));
        }

        // GET: api/folder/tree/{ownerId}
        [HttpGet("tree/{ownerId:guid}")]
        [SwaggerOperation(Summary = "Nhận cây thư mục lồng nhau cho một chủ sở hữu cụ thể")]
        public async Task<IActionResult> GetFolderTree(Guid ownerId)
        {
            var result = await _folderService.GetFolderTreeAsync(ownerId);
            try
            {
                if (result == null || !result.Any())
                    return NotFound(ApiResponseFactory.NotFound<Object>("Không tìm thấy thư mục nào cho chủ sở hữu này."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.ServerError("Đã xảy ra lỗi khi lấy cây thư mục."));
            }
            return Ok(ApiResponseFactory.Success(result, "Lấy cây thư mục thành công."));
        }

        //DELTE: api/folder/owner/{ownerId}

        // GET: api/folder/{folderId}/content
        [HttpGet("{folderId}/content")]
        public async Task<IActionResult> GetFolderContent(Guid folderId)
        {
            try
            {
                var folder = await _folderService.GetByIdAsync(folderId);
                if (folder == null)
                    return NotFound(new { message = $"Folder with id {folderId} not found." });

                var subFolders = await _folderService.GetByParentAsync(folderId);
                var files = await _fileService.GetByFolderAsync(folderId);

                return Ok(ApiResponseFactory.Success(new
                {
                    folderId = folder.FolderId,
                    folderName = folder.Name,
                    subFolders,
                    files
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folder content for {FolderId}", folderId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
