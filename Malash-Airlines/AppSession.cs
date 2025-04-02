using System;

namespace Malash_Airlines
{
    public static class AppSession
    {
        public static int UserId { get; set; }
        public static string Email { get; set; }
        public static string Role { get; private set; }
        public static bool IsLoggedIn { get; set; }
        public static string Name { get; private set; }

        public static void Login(int userId, string email, string role, string name)
        {
            UserId = userId;
            Email = email;
            Role = role;
            Name = name;
            IsLoggedIn = true;
        }

        public static void Logout()
        {
            UserId = 0;
            Email = string.Empty;
            Role = string.Empty;
            Name = string.Empty;
            IsLoggedIn = false;
        }

        public static bool CheckSession()
        {
            return IsLoggedIn;
        }

        public static bool IsAdmin()
        {
            return IsLoggedIn && Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsWorker()
        {
            return IsLoggedIn && Role.Equals("worker", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCustomer()
        {
            return IsLoggedIn && Role.Equals("customer", StringComparison.OrdinalIgnoreCase);
        }
    }
}