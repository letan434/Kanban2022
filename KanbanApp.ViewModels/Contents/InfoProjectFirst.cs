using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanApp.ViewModels.Contents
{
    public class InfoProjectFirst
    {
        public string ProjectId { get; set; }
        public string StatusName { get; set; }

    }
    public class InfoProjectFull
    {
        public string ProjectId { get; set; }
        public string StatusName { get; set; }
        public string IssueId { get; set; }

        public string IssueName { get; set; }


    }
    public class CountNewDashboard
    {
        public int ProjectCount { get; set; }
        public int IssueCount { get; set; }
        public int CommentCount { get; set; }

        public int UserCount { get; set; }
        public CountNewDashboard(int projectCount, int issueCount, int commentCount, int userCount)
        {
            this.CommentCount = commentCount;
            this.ProjectCount = projectCount;
            this.UserCount = userCount;
            this.IssueCount = issueCount;
        }
    }

}
