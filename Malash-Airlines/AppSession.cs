using System;

namespace Malash_Airlines
{
    internal class AppSession
    {
        // Basic user information
        public static int UserId { get; set; }
        public static string Email { get; set; }
        public static string FullName { get; set; }

        // Authentication state
        public static bool IsLoggedIn { get; set; } = false;

        // Current reservation information
        public static int CurrentFlightId { get; set; }
        public static string CurrentSeatNumber { get; set; }

        // Session validation
        public static bool CheckSession()
        {
            return IsLoggedIn;
        }

        // Reset session data when logging out
        public static void ClearSession()
        {
            UserId = 0;
            Email = string.Empty;
            FullName = string.Empty;
            IsLoggedIn = false;
            CurrentFlightId = 0;
            CurrentSeatNumber = string.Empty;
        }
    }
}