using System;
using Microsoft.AspNetCore.Http;

namespace KanbanApp.ViewModels.Systems
{
    public class UserCreateRequest
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Dob { get; set; }

        public string AvatarUrl { get; set; }
    }
}
