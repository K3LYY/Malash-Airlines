using System;
using System.Net;
using System.Net.Mail;
using DotNetEnv;

namespace Malash_Airlines {
    internal class mail_functions {
        static mail_functions() {
            // Load environment variables from .env file
            Env.Load();
        }

        private static string GenerateOneTimePassword() {
            Random random = new Random();
            string oneTimePassword = "";
            for (int i = 0; i < 6; i++) {
                oneTimePassword += random.Next(0, 9);
            }
            return oneTimePassword;
        }

        private static string lastCode = "";

        private static string styleMail = """
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f5f5f5;
        }

        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
        }

        .header {
            text-align: center;
            padding-bottom: 20px;
            border-bottom: 1px solid #eeeeee;
        }

        .logo {
            max-height: 100px;
            margin-bottom: 20px;
        }

        .content {
            padding: 30px 0;
        }

        .code-container {
            text-align: center;
            margin: 30px 0;
            padding: 15px;
            background-color: #f8f8f8;
            border-radius: 5px;
        }

        .code {
            font-size: 32px;
            font-weight: bold;
            letter-spacing: 5px;
            color: #2c3e50;
        }

        .footer {
            font-size: 12px;
            text-align: center;
            color: #999999;
            padding-top: 20px;
            border-top: 1px solid #eeeeee;
        }

        .button {
            display: inline-block;
            padding: 12px 24px;
            background-color: #4285f4;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
            margin-top: 15px;
        }

        .info-text {
            font-size: 14px;
            color: #666666;
            margin-top: 25px;
        }
        """;

        private static string GetMailReadyPassword(string oneTimeCode) {
            string mail = """
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Jednorazowy kod dostępu</title>
                <style>
                    {{STYLE}}
                </style>
            </head>
            <body>
                <div class="email-container">
                    <div class="header">
                        <img src="https://files.elory.me/api/public/dl/Xij8m_3q/Prywatne/Prywatne/Szkola/logo.png" alt="Logo Firmy" class="logo">
                        <h1>Twój jednorazowy kod dostępu</h1>
                    </div>

                    <div class="content">
                        <p>Witaj,</p>
                        <p>Poniżej znajduje się Twój jednorazowy kod dostępu do konta.</p>

                        <div class="code-container">
                            <div class="code">{{CODE}}</div>
                        </div>

                        <p>Wpisz powyższy kod w oknie weryfikacji, aby kontynuować logowanie.</p>

                    </div>

                    <div class="footer">
                        <p>Ta wiadomość została wygenerowana automatycznie. Prosimy na nią nie odpowiadać.</p>
                        <p>&copy; 2025 Malash Airlines. Wszelkie prawa zastrzeżone.</p>
                        <p><a href="#">Polityka prywatności</a> | <a href="#">Warunki korzystania</a></p>
                    </div>
                </div>
            </body>
            </html>
            """;

            return mail.Replace("{{CODE}}", oneTimeCode).Replace("{{STYLE}}", styleMail);
        }

        public static string SendOneTimePassword(string email) {
            string oneTimePassword = GenerateOneTimePassword();
            lastCode = oneTimePassword;
            try {
                // Odczytanie hasła z pliku .env
                string emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                string fromEmail = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");

                // Konfiguracja klienta SMTP dla Google
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com") {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true,
                };

                // Utworzenie wiadomości
                MailMessage mailMessage = new MailMessage {
                    From = new MailAddress(fromEmail, "Malash airlines"),
                    Subject = "Jednorazowy kod",
                    Body = GetMailReadyPassword(oneTimePassword),
                    IsBodyHtml = true, // Ustawienie IsBodyHtml na true
                };

                // Dodanie odbiorcy
                mailMessage.To.Add(email.Replace(" ", "").Replace("\n", ""));

                // Wysłanie wiadomości
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email został wysłany pomyślnie!");
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas wysyłania email: {ex.Message}");
            }
            return oneTimePassword;
        }
    }
}
