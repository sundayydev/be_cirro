using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.DTOs;
using BE_CIRRO.Shared.DTOs.Folder;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.Mapping
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Map User → UserDto
            config.NewConfig<User, UserDto>();

            // Map Folder → FolderDto (custom field OwnerName)
            config.NewConfig<Folder, FolderDto>()
                .Map(dest => dest.OwnerName, src => src.Owner != null ? src.Owner.Username : "Unknown");
        }
    }
}
