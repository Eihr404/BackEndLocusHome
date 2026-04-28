using System.Collections.Generic;

namespace Microservicio.Cliente.DatAccess.Common
{
    public class PagedResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        
        // Calcula dinámicamente el total de páginas necesarias
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}
