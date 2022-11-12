using Microsoft.AspNetCore.Http;
using System;
namespace KanbanApp.ViewModels.Contents
{
    public class ProjectCreateRequest
    {

        public string Id { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile AvatarUrl { get; set; }
    }
}
