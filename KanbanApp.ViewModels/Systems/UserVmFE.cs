using System;
using System.Collections.Generic;

namespace KanbanApp.ViewModels.Systems
{
    public class UserVmFE
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string UserNameMain { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string CreateDate { get; set; }
        public string LastModifiedDate { get; set; }
        public string[] IssueIds { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class UserIndexIssue
    {
        public string Index { get; set; }

        public string[] IssueIds { get; set; }
    }


}