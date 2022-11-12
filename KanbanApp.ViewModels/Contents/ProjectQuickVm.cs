using System;
namespace KanbanApp.ViewModels.Contents
{
    public class ProjectQuickVm
    {
        public string Id { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}
