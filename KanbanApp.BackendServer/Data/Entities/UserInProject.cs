using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanApp.BackendServer.Data.Entities
{
    [Table("UserInProjects")]
    public class UserInProject
    {
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string UserId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string ProjectId { get; set; }

        public string RoleStatuses { get; set; }
        public string RoleStatusesName { get; set; }
        public bool AllRoles { get; set; }

    }
}
