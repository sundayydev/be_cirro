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
}
