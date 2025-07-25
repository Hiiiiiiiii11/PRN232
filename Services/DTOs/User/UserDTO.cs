using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.User
{
    public class UserDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; } // URL đến ảnh đại diện của người dùng
        public string? Status { get; set; } // Active, Inactive, Banned
        public DateTime CreatedAt { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
