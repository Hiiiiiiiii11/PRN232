using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DrugUserPreventionUI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string AUTH_API_URL = "https://localhost:7045";

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UserLoginRequest LoginForm { get; set; } = new UserLoginRequest();

        public string? Message { get; set; }
        public string? MessageType { get; set; }

        public async Task<IActionResult> OnGetAsync(
            string? message = null,
            string? messageType = null,
            string? returnUrl = null
        )
        {
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
                MessageType = messageType ?? "info";
            }

            // Store returnUrl in ViewData for use in POST
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewData["ReturnUrl"] = returnUrl;
            }

            // Check if already logged in by checking and decoding token
            var token = HttpContext.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                var userInfo = DecodeJwtToken(token);
                if (userInfo != null && IsTokenValid(token))
                {
                    // User is already logged in, redirect to returnUrl or appropriate role page
                    if (!string.IsNullOrEmpty(returnUrl) && IsValidReturnUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToRoleBasedPage(userInfo.Role);
                }
                else
                {
                    // Token is invalid or expired, clear it
                    Response.Cookies.Delete("auth_token");
                    Console.WriteLine("Invalid or expired token found, cleared it");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                // Preserve returnUrl in ViewData if validation fails
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    ViewData["ReturnUrl"] = returnUrl;
                }
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var json = JsonSerializer.Serialize(
                    LoginForm,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var loginUrl = $"{AUTH_API_URL}/login";

                Console.WriteLine($"=== LOGIN DEBUG ===");
                Console.WriteLine($"URL: {loginUrl}");
                Console.WriteLine($"Payload: {json}");

                var response = await client.PostAsync(loginUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var loginResponse = JsonSerializer.Deserialize<TokenResponseDto>(
                            responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (
                            loginResponse != null
                            && !string.IsNullOrEmpty(loginResponse.AccessToken)
                        )
                        {
                            // Decode JWT token to verify it contains user info
                            var userInfo = DecodeJwtToken(loginResponse.AccessToken);

                            if (userInfo != null)
                            {
                                // Set all necessary session data
                                HttpContext.Session.SetString("user_id", userInfo.UserId.ToString());
                                HttpContext.Session.SetString("user_name", userInfo.UserName ?? "");
                                HttpContext.Session.SetString("user_email", userInfo.Email ?? "");
                                HttpContext.Session.SetString("user_role", userInfo.Role ?? "");
                                
                                // Only store the JWT token in cookie
                                var cookieOptions = new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = Request.IsHttps,
                                    SameSite = SameSiteMode.Lax,
                                    Expires = userInfo.Expiration ?? DateTime.UtcNow.AddHours(8),
                                };

                                Response.Cookies.Append(
                                    "auth_token",
                                    loginResponse.AccessToken,
                                    cookieOptions
                                );

                                Console.WriteLine($"=== LOGIN SUCCESS ===");
                                Console.WriteLine(
                                    $"Token: {loginResponse.AccessToken[..Math.Min(20, loginResponse.AccessToken.Length)]}..."
                                );
                                Console.WriteLine($"User: {userInfo.UserName} ({userInfo.Role})");
                                Console.WriteLine($"Email: {userInfo.Email}");
                                Console.WriteLine($"UserId: {userInfo.UserId}");
                                Console.WriteLine($"Expires: {userInfo.Expiration}");

                                // Redirect to returnUrl or role-based page
                                if (!string.IsNullOrEmpty(returnUrl) && IsValidReturnUrl(returnUrl))
                                {
                                    return Redirect(returnUrl);
                                }
                                
                                return RedirectToRoleBasedPage(
                                    userInfo.Role,
                                    $"Xin chào {userInfo.DisplayName}! Đăng nhập thành công."
                                );
                            }
                            else
                            {
                                Message =
                                    "Đăng nhập thất bại: Token không hợp lệ hoặc không chứa thông tin người dùng";
                                MessageType = "error";
                            }
                        }
                        else
                        {
                            Message = "Đăng nhập thất bại: Không nhận được token từ server";
                            MessageType = "error";
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Parse Error: {ex.Message}");
                        Console.WriteLine($"Raw response: {responseContent}");
                        Message = $"Lỗi parse JSON từ server";
                        MessageType = "error";
                    }
                }
                else
                {
                    Console.WriteLine($"Login failed - Status: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        Message = "Tên đăng nhập hoặc mật khẩu không chính xác";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Message = "Tài khoản không có quyền truy cập hoặc đã bị khóa";
                    }
                    else
                    {
                        Message =
                            $"Lỗi đăng nhập ({(int)response.StatusCode}): {response.ReasonPhrase}";
                        if (!string.IsNullOrWhiteSpace(responseContent))
                        {
                            Message += $". Chi tiết: {responseContent}";
                        }
                    }
                    MessageType = "error";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Message = $"Lỗi kết nối đến server: {ex.Message}";
                MessageType = "error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                Message = $"Lỗi không mong muốn: {ex.Message}";
                MessageType = "error";
            }

            return Page();
        }

        public IActionResult OnPostLogout()
        {
            try
            {
                // Clear the token cookie
                Response.Cookies.Delete("auth_token");
                
                // Clear all session data
                HttpContext.Session.Clear();

                Console.WriteLine("Auth token and session cleared - user logged out");

                return RedirectToPage(
                    "/Login",
                    new { message = "Đăng xuất thành công!", messageType = "success" }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
                return RedirectToPage(
                    "/Login",
                    new { message = "Đã đăng xuất", messageType = "info" }
                );
            }
        }

        // Decode JWT token to get user information
        public UserInfoDto? DecodeJwtToken(string? token = null)
        {
            try
            {
                token ??= HttpContext.Request.Cookies["auth_token"];
                if (string.IsNullOrEmpty(token))
                    return null;

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                    return null;

                var jsonToken = handler.ReadJwtToken(token);
                var userInfo = new UserInfoDto();

                foreach (var claim in jsonToken.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");

                    switch (claim.Type)
                    {
                        // ID
                        case "sub":
                        case "userid":
                        case "id":
                        case "nameid":
                        case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier":
                            if (int.TryParse(claim.Value, out var userId))
                                userInfo.UserId = userId;
                            break;

                        // Username
                        case "unique_name":
                        case "username":
                        case "name":
                        case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name":
                            userInfo.UserName = claim.Value;
                            break;

                        // Email
                        case "email":
                            userInfo.Email = claim.Value;
                            break;

                        // Role
                        case "role":
                        case "http://schemas.microsoft.com/ws/2008/06/identity/claims/role":
                            userInfo.Role = ValidateAndNormalizeRole(claim.Value);
                            break;

                        // FullName
                        case "fullname":
                        case "given_name":
                        case "family_name":
                            userInfo.FullName = claim.Value;
                            break;
                    }
                }

                userInfo.Expiration = jsonToken.ValidTo;

                if (string.IsNullOrEmpty(userInfo.UserName))
                {
                    Console.WriteLine("Token missing username claim");
                    return null;
                }

                userInfo.Role ??= "Guest";

                Console.WriteLine(
                    $"Decoded JWT - User: {userInfo.UserName}, Role: {userInfo.Role}, Expires: {userInfo.Expiration}"
                );

                return userInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding JWT token: {ex.Message}");
                return null;
            }
        }

        // Check if token is still valid (not expired)
        public bool IsTokenValid(string? token = null)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    token = HttpContext.Request.Cookies["auth_token"];
                }

                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                {
                    return false;
                }

                var jsonToken = handler.ReadJwtToken(token);

                // Check if token is expired
                var isExpired = jsonToken.ValidTo < DateTime.UtcNow;

                Console.WriteLine(
                    $"Token valid until: {jsonToken.ValidTo}, Current time: {DateTime.UtcNow}, Is expired: {isExpired}"
                );

                return !isExpired;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                return false;
            }
        }

        // Helper method to get current user info from JWT
        public UserInfoDto? GetCurrentUser()
        {
            var token = HttpContext.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token) || !IsTokenValid(token))
            {
                return null;
            }

            return DecodeJwtToken(token);
        }

        // Helper method to check if user is authenticated
        public bool IsAuthenticated()
        {
            var token = HttpContext.Request.Cookies["auth_token"];
            return !string.IsNullOrEmpty(token)
                && IsTokenValid(token)
                && DecodeJwtToken(token) != null;
        }

        // Helper method to check if user has specific role or higher
        public bool HasRoleOrHigher(string requiredRole)
        {
            var userInfo = GetCurrentUser();
            if (userInfo == null)
                return false;

            var roleHierarchy = new Dictionary<string, int>
            {
                { "Guest", 0 },
                { "Member", 1 },
                { "Staff", 2 },
                { "Consultant", 3 },
                { "Manager", 4 },
                { "Admin", 5 },
            };

            var userRoleLevel = roleHierarchy.GetValueOrDefault(userInfo.Role, 0);
            var requiredRoleLevel = roleHierarchy.GetValueOrDefault(requiredRole, 0);

            return userRoleLevel >= requiredRoleLevel;
        }

        // Get user's role
        public string GetUserRole()
        {
            var userInfo = GetCurrentUser();
            return userInfo?.Role ?? "Guest";
        }

        // Get user's display name
        public string GetDisplayName()
        {
            var userInfo = GetCurrentUser();
            return userInfo?.DisplayName ?? "Guest";
        }

        // Get auth token
        public string GetAuthToken()
        {
            return HttpContext.Request.Cookies["auth_token"] ?? "";
        }

        // Helper method to redirect based on user role
        private IActionResult RedirectToRoleBasedPage(string role, string? message = null)
        {
            var messageParams = new
            {
                message = message ?? "",
                messageType = string.IsNullOrEmpty(message) ? "" : "success",
            };

            var normalizedRole = ValidateAndNormalizeRole(role);

            return normalizedRole.ToLower() switch
            {
                "admin" => RedirectToPage("/AdminDashboard/AdminDashboard", messageParams),
                // Tất cả role quản lý đều về CourseDashboard
                "staff" => RedirectToPage("/StaffDashboard/StaffDashboard", messageParams),
                "manager" => RedirectToPage(messageParams),
                "consultant" => RedirectToPage("/CourseDashboard/CourseDashboard", messageParams),

                // Member về trang khóa học cá nhân
                "member" => RedirectToPage("/Courses/MyCourses", messageParams),

                // Guest về trang khóa học công khai
                "guest" or _ => RedirectToPage("/Courses", messageParams),
            };
        }

        // Validate and normalize role names - Guest is default
        private string ValidateAndNormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Guest"; // Default role

            var normalizedRole = role.Trim().ToLower();

            return normalizedRole switch
            {
                "admin" or "administrator" => "Admin",
                "manager" or "quản lý" => "Manager",
                "consultant" or "chuyên viên tư vấn" or "tư vấn viên" => "Consultant",
                "staff" or "nhân viên" => "Staff",
                "member" or "thành viên" => "Member",
                "guest" or "khách" or _ =>
                    "Guest" // Guest is default for unknown roles
                ,
            };
        }

        // Validate returnUrl to prevent open redirect attacks
        private bool IsValidReturnUrl(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                return false;

            // Only allow relative URLs that start with /
            if (!returnUrl.StartsWith("/"))
                return false;

            // Prevent double slashes which could be used for external redirects
            if (returnUrl.StartsWith("//"))
                return false;

            // Allow common pages
            var allowedPaths = new[] { 
                "/Profile", "/Courses", "/MyCourses", "/Dashboard", 
                "/AdminDashboard", "/StaffDashboard", "/CourseDashboard" 
            };

            return allowedPaths.Any(path => returnUrl.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        }
    }

    // DTOs
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }

    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
    }

    // DTO for user information from JWT
    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest"; // Default role
        public string? FullName { get; set; }
        public DateTime? Expiration { get; set; }

        // Computed property for display name
        public string DisplayName => FullName ?? UserName ?? "User";
    }
}
