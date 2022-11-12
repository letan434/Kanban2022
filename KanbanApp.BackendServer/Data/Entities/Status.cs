using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KanbanApp.BackendServer.Data.Interfaces;

namespace KanbanApp.BackendServer.Data.Entities
{
    [Table("Statuses")]
    public class Status : IDateTracking
    {
        [Key]
        public string Id { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Required]
        public string ProjectId { get; set; }

        [MaxLength(500)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int? Limit { get; set; }

        public int ListPosition { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}
