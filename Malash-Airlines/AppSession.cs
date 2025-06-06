﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Malash_Airlines
{
    //klasa do przechowania informacji o aktualnej sesji użytkownika
    internal class AppSession {
        public static User CurrentUser { get; set; }
        public static bool isLoggedIn = false;

        public static bool checkSession() {
            return AppSession.isLoggedIn && CurrentUser != null;
        }

        
        public static string eMail {
            get { return CurrentUser?.Email; }
            set {
                if (CurrentUser != null)
                    CurrentUser.Email = value;
            }
        }

        public static string userRole {
            get { return CurrentUser?.Role; }
            set {
                if (CurrentUser != null)
                    CurrentUser.Role = value;
            }
        }
    }
}
