using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    /// <summary>
    /// Base DTO for pagination requests
    /// </summary>
    public class BasePaginationDto
    {
        private int _pageIndex = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Items per page (1-100)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
        }
    }
    }