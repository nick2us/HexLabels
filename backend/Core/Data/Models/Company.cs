using HexLabels.Api.Core.Data.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace HexLabels.Api.Core.Data.Models
{

    public class CompanyNotFoundException(Guid? companyId) : NotFoundException($"Company {companyId} was not found.");

    public class Company : BaseModel
    {
        [Key]
        public Guid ID { get; set; }
        public string? Name { get; set; }
        public List<UserRoles> UserRoles { get; set; } = [];
    }
}
