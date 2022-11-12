using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace KanbanApp.ViewModels.Contents
{
    public class IssueUpdateRequest
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Sample { get; set; }
        public string ProjectId { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public string ReporterId { get; set; }
        public string[] UserIds { get; set; }
        public StatusVm Status { get; set; }
        public CommentVm[] Comments { get; set; }
        public AttachmentVm[] Attachments { get; set; }
        public string CreateDate { get; set; }
        public string LastModifiedDate { get; set; }
        public int ListPosition { get; set; }
        public string[] Labels { get; set; }
    }
}
