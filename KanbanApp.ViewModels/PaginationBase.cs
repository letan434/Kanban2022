using System;
namespace KanbanApp.ViewModels
{
    public class PaginationBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        //Calculated properties
        public int PageCount
        {
            get
            {
                var pageCount = (double)TotalRecords / PageSize;
                return (int)Math.Ceiling(pageCount);
            }
            //set
            //{
            //    if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            //    PageCount = value;
            //}
        }
    }
}
