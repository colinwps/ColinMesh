using ColinApp.Auth.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColinApp.Auth.Models.System
{
    /// <summary>
    /// 用户token刷新
    /// </summary>
    [Table("refreshtokens")]
    public class RefreshToken : BaseEntity
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Token { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string? ReplacedByToken { get; set; }
        public string? UserId { get; set; }
    }
}
