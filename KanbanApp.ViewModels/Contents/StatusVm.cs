using System;
namespace KanbanApp.ViewModels.Contents
{
    public class StatusVm
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool NoDisabled { get; set; }
    }
}
