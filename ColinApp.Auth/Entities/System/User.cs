using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColinApp.Auth.Entities.System
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("主键ID")]
        public long Id { get; set; }

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

        [Column("CreatedId")]
        [StringLength(255)]
        [Comment("创建人编号")]
        public string? CreatedId { get; set; }

        [Column("CreatedName")]
        [StringLength(255)]
        [Comment("创建人姓名")]
        public string? CreatedName { get; set; }

        [Column("CreatedAt")]
        [Comment("创建时间")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column("UpdatedId")]
        [StringLength(255)]
        [Comment("修改人编号")]
        public string? UpdatedId { get; set; }

        [Column("UpdatedName")]
        [StringLength(255)]
        [Comment("修改人姓名")]
        public string? UpdatedName { get; set; }

        [Column("UpdatedAt")]
        [Comment("更新时间")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
