using System;
namespace KanbanApp.ViewModels.Contents
{
    public class CategoryVm
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SeoAlias { get; set; }

        public string SeoDescription { get; set; }
    }
}
