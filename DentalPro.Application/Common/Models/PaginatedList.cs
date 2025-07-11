using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Application.Common.Models
{
    /// <summary>
    /// Representa una lista paginada de elementos
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos en la lista</typeparam>
    public class PaginatedList<T>
    {
        /// <summary>
        /// Lista de elementos de la página actual
        /// </summary>
        public List<T> Items { get; }
        
        /// <summary>
        /// Número de página actual
        /// </summary>
        public int PageNumber { get; }
        
        /// <summary>
        /// Tamaño de la página (cantidad de elementos por página)
        /// </summary>
        public int PageSize { get; }
        
        /// <summary>
        /// Cantidad total de páginas
        /// </summary>
        public int TotalPages { get; }
        
        /// <summary>
        /// Cantidad total de elementos
        /// </summary>
        public int TotalCount { get; }
        
        /// <summary>
        /// Indica si hay una página anterior
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
        
        /// <summary>
        /// Indica si hay una página siguiente
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Constructor de la lista paginada
        /// </summary>
        public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            Items = items;
        }

        /// <summary>
        /// Crea una lista paginada a partir de una fuente de datos IQueryable
        /// </summary>
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageNumber, int pageSize, 
            Func<IQueryable<T>, Task<int>> countFunc, 
            Func<IQueryable<T>, Task<List<T>>> toListFunc)
        {
            var totalCount = await countFunc(source);
            
            // Ajuste de número de página
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;
            
            var items = await toListFunc(source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize));

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
        
        /// <summary>
        /// Crea una lista paginada vacía
        /// </summary>
        public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PaginatedList<T>(new List<T>(), 0, pageNumber, pageSize);
        }
        
        /// <summary>
        /// Crea una lista paginada a partir de una colección en memoria
        /// </summary>
        public static PaginatedList<T> Create(
            IEnumerable<T> source, int totalCount, int pageNumber, int pageSize)
        {
            // Ajuste de número de página
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;
            
            var items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}
