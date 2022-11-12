using System;
namespace KanbanApp.ViewModels.Contents
{
    public class CategoryCreateRequest
    {
        public string Name { get; set; }

        public string SeoAlias { get; set; }

        public string SeoDescription { get; set; }
    }
}
