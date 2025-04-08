using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Malash_Airlines
{
    internal class AppSession
    {
        public static string eMail { get; set; }
        public static bool isLoggedIn = false;

        public static bool checkSession()
        {
            if (AppSession.isLoggedIn)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
