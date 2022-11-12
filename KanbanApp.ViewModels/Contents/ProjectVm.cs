using System;
using System.Collections.Generic;
using KanbanApp.ViewModels.Systems;

namespace KanbanApp.ViewModels.Contents
{
    public class ProjectVm
    {
        public string Id { get; set; }

        public int CategoryId { get; set; }

        public string OwnerUserId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<StatusVm> Statuses { get; set; }

        public List<UserVmFE> Users { get; set; }

        public List<IssueQuickVm> Issues { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }
        public string AvatarUrl { get; set; }
    }
}
