using HexLabels.Api.Core.Data.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace HexLabels.Api.Core.Data.Models
{

    public class DepartmentNotFoundException(Guid? departmentId) : NotFoundException($"Department {departmentId} was not found.");

    public class Department : BaseModel
    {
        [Key]
        public Guid ID { get; set; }
        public string? Name { get; set; }
        public List<UserRoles> UserRoles { get; set; } = [];
    }
}
