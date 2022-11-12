using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanApp.ViewModels.Contents
{
    public class IssueAttachmentsRequest
    {
        public IFormFile File { get; set; }
        public string Id { get; set; }

    }
}
