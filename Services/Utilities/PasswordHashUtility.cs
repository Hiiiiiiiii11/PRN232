using Microsoft.AspNetCore.Identity;
using BussinessObjects;

namespace Services.Utilities
{
    /// <summary>
    /// Utility class for generating and verifying password hashes
    /// Uses the same method as AuthService to ensure consistency
    /// </summary>
    public static class PasswordHashUtility
    {
        private static readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        /// <summary>
        /// Generate a password hash using ASP.NET Core Identity PasswordHasher
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password string</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return _passwordHasher.HashPassword(null, password);
        }

        /// <summary>
        /// Verify a password against its hash
        /// </summary>
        /// <param name="hashedPassword">The hashed password from database</param>
        /// <param name="providedPassword">The plain text password to verify</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
                return false;

            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

        /// <summary>
        /// Generate multiple password hashes for testing
        /// </summary>
        /// <param name="passwords">Dictionary of label -> password pairs</param>
        /// <returns>Dictionary of label -> hash pairs</returns>
        public static Dictionary<string, string> HashMultiplePasswords(Dictionary<string, string> passwords)
        {
            var result = new Dictionary<string, string>();
            
            foreach (var kvp in passwords)
            {
                result[kvp.Key] = HashPassword(kvp.Value);
            }
            
            return result;
        }

        /// <summary>
        /// Generate SQL statements for common users
        /// </summary>
        /// <returns>SQL insert statements with hashed passwords</returns>
        public static string GenerateDefaultUsersSql()
        {
            var defaultUsers = new Dictionary<string, (string fullName, string username, string email, string password, string role)>
            {
                ["admin"] = ("System Administrator", "admin", "admin@drugprevention.com", "Admin123!", "Admin"),
                ["manager"] = ("Manager User", "manager", "manager@drugprevention.com", "Manager123!", "Manager"),
                ["staff"] = ("Staff User", "staff", "staff@drugprevention.com", "Staff123!", "Staff"),
                ["consultant"] = ("Dr. John Consultant", "consultant", "consultant@drugprevention.com", "Consultant123!", "Consultant")
            };

            var sqlStatements = new List<string>();
            sqlStatements.Add("-- Default users with hashed passwords");
            sqlStatements.Add("-- Generated using PasswordHashUtility");
            sqlStatements.Add("");

            foreach (var user in defaultUsers)
            {
                var hashedPassword = HashPassword(user.Value.password);
                sqlStatements.Add($"-- {user.Value.fullName} (Password: {user.Value.password})");
                sqlStatements.Add($"INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status, IsEmailVerified) VALUES");
                sqlStatements.Add($"('{user.Value.fullName}', '{user.Value.username}', '{user.Value.email}', '{hashedPassword}', '{user.Value.role}', 'Active', 1);");
                sqlStatements.Add("");
            }

            return string.Join(Environment.NewLine, sqlStatements);
        }
    }
} 