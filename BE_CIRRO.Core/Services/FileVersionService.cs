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
}
