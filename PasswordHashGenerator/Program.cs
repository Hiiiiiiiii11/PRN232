using Microsoft.AspNetCore.Identity;
using BussinessObjects;

namespace PasswordHashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Drug Use Prevention - Password Hash Generator ===");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter password to hash (or 'exit' to quit): ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Please enter a password.");
                    continue;
                }

                if (input.ToLower() == "exit")
                {
                    break;
                }

                // Generate hash using the same method as AuthService
                var passwordHasher = new PasswordHasher<User>();
                string hashedPassword = passwordHasher.HashPassword(null, input);

                Console.WriteLine();
                Console.WriteLine("Generated Hash:");
                Console.WriteLine(hashedPassword);
                Console.WriteLine();
                Console.WriteLine("SQL Insert Statement:");
                Console.WriteLine($"-- Update admin password");
                Console.WriteLine($"UPDATE Users SET PasswordHash = '{hashedPassword}' WHERE Username = 'admin';");
                Console.WriteLine();
                Console.WriteLine("Or for new user insert:");
                Console.WriteLine($"INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status, IsEmailVerified) VALUES");
                Console.WriteLine($"('System Administrator', 'admin', 'admin@drugprevention.com', '{hashedPassword}', 'Admin', 'Active', 1);");
                Console.WriteLine();
                Console.WriteLine("----------------------------------------");
                Console.WriteLine();

                // Test verification
                Console.Write("Test verification? Enter the original password again to verify (or press Enter to skip): ");
                string? testPassword = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(testPassword))
                {
                    var verificationResult = passwordHasher.VerifyHashedPassword(null, hashedPassword, testPassword);
                    
                    if (verificationResult == PasswordVerificationResult.Success)
                    {
                        Console.WriteLine("✅ Password verification: SUCCESS");
                    }
                    else
                    {
                        Console.WriteLine("❌ Password verification: FAILED");
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Goodbye!");
        }
    }
} 