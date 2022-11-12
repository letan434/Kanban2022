using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanApp.BackendServer.Data.Entities
{
    [Table("LabelInIssues")]
    public class LabelInIssue
    {
        public string IssueId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string LabelId { get; set; }
    }
}
