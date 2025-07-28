using Services.Utilities;

namespace TestPasswordHash
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Password Hash Generator Test ===");
            Console.WriteLine();

            // Example 1: Hash a single password
            string adminPassword = "Admin123!";
            string adminHash = PasswordHashUtility.HashPassword(adminPassword);
            
            Console.WriteLine($"Password: {adminPassword}");
            Console.WriteLine($"Hash: {adminHash}");
            Console.WriteLine();

            // Example 2: Verify password
            bool isValid = PasswordHashUtility.VerifyPassword(adminHash, adminPassword);
            Console.WriteLine($"Password verification: {(isValid ? "✅ SUCCESS" : "❌ FAILED")}");
            Console.WriteLine();

            // Example 3: Generate SQL for default users
            Console.WriteLine("=== Generated SQL for Default Users ===");
            Console.WriteLine();
            string defaultUsersSql = PasswordHashUtility.GenerateDefaultUsersSql();
            Console.WriteLine(defaultUsersSql);

            // Example 4: Hash multiple passwords
            var passwords = new Dictionary<string, string>
            {
                ["admin"] = "Admin123!",
                ["test"] = "Test123!",
                ["demo"] = "Demo123!"
            };

            var hashes = PasswordHashUtility.HashMultiplePasswords(passwords);
            
            Console.WriteLine("=== Multiple Password Hashes ===");
            foreach (var kvp in hashes)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }
    }
} 