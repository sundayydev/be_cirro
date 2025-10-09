using BE_CIRRO.Shared.Common;

namespace BE_CIRRO.Shared.DTOs;

public class UserDto : DTOBase
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
}
