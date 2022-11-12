using KanbanApp.ViewModels.Systems;
using System;
namespace KanbanApp.ViewModels.Contents
{
    public class CommentVm
    {
        public string Id { get; set; }

        public string Body { get; set; }

        public string IssueId { get; set; }

        public string UserId { get; set; }
        public UserVmFE User { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}