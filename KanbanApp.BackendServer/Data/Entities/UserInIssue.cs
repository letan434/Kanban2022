using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanApp.BackendServer.Data.Entities
{
    [Table("UserInIssues")]
    public class UserInIssue
    {
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Required]
        public string UserId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Required]
        public string IssueId { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Required]
        public string ProjectId { get; set; }
    }
}
