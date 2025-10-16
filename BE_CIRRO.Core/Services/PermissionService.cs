using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs.Permission;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Services;

public class PermissionService
{
    private readonly IPermissionRepository _permissionRepo;
    private readonly IMapper _mapper;

    public PermissionService(IPermissionRepository permissionRepo, IMapper mapper)
    {
        _permissionRepo = permissionRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        var permissions = await _permissionRepo.GetAllAsync();
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<IEnumerable<PermissionDto>> GetByFileAsync(Guid fileId)
    {
        var permissions = await _permissionRepo.GetByFileIdAsync(fileId);
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<IEnumerable<PermissionDto>> GetByUserAsync(Guid userId)
    {
        var permissions = await _permissionRepo.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<PermissionDto> CreateAsync(PermissionCreateDto dto)
    {
        var entity = _mapper.Map<Permission>(dto);
        entity.PermissionId = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        await _permissionRepo.AddAsync(entity);

        return _mapper.Map<PermissionDto>(entity);
    }

    public async Task<PermissionDto?> UpdateAsync(Guid id, PermissionUpdateDto dto)
    {
        var permission = await _permissionRepo.GetByIdAsync(id);
        if (permission == null) return null;

        permission.PermissionType = dto.PermissionType;
        await _permissionRepo.UpdateAsync(permission);

        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var permission = await _permissionRepo.GetByIdAsync(id);
        if (permission == null) return false;

        await _permissionRepo.DeleteAsync(permission);
        return true;
    }

    // Lấy chi tiết một permission cụ thể
    public async Task<PermissionDto?> GetByIdAsync(Guid id)
    {
        var permission = await _permissionRepo.GetByIdAsync(id);
        if (permission == null) return null;

        return _mapper.Map<PermissionDto>(permission);
    }

    // Lấy tất cả permissions của một folder
    public async Task<IEnumerable<PermissionDto>> GetByFolderAsync(Guid folderId)
    {
        var permissions = await _permissionRepo.GetByFolderIdAsync(folderId);
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    // Tạo permissions hàng loạt cho nhiều user
    public async Task<IEnumerable<PermissionDto>> CreateBulkAsync(PermissionBulkCreateDto dto)
    {
        var createdPermissions = new List<PermissionDto>();

        foreach (var userId in dto.UserIds)
        {
            var permissionDto = new PermissionCreateDto
            {
                UserId = userId,
                FileId = dto.FileId,
                FolderId = dto.FolderId,
                PermissionType = dto.PermissionType
            };

            var created = await CreateAsync(permissionDto);
            createdPermissions.Add(created);
        }

        return createdPermissions;
    }

    // Xóa permission của user với file cụ thể
    public async Task<bool> DeleteByUserAndFileAsync(Guid userId, Guid fileId)
    {
        var permissions = await _permissionRepo.GetByFileIdAsync(fileId);
        var permission = permissions.FirstOrDefault(p => p.UserId == userId);
        
        if (permission == null) return false;

        await _permissionRepo.DeleteAsync(permission);
        return true;
    }

    // Xóa permission của user với folder cụ thể
    public async Task<bool> DeleteByUserAndFolderAsync(Guid userId, Guid folderId)
    {
        var permissions = await _permissionRepo.GetByFolderIdAsync(folderId);
        var permission = permissions.FirstOrDefault(p => p.UserId == userId);
        
        if (permission == null) return false;

        await _permissionRepo.DeleteAsync(permission);
        return true;
    }

    // Lấy danh sách file mà user có quyền truy cập
    public async Task<IEnumerable<object>> GetAccessibleFilesAsync(Guid userId)
    {
        var permissions = await _permissionRepo.GetByUserIdAsync(userId);
        var filePermissions = permissions.Where(p => p.FileId.HasValue);

        // Trả về thông tin file với quyền tương ứng
        return filePermissions.Select(p => new
        {
            FileId = p.FileId,
            PermissionType = p.PermissionType,
            PermissionId = p.PermissionId
        });
    }

    // Lấy danh sách folder mà user có quyền truy cập
    public async Task<IEnumerable<object>> GetAccessibleFoldersAsync(Guid userId)
    {
        var permissions = await _permissionRepo.GetByUserIdAsync(userId);
        var folderPermissions = permissions.Where(p => p.FolderId.HasValue);

        // Trả về thông tin folder với quyền tương ứng
        return folderPermissions.Select(p => new
        {
            FolderId = p.FolderId,
            PermissionType = p.PermissionType,
            PermissionId = p.PermissionId
        });
    }
}
