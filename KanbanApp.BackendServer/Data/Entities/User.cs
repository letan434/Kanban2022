using System;
using System.ComponentModel.DataAnnotations;
using KanbanApp.BackendServer.Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KanbanApp.BackendServer.Data.Entities
{
    public class User : IdentityUser, IDateTracking
    {
        public User()
        {
        }
        public User(string id, string userName, string firstName, string lastName,
            string email, string phoneNumber, DateTime dob, string avatarUrl)
        {
            Id = id;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Dob = dob;
            AvatarUrl = avatarUrl;
        }
        [MaxLength(50)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(50)]
        [Required]
        public string LastName { get; set; }

        public string AvatarUrl { get; set; }
        [Required]
        public DateTime Dob { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}
