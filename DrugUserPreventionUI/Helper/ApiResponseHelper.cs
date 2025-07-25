using System.Text.Json;
using DrugUserPreventionUI.Models.Common;

namespace DrugUserPreventionUI.Helpers
{
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Safely deserialize API response with detailed error logging
        /// </summary>
        public static async Task<T?> SafeDeserializeAsync<T>(
            HttpResponseMessage response,
            JsonSerializerOptions jsonOptions,
            ILogger logger
        )
            where T : class
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    logger.LogWarning("Empty response content");
                    return null;
                }

                logger.LogDebug(
                    "Deserializing response of type {Type}, content length: {Length}",
                    typeof(T).Name,
                    content.Length
                );

                return JsonSerializer.Deserialize<T>(content, jsonOptions);
            }
            catch (JsonException jsonEx)
            {
                logger.LogError(
                    jsonEx,
                    "JSON deserialization failed for type {Type}",
                    typeof(T).Name
                );
                var contentPreview = await GetContentPreview(response);
                logger.LogError("Response content preview: {Preview}", contentPreview);
                throw;
            }
        }

        /// <summary>
        /// Get a safe preview of response content for logging
        /// </summary>
        public static async Task<string> GetContentPreview(
            HttpResponseMessage response,
            int maxLength = 1000
        )
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                    return "[Empty]";

                return content.Length > maxLength
                    ? content.Substring(0, maxLength) + "..."
                    : content;
            }
            catch
            {
                return "[Error reading content]";
            }
        }

        /// <summary>
        /// Validate PaginatedApiResponse structure
        /// </summary>
        public static bool ValidatePaginatedResponse<T>(
            PaginatedApiResponse<T>? response,
            ILogger logger
        )
        {
            if (response == null)
            {
                logger.LogWarning("API response is null");
                return false;
            }

            if (response.Data == null)
            {
                logger.LogWarning("API response data is null");
                return false;
            }

            if (response.Pagination == null)
            {
                logger.LogWarning("API response pagination is null");
                // Don't return false here, as data might still be valid
            }

            return true;
        }

        /// <summary>
        /// Create default pagination info when API doesn't provide it
        /// </summary>
        public static PaginationInfo CreateDefaultPagination<T>(
            IList<T> data,
            int pageIndex = 1,
            int pageSize = 12
        )
        {
            var totalItems = data?.Count ?? 0;
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalItems / pageSize));

            return new PaginationInfo
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasPreviousPage = pageIndex > 1,
                HasNextPage = pageIndex < totalPages,
            };
        }

        /// <summary>
        /// Try to extract error message from API response
        /// </summary>
        public static async Task<string> ExtractErrorMessage(
            HttpResponseMessage response,
            JsonSerializerOptions jsonOptions
        )
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return "Không có thông tin lỗi.";

                // Try to parse as standard API error response
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
                    content,
                    jsonOptions
                );
                if (!string.IsNullOrWhiteSpace(errorResponse?.Message))
                    return errorResponse.Message;

                // Try to parse as simple error object
                var simpleError = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    content,
                    jsonOptions
                );
                if (simpleError != null)
                {
                    // Look for common error message fields
                    var messageFields = new[] { "message", "error", "detail", "title" };
                    foreach (var field in messageFields)
                    {
                        if (simpleError.TryGetValue(field, out var value) && value != null)
                            return value.ToString() ?? "";
                    }
                }

                // If all else fails, return the raw content (truncated)
                return content.Length > 200 ? content.Substring(0, 200) + "..." : content;
            }
            catch
            {
                return $"Lỗi HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
            }
        }

        /// <summary>
        /// Check if response indicates authentication/authorization issues
        /// </summary>
        public static bool IsAuthenticationError(HttpResponseMessage response)
        {
            return response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                || response.StatusCode == System.Net.HttpStatusCode.Forbidden;
        }

        /// <summary>
        /// Get user-friendly error message based on HTTP status code
        /// </summary>
        public static string GetUserFriendlyErrorMessage(System.Net.HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized =>
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                System.Net.HttpStatusCode.Forbidden => "Bạn không có quyền truy cập tính năng này.",
                System.Net.HttpStatusCode.NotFound => "Không tìm thấy dữ liệu yêu cầu.",
                System.Net.HttpStatusCode.BadRequest =>
                    "Yêu cầu không hợp lệ. Vui lòng kiểm tra lại thông tin.",
                System.Net.HttpStatusCode.InternalServerError =>
                    "Lỗi máy chủ. Vui lòng thử lại sau.",
                System.Net.HttpStatusCode.ServiceUnavailable =>
                    "Dịch vụ tạm thời không khả dụng. Vui lòng thử lại sau.",
                System.Net.HttpStatusCode.RequestTimeout => "Yêu cầu bị timeout. Vui lòng thử lại.",
                _ => $"Lỗi kết nối (Mã: {(int)statusCode}). Vui lòng thử lại sau.",
            };
        }
    }
}
