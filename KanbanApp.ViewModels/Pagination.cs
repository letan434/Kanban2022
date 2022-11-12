using System;
using System.Collections.Generic;

namespace KanbanApp.ViewModels
{
    public class Pagination<T> : PaginationBase where T : class
    {
        public List<T> Items { get; set; }
    }
}
