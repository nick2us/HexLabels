using System.ComponentModel.DataAnnotations;
using HexLabels.Api.Core.Data.Exceptions;

namespace HexLabels.Api.Core.Data.Models
{

    public class UserNotFoundException(Guid userId) : NotFoundException($"User {userId} was not found.");

    public enum UserFlags
    {
        IsActive = 1 << 0,
        IsVerified = 1 << 1,
        IsSuperAdmin = 1 << 2
    }

    public class User : BaseModel
    {
        [Key]
        public Guid ID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public UserFlags Flags { get; set; }

        public virtual List<UserRoles> UserRole { get; set; } = [];
        public virtual List<Company> Companies { get; set; } = [];
    }
}
