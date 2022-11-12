using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanApp.ViewModels.Contents
{
    public class InfoProject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int TaskDone { get; set; }
        public int TaskProgress { get; set; }
        public int TaskBackLog{ get; set; }

    }
}
