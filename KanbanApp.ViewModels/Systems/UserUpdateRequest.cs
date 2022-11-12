using System;
using Microsoft.AspNetCore.Http;

namespace KanbanApp.ViewModels.Systems
{
    public class UserUpdateRequest
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public IFormFile AvatarUrl { get; set; }

    }
}
