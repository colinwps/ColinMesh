using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ColinApp.Auth.Entities.Base
{
    /// <summary>
    /// 基础类
    /// </summary>
    public abstract class BaseEntity
    {
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
