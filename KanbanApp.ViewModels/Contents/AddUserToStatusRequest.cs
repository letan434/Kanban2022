using System;
namespace KanbanApp.ViewModels.Contents
{
	public class AddUserToStatusRequest
	{
		public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string StatusId { get; set; }
        public string StatusName { get; set; }

    }
}

