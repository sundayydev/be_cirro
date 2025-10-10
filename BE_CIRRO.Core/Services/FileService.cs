using BE_CIRRO.Core.Repositories;
using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs.File;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Services;

public class FileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileVersionRepository _fileVersionRepo;
    private readonly S3FileService _s3FileService;
    private IMapper _mapper;

    public FileService(IFileRepository fileRepository, S3FileService s3FileService, IFileVersionRepository fileVersionRepo)
    {
        _fileRepository = fileRepository;
        _s3FileService = s3FileService;
        _mapper = new Mapper();
        _fileVersionRepo = fileVersionRepo;
    }


    public async Task<IEnumerable<FileDto>> GetByFolderAsync(Guid folderId)
    {
        var file = await _fileRepository.GetByFolderAsync(folderId);
        return _mapper.Map<IEnumerable<FileDto>>(file);
    }

    public async Task<FileDto?> GetByIdAsync(Guid id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        return _mapper.Map<FileDto?>(file);
    }

    public async Task<FileDto> UploadAsync(IFormFile file, Guid ownerId, Guid folderId)
    {
        try
        {
            // 🧩 B1: Kiểm tra file trùng tên trong cùng folder
            var existingFile = await _fileRepository
                .FirstOrDefaultAsync(f => f.OwnerId == ownerId
                             && f.FolderId == folderId
                             && f.Name == file.FileName);

            // 🧩 B2: Upload file lên S3 (luôn tạo key mới để không ghi đè)
            var fileUrl = await _s3FileService.UploadFileAsync(file, ownerId, folderId);

            if (existingFile == null)
            {
                // 🆕 Lần đầu upload file này → tạo File + Version 1
                var newFile = new FileDto
                {
                    FileId = Guid.NewGuid(),
                    Name = file.FileName,
                    FilePath = fileUrl,
                    FileType = file.ContentType,
                    Size = (int)file.Length,
                    FolderId = folderId,
                    OwnerId = ownerId,
                };

                var newFileEntity = _mapper.Map<Domain.Models.File>(newFile);
                await _fileRepository.AddAsync(newFileEntity);

                var firstVersion = new FileVersion
                {
                    VersionId = Guid.NewGuid(),
                    FileId = newFile.FileId,
                    VersionNumber = 1,
                    File = newFileEntity,
                    FilePath = fileUrl,
                    Size = (int)file.Length,
                    CreatedAt = DateTime.UtcNow
                };

                await _fileVersionRepo.AddAsync(firstVersion);

                return newFile;
            }
            else
            {
                //Nếu tên file trùng → tạo version mới
                var latestVersion = await _fileVersionRepo
                    .GetLatestVersionAsync(existingFile.FileId);

                var nextVersionNumber = (latestVersion?.VersionNumber ?? 0) + 1;

                var newVersion = new FileVersion
                {
                    VersionId = Guid.NewGuid(),
                    FileId = existingFile.FileId,
                    VersionNumber = nextVersionNumber,
                    File = existingFile,
                    FilePath = fileUrl,
                    Size = (int)file.Length,
                    CreatedAt = DateTime.UtcNow
                };

                await _fileVersionRepo.AddAsync(newVersion);

                // 🔄 Cập nhật đường dẫn mới nhất vào bảng File
                existingFile.FilePath = fileUrl;
                existingFile.Size = (int)file.Length;
                await _fileRepository.UpdateAsync(existingFile);

                // Map sang DTO để trả về
                var updatedDto = _mapper.Map<FileDto>(existingFile);
                return updatedDto;
            }
        }
        catch (Amazon.S3.AmazonS3Exception s3Ex)
        {
            throw new Exception($"⚠️ AWS S3 upload error: {s3Ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"❌ Upload failed: {ex.Message}");
        }
    }


    public async Task<Stream?> GetFileStreamAsync(Guid id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null) return null;

        return await _s3FileService.GetFileStreamAsync(file.OwnerId, file.FolderId, file.Name);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null) return false;

        await _s3FileService.DeleteFileAsync(file.OwnerId, file.FolderId, file.Name);
        await _fileRepository.DeleteAsync(file);
        return true;
    }

}
