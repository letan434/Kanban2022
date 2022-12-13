using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KanbanApp.BackendServer.Constants;
using KanbanApp.BackendServer.Data.Interfaces;

namespace KanbanApp.BackendServer.Data.Entities
{
    [Table("Issues")]
    public class Issue : IDateTracking
    {
        [Key]
        public string Id { get; set; }

        [MaxLength(50)]
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string StatusId { get; set; }

        [MaxLength(50)]
        [Required]
        [Column(TypeName = "varchar(50)")]
        public Sample Sample { get; set; }

        [MaxLength(50)]
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ReporterId { get; set; }

        [MaxLength(500)]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public int Estimate { get; set; }

        public int TimeSpent { get; set; }
        public int TimeRemaining { get; set; }

        public int ListPosition { get; set; }

        public string Labels { get; set; }

        public int? NumberOfComments { get; set; }
        //public DateTime DealineDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}
