using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs.Folder;
using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Services;

public class FolderService
{
    private readonly IFolderRepository _folderRepository;
    private readonly IMapper _mapper;

    public FolderService(IFolderRepository folderRepository, IMapper mapper)
    {
        this._folderRepository = folderRepository;
        this._mapper = mapper;
    }

    public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
        => await _folderRepository.GetAllAsync();

    public async Task<Folder?> GetFolderByIdAsync(Guid id)
        => await _folderRepository.GetByIdAsync(id);

    public async Task<IEnumerable<Folder>> GetFoldersByOwnerAsync(Guid ownerId)
        => await _folderRepository.GetByOwnerAsync(ownerId);

    public async Task<IEnumerable<Folder>> GetFoldersByParentAsync(Guid? parentId)
        => await _folderRepository.GetByParentAsync(parentId);

    public async Task<Folder> CreateFolderAsync(FolderCreateDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Dữ liệu Folder không thể để giá trị null.");

        var folder = _mapper.Map<Folder>(dto);
        folder.FolderId = Guid.NewGuid();
        folder.CreatedAt = DateTime.UtcNow;

        await _folderRepository.AddAsync(folder);
        return folder;
    }
    public async Task<Folder?> UpdateFolderAsync(Guid id, Folder updatedFolder)
    {
        var existingFolder = await _folderRepository.GetByIdAsync(id);
        if (existingFolder == null)
            return null;

        // Cập nhật có điều kiện (chỉ thay đổi field được gửi lên)
        if (!string.IsNullOrWhiteSpace(updatedFolder.Name))
            existingFolder.Name = updatedFolder.Name;

        if (updatedFolder.OwnerId != Guid.Empty)
            existingFolder.OwnerId = updatedFolder.OwnerId;

        if (updatedFolder.ParentFolderId.HasValue)
            existingFolder.ParentFolderId = updatedFolder.ParentFolderId;

        existingFolder.UpdatedAt = DateTime.UtcNow; // nếu có cột UpdatedAt

        await _folderRepository.UpdateAsync(existingFolder);
        return existingFolder;
    }
    public async Task<IEnumerable<FolderDto>> GetFoldersByUserAsync(Guid ownerId)
    {
        var folders = await _folderRepository.GetRootFoldersByUserAsync(ownerId);
        return _mapper.Map<IEnumerable<FolderDto>>(folders);
    }



    public async Task<bool> DeleteFolderAsync(Guid id)
    {
        var folder = await _folderRepository.GetByIdAsync(id);
        if (folder == null) return false;
        await _folderRepository.DeleteAsync(folder);
        return true;
    }

    // Phương thức mới: Lấy cây thư mục lồng nhau cho owner
    public async Task<IEnumerable<FolderTreeDto>> GetFolderTreeAsync(Guid ownerId)
    {
        var allFolders = (await GetFoldersByOwnerAsync(ownerId)).ToList();

        // Bước 1: map sang DTO (giữ nguyên ParentFolderId và FolderId)
        var dtoList = allFolders.Select(f => new FolderTreeDto
        {
            FolderId = f.FolderId,
            Name = f.Name,
            ParentFolderId = f.ParentFolderId,
            OwnerId = f.OwnerId,
            Children = new List<FolderTreeDto>()
        }).ToList();

        // Bước 2: tạo dictionary để dễ truy cập
        var folderDict = dtoList.ToDictionary(f => f.FolderId);

        // Bước 3: xây dựng cấu trúc cây
        List<FolderTreeDto> rootFolders = new();

        foreach (var folder in dtoList)
        {
            if (folder.ParentFolderId.HasValue && folderDict.ContainsKey(folder.ParentFolderId.Value))
            {
                folderDict[folder.ParentFolderId.Value].Children.Add(folder);
            }
            else
            {
                rootFolders.Add(folder);
            }
        }

        return rootFolders;

    }

    public async Task<Folder?> GetByIdAsync(Guid folderId)
    {
        return await _folderRepository.GetByIdAsync(folderId);
    }

    public async Task<IEnumerable<FolderDto>> GetByParentAsync(Guid? parentId)
    {
        var folders = await _folderRepository.GetByParentAsync(parentId);
        return folders.Adapt<IEnumerable<FolderDto>>();
    }
}
