using System;
using System.Collections.Generic;
using KanbanApp.ViewModels.Systems;

namespace KanbanApp.ViewModels.Contents
{
    public class IssueQuickVm
    {
        public string Id { get; set; }


        public StatusVm Status { get; set; }


        public string Sample { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public int ListPosition { get; set; }

        public int Estimate { get; set; }

        public int TimeSpent { get; set; }
        public int TimeRemaining { get; set; }
        public string ProjectId { get; set; }
        public string ReporterId { get; set; }

        public string[] Labels { get; set; }
        public string[] UserIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<AttachmentVm> Attachments { get; set; }

        public List<CommentVm> Comments { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

    }
}
