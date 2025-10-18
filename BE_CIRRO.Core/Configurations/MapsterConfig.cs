using Mapster;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs.Permission;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Permission, PermissionDto>
            .NewConfig()
            .Map(dest => dest.OwnerName, 
                src => src.File != null && src.File.Owner != null 
                    ? src.File.Owner.Username 
                    : string.Empty);
    }
}