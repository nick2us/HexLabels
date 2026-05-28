using System.ComponentModel.DataAnnotations;

namespace HexLabels.Api.Core.Data.Models
{

    public enum ApiKeyType
    {
        Vendor,
        Company
    }

    public class ApiKey : BaseModel
    {
        [Key]
        public Guid ApiKeyId { get; set; }
        public Guid Key { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual required Company Company { get; set; }
    }
}
