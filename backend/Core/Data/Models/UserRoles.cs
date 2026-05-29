using System.ComponentModel.DataAnnotations;
using HexLabels.Api.Core.Data.Models;

namespace HexLabels.Api.Core.Data.Models
{

    [Flags]
    public enum UserRoleTypes
    {
        Admin = 1 << 0,
        Employee = 1 << 1,
        User = 1 << 2,
        Viewer = 1 << 3,

        AllowedToSend = Admin | Employee,
        Superuser = Admin | Employee | User | Viewer,
    }

    public class UserRoles : BaseModel
    {

        [Key]
        public Guid UserRoleId { get; set; }
        public virtual required User User { get; set; }
        public virtual required Company Company { get; set; }
        public virtual required Department Department { get; set; }
        public virtual required UserRoleTypes Role { get; set; }

    }
}
