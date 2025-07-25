
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugUserPreventionUI.Models.Common
{
    /// <summary>
    /// Paginated API response
    /// </summary>
    public class PaginatedApiResponse<T> : ApiResponse<IReadOnlyCollection<T>>
    {
        public PaginationInfo Pagination { get; set; } = new();

        public static PaginatedApiResponse<T> SuccessResult(
            BasePaginatedList<T> paginatedData,
            string message = "Success")
        {
            return new PaginatedApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = paginatedData.Items,
                Pagination = new PaginationInfo
                {
                    CurrentPage = paginatedData.CurrentPage,
                    TotalPages = paginatedData.TotalPages,
                    PageSize = paginatedData.PageSize,
                    TotalItems = paginatedData.TotalItems,
                    HasNextPage = paginatedData.HasNextPage,
                    HasPreviousPage = paginatedData.HasPreviousPage
                }
            };
        }
    }
    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
