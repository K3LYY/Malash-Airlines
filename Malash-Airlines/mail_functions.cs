using System;
using System.Net;
using System.Net.Mail;
using System.IO;
using DotNetEnv;
using System.Windows;
using iText.Layout.Properties;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media.Media3D;

namespace Malash_Airlines {
    internal class mail_functions {
        static mail_functions() {
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env");
            Env.Load(envPath);
        }

        private static string GenerateOneTimePassword() {
            Random random = new Random();
            string oneTimePassword = "";
            for (int i = 0; i < 6; i++) {
                oneTimePassword += random.Next(0, 9);
            }
            return lastCode = oneTimePassword;
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
            try {
                string emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                string fromEmail = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com") {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true,
                };

                MailMessage mailMessage = new MailMessage {
                    From = new MailAddress(fromEmail, "Malash airlines"),
                    Subject = "Jednorazowy kod",
                    Body = GetMailReadyPassword(oneTimePassword),
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email.Replace(" ", "").Replace("\n", ""));

                smtpClient.Send(mailMessage);
                Console.WriteLine("Email został wysłany pomyślnie!");
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas wysyłania email: {ex.Message}");
            }
            return oneTimePassword;
        }

        public static void SendBookingConfirmation(string email, string pdfFilePath) {
            try {
                string emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                string fromEmail = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com") {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true,
                };

                string htmlBody = """
        <!DOCTYPE html>
        <html lang="pl">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Potwierdzenie Rezerwacji</title>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; background-color: #f5f5f5; padding: 20px; }
                .container { background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
                h1 { color: #4CAF50; }
                p { font-size: 16px; color: #333; }
                .footer { margin-top: 20px; font-size: 12px; color: #777; }
            </style>
        </head>
        <body>
            <div class="container">
                <h1>Potwierdzenie Rezerwacji</h1>
                <p>Witaj,</p>
                <p>W załączniku znajduje się potwierdzenie Twojej rezerwacji. Dziękujemy za wybranie Malash Airlines!</p>
                <p>Życzymy przyjemnej podróży.</p>
                <div class="footer">&copy; 2025 Malash Airlines. Wszelkie prawa zastrzeżone.</div>
            </div>
        </body>
        </html>
        """;

                MailMessage mailMessage = new MailMessage {
                    From = new MailAddress(fromEmail, "Malash Airlines"),
                    Subject = "Potwierdzenie Rezerwacji",
                    Body = htmlBody,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                if (File.Exists(pdfFilePath)) {
                    Attachment attachment = new Attachment(pdfFilePath);
                    mailMessage.Attachments.Add(attachment);
                } else {
                    Console.WriteLine("Plik PDF nie istnieje.");
                    return;
                }

                smtpClient.Send(mailMessage);
                Console.WriteLine("Potwierdzenie rezerwacji zostało wysłane pomyślnie!");
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas wysyłania potwierdzenia rezerwacji: {ex.Message}");
            }
        }

        public static void SendReservationDocuments(string email, Reservation reservation, User user, Flight flight) {
            try {
                string emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                string fromEmail = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");

                PDFGenerationService pdfService = new PDFGenerationService();
                var pdfResults = pdfService.GenerateDocuments(reservation, user, flight);

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com") {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true,
                };

                string style = """
        <style>
            body { font-family: Arial, sans-serif; line-height: 1.6; background-color: #f5f5f5; padding: 20px; }
            .container { background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
            h1 { color: #212529; }
            h2 { color: #4682B4; }
            p { font-size: 16px; color: #333; }
            .footer { margin-top: 20px; font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 10px; }
            .flight-details { background-color: #f8f8f8; padding: 15px; border-radius: 5px; margin: 15px 0; }
            .amount { font-size: 24px; font-weight: bold; color: #212529; }
            .status { font-weight: bold; }
        </style>
        """;

                string htmlBody = $"""
        <!DOCTYPE html>
        <html lang="pl">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Twoja rezerwacja w Malash Airlines</title>
            {style}
        </head>
        <body>
            <div class="container">
                <h1>Twoja rezerwacja w Malash Airlines</h1>
                <p>Witaj {user.Name},</p>
                <p>Potwierdzamy utworzenie rezerwacji na lot z {flight.Departure} do {flight.Destination}.</p>
                
                <div class="flight-details">
                    <h2>Szczegóły lotu:</h2>
                    <p><strong>Numer lotu:</strong> {flight.ID}</p>
                    <p><strong>Data:</strong> {flight.Date:dd MMMM yyyy}</p>
                    <p><strong>Czas wejścia na pokład:</strong> {flight.Time}</p>
                    <p><strong>Miejsce:</strong> {reservation.SeatNumber}</p>
                    <p><strong>Status:</strong> {reservation.Status}</p>
                </div>
                
                <p>W załącznikach znajdziesz bilet oraz fakturę za rezerwację.</p>
                <p>Prosimy o przybycie na lotnisko co najmniej 2 godziny przed wylotem.</p>
                
                <div class="footer">
                    <p>Ta wiadomość została wygenerowana automatycznie. Prosimy na nią nie odpowiadać.</p>
                    <p>&copy; 2025 Malash Airlines. Wszelkie prawa zastrzeżone.</p>
                </div>
            </div>
        </body>
        </html>
        """;

                MailMessage mailMessage = new MailMessage {
                    From = new MailAddress(fromEmail, "Malash Airlines"),
                    Subject = $"Potwierdzenie rezerwacji lotu {flight.ID}: {flight.Departure} → {flight.Destination}",
                    Body = htmlBody,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                if (File.Exists(pdfResults.TicketPath)) {
                    mailMessage.Attachments.Add(new Attachment(pdfResults.TicketPath));
                }

                if (File.Exists(pdfResults.InvoicePath)) {
                    mailMessage.Attachments.Add(new Attachment(pdfResults.InvoicePath));
                }

                smtpClient.Send(mailMessage);
                Console.WriteLine("Dokumenty rezerwacji zostały wysłane pomyślnie!");

                try {
                    if (File.Exists(pdfResults.TicketPath)) File.Delete(pdfResults.TicketPath);
                    if (File.Exists(pdfResults.InvoicePath)) File.Delete(pdfResults.InvoicePath);
                } catch (Exception ex) {
                    Console.WriteLine($"Błąd podczas usuwania plików tymczasowych: {ex.Message}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas wysyłania dokumentów rezerwacji: {ex.Message}");
            }
        }

        public static void SendInvoice(string email, Invoice invoice, Reservation reservation, User user, Flight flight) {
            try {
                string emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                string fromEmail = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");

                PDFGenerationService pdfService = new PDFGenerationService();
                var pdfResults = pdfService.GenerateDocuments(reservation, user, flight);

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com") {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true,
                };

                string statusText = invoice.Status.ToLower() switch {
                    "paid" => "Opłacona",
                    "unpaid" => "Nieopłacona - wymaga płatności",
                    "pending" => "Oczekująca na płatność",
                    "cancelled" => "Anulowana",
                    _ => invoice.Status
                };

                string style = """
        <style>
            body { font-family: Arial, sans-serif; line-height: 1.6; background-color: #f5f5f5; padding: 20px; }
            .container { background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
            h1 { color: #212529; }
            h2 { color: #4682B4; }
            p { font-size: 16px; color: #333; }
            .footer { margin-top: 20px; font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 10px; }
            .invoice-details { background-color: #f8f8f8; padding: 15px; border-radius: 5px; margin: 15px 0; }
            .amount { font-size: 24px; font-weight: bold; color: #212529; }
            .status { font-weight: bold; }
        </style>
        """;

                string htmlBody = $"""
        <!DOCTYPE html>
        <html lang="pl">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Faktura {invoice.InvoiceNumber}</title>
            {style}
        </head>
        <body>
            <div class="container">
                <h1>Faktura - Malash Airlines</h1>
                <p>Witaj {user.Name},</p>
                <p>W załączniku znajdziesz fakturę za rezerwację lotu z {flight.Departure} do {flight.Destination}.</p>
                
                <div class="invoice-details">
                    <h2>Szczegóły faktury:</h2>
                    <p><strong>Numer faktury:</strong> {invoice.InvoiceNumber}</p>
                    <p><strong>Data wystawienia:</strong> {invoice.IssueDate:dd MMMM yyyy}</p>
                    <p><strong>Termin płatności:</strong> {invoice.DueDate:dd MMMM yyyy}</p>
                    <p class="amount">Kwota: {invoice.Amount:C}</p>
                    <p class="status">Status: {statusText}</p>
                </div>
                
                <p>Dziękujemy za wybranie Malash Airlines.</p>
                
                <div class="footer">
                    <p>Ta wiadomość została wygenerowana automatycznie. Prosimy na nią nie odpowiadać.</p>
                    <p>&copy; 2025 Malash Airlines. Wszelkie prawa zastrzeżone.</p>
                </div>
            </div>
        </body>
        </html>
        """;

                MailMessage mailMessage = new MailMessage {
                    From = new MailAddress(fromEmail, "Malash Airlines"),
                    Subject = $"Faktura {invoice.InvoiceNumber} - Malash Airlines",
                    Body = htmlBody,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                if (File.Exists(pdfResults.InvoicePath)) {
                    mailMessage.Attachments.Add(new Attachment(pdfResults.InvoicePath));
                }

                smtpClient.Send(mailMessage);
                Console.WriteLine("Faktura została wysłana pomyślnie!");

                try {
                    if (File.Exists(pdfResults.TicketPath)) File.Delete(pdfResults.TicketPath);
                    if (File.Exists(pdfResults.InvoicePath)) File.Delete(pdfResults.InvoicePath);
                } catch (Exception ex) {
                    Console.WriteLine($"Błąd podczas usuwania plików tymczasowych: {ex.Message}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas wysyłania faktury: {ex.Message}");
            }
        }
    }
}
