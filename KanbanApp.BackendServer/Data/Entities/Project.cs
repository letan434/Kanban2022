using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KanbanApp.BackendServer.Data.Interfaces;

namespace KanbanApp.BackendServer.Data.Entities
{
    [Table("Projects")]
    public class Project : IDateTracking
    {
        [Key]
        public string Id { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        [MaxLength(500)]
        [Required]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OwnerUserId { get; set; }

        public string AvatarUrl { get; set; }

        public DateTime CreateDate { get ; set ; }

        public DateTime? LastModifiedDate { get ;set ; }
    }
}
