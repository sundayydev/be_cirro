namespace BE_CIRRO.Shared.DTOs.Permission
{
    public class PermissionCreateByEmailDto
    {
        public string Email { get; set; } // Email của người dùng
        public Guid FileId { get; set; }  // ID của file (nếu chia sẻ file)
        public Guid? FolderId { get; set; } // ID của folder (nếu chia sẻ folder)
        public string PermissionType { get; set; }  = default!;
    }
}