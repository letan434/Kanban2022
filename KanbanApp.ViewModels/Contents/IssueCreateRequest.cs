using System;
namespace KanbanApp.ViewModels.Contents
{
    public class IssueCreateRequest
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
        public string CreateDate { get; set; }
    }
}