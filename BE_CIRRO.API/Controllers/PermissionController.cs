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

        // GET:     
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _permissionService.GetAllAsync();
            return Ok(ApiResponseFactory.Success(result));
        }

        // GET: api/permission/file/{fileId}
        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetByFile(Guid fileId)
        {
            var result = await _permissionService.GetByFileAsync(fileId);
            return Ok(ApiResponseFactory.Success(result));
        }

        // GET: api/permission/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var result = await _permissionService.GetByUserAsync(userId);
            return Ok(ApiResponseFactory.Success(result));
        }

        // POST: api/permission
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

        // PUT: api/permission/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PermissionUpdateDto dto)
        {
            var updated = await _permissionService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(new { message = "Permission not found" });
            return Ok(ApiResponseFactory.Success(updated));
        }

        // DELETE: api/permission/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _permissionService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Permission not found" });
            return Ok(new { message = "Permission deleted successfully" });
        }
    }
}
