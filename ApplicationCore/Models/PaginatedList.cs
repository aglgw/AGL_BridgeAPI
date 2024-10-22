using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; init; }
        public int PageIndex { get; init; }
        public int TotalPages { get; init; }
        public int TotalCount { get; init; }
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = new List<T>();

            if (pageSize > 0)
            {
                items = await source.Skip(pageIndex).Take(pageSize).ToListAsync();
            }
            else
            {
                items = await source.ToListAsync();
            }

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = new List<T>();

            if (pageSize > 0)
            {
                items = source.Skip(pageIndex).Take(pageSize).ToList();
            }
            else
            {
                items = source.ToList();
            }

            await Task.CompletedTask;
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
