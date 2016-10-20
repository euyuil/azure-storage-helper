using System.Collections.Generic;

namespace Euyuil.Azure.Storage.Helper
{
    public class PagedList<T>
    {
        public PagedList()
        {
            Items = new List<T>();
        }

        public PagedList(IEnumerable<T> items)
        {
            Items = new List<T>(items);
        }

        public PagedList(IEnumerable<T> items, string paginationToken)
        {
            Items = new List<T>(items);
            PaginationToken = paginationToken;
        }

        public List<T> Items { get; set; }

        public string PaginationToken { get; set; }
    }
}
