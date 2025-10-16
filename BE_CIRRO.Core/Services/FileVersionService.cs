using BE_CIRRO.Core.Repositories;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs.FileVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Services;

public class FileVersionService
{
    private readonly IFileVersionRepository _fileVersionRepo;
    private readonly IFileRepository _fileRepo;
    private readonly S3FileService _s3Service;

    public FileVersionService(IFileVersionRepository fileVersionRepo, IFileRepository fileRepo, S3FileService s3Service)
    {
        _fileVersionRepo = fileVersionRepo;
        _fileRepo = fileRepo;
        _s3Service = s3Service;
    }

    public async Task<IEnumerable<FileVersion>> GetByFileAsync(Guid fileId)
    {
        return await _fileVersionRepo.GetByFileAsync(fileId);
    }

    public async Task<FileVersion?> GetByIdAsync(Guid id)
    {
        return await _fileVersionRepo.GetByIdAsync(id);
    }

    public async Task<FileVersion?> UploadVersionAsync(FileVersionCreateDto dto)
    {
        var file = await _fileRepo.GetByIdAsync(dto.FileId);
        if (file == null)
            throw new Exception("File not found");

        // Tạo version number: V{N}
        var existingVersions = await _fileVersionRepo.GetByFileAsync(dto.FileId);
        var versionNumber = existingVersions.Count() + 1;

        // Upload version lên S3
        var url = await _s3Service.UploadFileAsync(dto.File, file.OwnerId, file.FolderId);

        var newVersion = new FileVersion
        {
            VersionId = Guid.NewGuid(),
            FileId = dto.FileId,
            File = file, // Fix: set required 'File' property
            VersionNumber = versionNumber,
            FilePath = url,
            Size = (int)dto.File.Length,
            CreatedAt = DateTime.UtcNow
        };

        await _fileVersionRepo.AddAsync(newVersion);

        return newVersion;
    }

    public async Task<Stream?> GetFileVersionStreamAsync(Guid versionId)
    {
        var version = await _fileVersionRepo.GetByIdAsync(versionId);
        if (version == null) return null;

        var file = await _fileRepo.GetByIdAsync(version.FileId);
        if (file == null) return null;

        var fileName = $"{file.Name}_({version.VersionNumber})";
        return await _s3Service.GetFileStreamAsync(file.OwnerId, file.FolderId, fileName);
    }

    // Lấy tất cả phiên bản file trong hệ thống
    public async Task<IEnumerable<FileVersion>> GetAllAsync()
    {
        return await _fileVersionRepo.GetAllAsync();
    }

    // Xóa một phiên bản file cụ thể
    public async Task<bool> DeleteAsync(Guid versionId)
    {
        var version = await _fileVersionRepo.GetByIdAsync(versionId);
        if (version == null) return false;

        // Xóa file từ S3
        var file = await _fileRepo.GetByIdAsync(version.FileId);
        if (file != null)
        {
            var fileName = $"{file.Name}_({version.VersionNumber})";
            await _s3Service.DeleteFileAsync(file.OwnerId, file.FolderId, fileName);
        }

        await _fileVersionRepo.DeleteAsync(version);
        return true;
    }

    // Cập nhật thông tin phiên bản file
    public async Task<FileVersion?> UpdateAsync(Guid versionId, FileVersionUpdateDto dto)
    {
        var version = await _fileVersionRepo.GetByIdAsync(versionId);
        if (version == null) return null;

        // Cập nhật các thông tin có thể thay đổi
        if (dto.VersionNumber.HasValue)
        {
            version.VersionNumber = dto.VersionNumber.Value;
        }

        version.CreatedAt = dto.UpdatedAt ?? DateTime.UtcNow;

        await _fileVersionRepo.UpdateAsync(version);
        return version;
    }

    // Lấy phiên bản mới nhất của file
    public async Task<FileVersion?> GetLatestVersionAsync(Guid fileId)
    {
        return await _fileVersionRepo.GetLatestVersionAsync(fileId);
    }

    // Khôi phục file về phiên bản cụ thể
    public async Task<bool> RestoreVersionAsync(Guid versionId)
    {
        var version = await _fileVersionRepo.GetByIdAsync(versionId);
        if (version == null) return false;

        var file = await _fileRepo.GetByIdAsync(version.FileId);
        if (file == null) return false;

        // Tạo phiên bản mới từ phiên bản được chọn để khôi phục
        var restoreVersion = new FileVersion
        {
            VersionId = Guid.NewGuid(),
            FileId = version.FileId,
            File = file,
            VersionNumber = (await _fileVersionRepo.GetByFileAsync(file.FileId)).Count() + 1,
            FilePath = version.FilePath, // Sử dụng cùng đường dẫn file
            Size = version.Size,
            CreatedAt = DateTime.UtcNow
        };

        await _fileVersionRepo.AddAsync(restoreVersion);
        return true;
    }
}
