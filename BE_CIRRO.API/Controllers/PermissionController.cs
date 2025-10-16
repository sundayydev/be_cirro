using BE_CIRRO.Core.Services;
using BE_CIRRO.Shared.Common;
using BE_CIRRO.Shared.DTOs.Permission;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BE_CIRRO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionService _permissionService;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(PermissionService permissionService, ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: api/permission - Lấy tất cả permissions trong hệ thống
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _permissionService.GetAllAsync();
            return Ok(ApiResponseFactory.Success(result));
        }

        // GET: api/permission/file/{fileId} - Lấy tất cả permissions của một file cụ thể
        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetByFile(Guid fileId)
        {
            var result = await _permissionService.GetByFileAsync(fileId);
            return Ok(ApiResponseFactory.Success(result));
        }

        // GET: api/permission/user/{userId} - Lấy tất cả permissions của một user cụ thể
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var result = await _permissionService.GetByUserAsync(userId);
            return Ok(ApiResponseFactory.Success(result));
        }

        // POST: api/permission - Tạo permission mới hoặc cập nhật permission hiện có
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermissionCreateDto dto)
        {
            try
            {
                var existingPermissions = await _permissionService.GetByFileAsync(dto.FileId);
                if (existingPermissions.Any(p => p.UserId == dto.UserId))
                {
                    var updateDto = new PermissionUpdateDto { PermissionType = dto.PermissionType };
                    var existingPermission = existingPermissions.First(p => p.UserId == dto.UserId && p.PermissionType == dto.PermissionType);
                    var updated = await _permissionService.UpdateAsync(existingPermission.PermissionId, updateDto);
                    return Ok(ApiResponseFactory.Success(updated));
                }
                var created = await _permissionService.CreateAsync(dto);
                return Ok(ApiResponseFactory.Success(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, new { message = "Failed to create permission", error = ex.Message });
            }
        }

        // PUT: api/permission/{id} - Cập nhật permission theo ID
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PermissionUpdateDto dto)
        {
            var updated = await _permissionService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(new { message = "Permission not found" });
            return Ok(ApiResponseFactory.Success(updated));
        }

        // DELETE: api/permission/{id} - Xóa permission theo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _permissionService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Permission not found" });
            return Ok(new { message = "Permission deleted successfully" });
        }

        // GET: api/permission/{id} - Lấy chi tiết một permission cụ thể
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var permission = await _permissionService.GetByIdAsync(id);
                if (permission == null) return NotFound(new { message = "Permission not found" });
                return Ok(ApiResponseFactory.Success(permission));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission by ID");
                return StatusCode(500, new { message = "Failed to retrieve permission", error = ex.Message });
            }
        }

        // GET: api/permission/folder/{folderId} - Lấy tất cả permissions của một folder
        [HttpGet("folder/{folderId}")]
        public async Task<IActionResult> GetByFolder(Guid folderId)
        {
            try
            {
                var permissions = await _permissionService.GetByFolderAsync(folderId);
                return Ok(ApiResponseFactory.Success(permissions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions by folder");
                return StatusCode(500, new { message = "Failed to retrieve folder permissions", error = ex.Message });
            }
        }

        // POST: api/permission/bulk - Cấp quyền hàng loạt cho nhiều user
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] PermissionBulkCreateDto dto)
        {
            try
            {
                var createdPermissions = await _permissionService.CreateBulkAsync(dto);
                return Ok(ApiResponseFactory.Success(createdPermissions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk permissions");
                return StatusCode(500, new { message = "Failed to create bulk permissions", error = ex.Message });
            }
        }

        // DELETE: api/permission/user/{userId}/file/{fileId} - Xóa permission của user với file cụ thể
        [HttpDelete("user/{userId}/file/{fileId}")]
        public async Task<IActionResult> DeleteByUserAndFile(Guid userId, Guid fileId)
        {
            try
            {
                var success = await _permissionService.DeleteByUserAndFileAsync(userId, fileId);
                if (!success) return NotFound(new { message = "Permission not found" });
                return Ok(new { message = "Permission deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission by user and file");
                return StatusCode(500, new { message = "Failed to delete permission", error = ex.Message });
            }
        }

        // DELETE: api/permission/user/{userId}/folder/{folderId} - Xóa permission của user với folder cụ thể
        [HttpDelete("user/{userId}/folder/{folderId}")]
        public async Task<IActionResult> DeleteByUserAndFolder(Guid userId, Guid folderId)
        {
            try
            {
                var success = await _permissionService.DeleteByUserAndFolderAsync(userId, folderId);
                if (!success) return NotFound(new { message = "Permission not found" });
                return Ok(new { message = "Permission deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission by user and folder");
                return StatusCode(500, new { message = "Failed to delete permission", error = ex.Message });
            }
        }

        // GET: api/permission/user/{userId}/accessible-files - Lấy danh sách file mà user có quyền truy cập
        [HttpGet("user/{userId}/accessible-files")]
        public async Task<IActionResult> GetAccessibleFiles(Guid userId)
        {
            try
            {
                var files = await _permissionService.GetAccessibleFilesAsync(userId);
                return Ok(ApiResponseFactory.Success(files));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accessible files for user");
                return StatusCode(500, new { message = "Failed to retrieve accessible files", error = ex.Message });
            }
        }

        // GET: api/permission/user/{userId}/accessible-folders - Lấy danh sách folder mà user có quyền truy cập
        [HttpGet("user/{userId}/accessible-folders")]
        public async Task<IActionResult> GetAccessibleFolders(Guid userId)
        {
            try
            {
                var folders = await _permissionService.GetAccessibleFoldersAsync(userId);
                return Ok(ApiResponseFactory.Success(folders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accessible folders for user");
                return StatusCode(500, new { message = "Failed to retrieve accessible folders", error = ex.Message });
            }
        }
    }
}
