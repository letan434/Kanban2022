using System;
namespace KanbanApp.ViewModels.Contents
{
    public class StatusCreateRequest
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
