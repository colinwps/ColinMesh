using ColinApp.Auth.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColinApp.Auth.Models.System
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    [Table("users")]
    public class User : BaseEntity
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("主键ID")]
        public long Id { get; set; }

        [Column("Guid")]
        [StringLength(255)]
        [Comment("唯一编号")]
        public string Guid {  get; set; }

        [Required]
        [Column("UserId")]
        [StringLength(255)]
        [Comment("用户编号")]
        public string? UserId { get; set; }

        [Column("UserLoginName")]
        [StringLength(255)]
        [Comment("登录名")]
        public string? UserLoginName { get; set; }

        [Column("UserName")]
        [StringLength(50)]
        [Comment("用户名")]
        public string? UserName { get; set; }

        [Column("Email")]
        [StringLength(100)]
        [Comment("邮箱地址")]
        public string? Email { get; set; }

        [Column("Phone")]
        [StringLength(20)]
        [Comment("手机号")]
        public string? Phone { get; set; }

        [Column("PasswordHash")]
        [StringLength(255)]
        [Comment("密码哈希")]
        public string? PasswordHash { get; set; }

        [Column("Avatar")]
        [StringLength(255)]
        [Comment("头像URL")]
        public string? Avatar { get; set; }

        [Column("Status")]
        [Comment("状态：1=正常，0=禁用")]
        public byte? Status { get; set; } = 1;

        [Column("IsDeleted")]
        [Comment("是否已删除：0=否，1=是")]
        public byte? IsDeleted { get; set; } = 0;

        
    }
}
