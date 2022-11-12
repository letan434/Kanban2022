using System;
using System.Collections.Generic;

namespace KanbanApp.ViewModels.Contents
{
    public class IssueVm
    {
        public string Id { get; set; }

        
        public string StatusId { get; set; }

        
        public string Sample { get; set; }

        public string ReporterId { get; set; }

       
        public string Title { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public int ListPosition { get; set; }

        public string[] Labels { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }
        public List<AttachmentVm> Attachments { set; get; }

    }
}
