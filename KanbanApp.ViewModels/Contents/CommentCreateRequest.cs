using System;
namespace KanbanApp.ViewModels.Contents
{
    public class CommentCreateRequest
    {
        public string Body { get; set; }

        public string IssueId { get; set; }
    }
}
