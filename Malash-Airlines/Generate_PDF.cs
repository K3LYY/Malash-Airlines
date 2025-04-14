using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;

namespace Malash_Airlines {
    public class PDFGenerationResult {
        public string TicketPath { get; set; }
        public string InvoicePath { get; set; }
    }

    public class PDFGenerationService {
        private readonly string _documentFolder;
        private readonly string _logoPath;

        public PDFGenerationService(string documentFolder = null, string logoPath = null) {
            _documentFolder = documentFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _logoPath = logoPath ?? Path.Combine(Directory.GetCurrentDirectory(), "logo.png");
        }

        public PDFGenerationResult GenerateDocuments(Reservation reservation, User user, Flight flight) {
            string ticketPath = Path.Combine(_documentFolder, $"FlightTicket_{user.Name}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            string invoicePath = Path.Combine(_documentFolder, $"Invoice_{user.Name}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            GenerateFlightTicket(reservation, user, flight, ticketPath);

            // Pobierz fakturę dla tej rezerwacji
            var invoices = Database.GetInvoices(reservation.ID);
            if (invoices.Count > 0) {
                GenerateInvoice(invoices[0], user, flight, reservation, invoicePath);
            } else {
                // Jeśli faktura nie istnieje, generuj domyślną
                GenerateDefaultInvoice(user, flight, reservation, invoicePath);
            }

            return new PDFGenerationResult {
                TicketPath = ticketPath,
                InvoicePath = invoicePath
            };
        }

        private void GenerateFlightTicket(Reservation reservation, User user, Flight flight, string outputPath) {
            Document.Create(container => {
                container.Page(page => {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(ComposeTicketHeader);

                    page.Content().PaddingVertical(10).Column(col => {
                        col.Item().LineHorizontal(2).LineColor("#003366");
                        col.Item().Text($"Pasażer: {user.Name}").FontSize(14).Bold();
                        col.Item().Text($"Numer lotu: {flight.ID}");
                        col.Item().Text($"Miejsce: {reservation.SeatNumber}");
                        col.Item().Text($"Data: {flight.Date:dd MMMM yyyy}");
                        col.Item().Text($"Czas wejścia na pokład: {flight.Time}");
                        col.Item().Text($"Wylot z: {flight.Departure}");
                        col.Item().Text($"Cel podróży: {flight.Destination}");
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Footer().AlignCenter().Text("Prosimy o przybycie do bramki 45 minut przed odlotem.").Italic();
                });
            })
            .GeneratePdf(outputPath);
        }

        private void GenerateInvoice(Invoice invoice, User user, Flight flight, Reservation reservation, string outputPath) {
            Document.Create(container => {
                container.Page(page => {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(ComposeInvoiceHeader);

                    page.Content().PaddingVertical(10).Column(col => {
                        col.Item().LineHorizontal(2).LineColor("#212529");

                        col.Item().Text($"Pasażer: {user.Name}").FontSize(14).Bold();
                        col.Item().Text($"E-mail: {user.Email}");
                        col.Item().Text($"Numer lotu: {flight.ID}");
                        col.Item().Text($"Miejsce: {reservation.SeatNumber}");
                        col.Item().Text($"Numer faktury: {invoice.InvoiceNumber}");
                        col.Item().Text($"Data wystawienia: {invoice.IssueDate:dd MMMM yyyy}");
                        col.Item().Text($"Termin płatności: {invoice.DueDate:dd MMMM yyyy}");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Invoice Table
                        col.Item().Table(table => {
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header => {
                                header.Cell().Element(CellStyle).Background("#212529").Text("Opis").FontColor(Colors.White);
                                header.Cell().Element(CellStyle).Background("#212529").AlignRight().Text("Kwota").FontColor(Colors.White);
                            });

                            // Cena podstawowa (80% całości)
                            decimal basePrice = Math.Round(invoice.Amount * 0.8m, 2);
                            // Podatki i opłaty (15%)
                            decimal taxes = Math.Round(invoice.Amount * 0.15m, 2);
                            // Opłata serwisowa (5%)
                            decimal serviceFee = Math.Round(invoice.Amount * 0.05m, 2);

                            table.Cell().Element(CellStyle).Text($"Bilet lotniczy ({flight.Departure} → {flight.Destination})");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{basePrice:C}");

                            table.Cell().Element(CellStyle).Text("Podatki i opłaty lotniskowe");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{taxes:C}");

                            table.Cell().Element(CellStyle).Text("Opłata serwisowa");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{serviceFee:C}");

                            table.Cell().Element(CellStyle).Background("#212529").Text("RAZEM").FontColor(Colors.White).Bold();
                            table.Cell().Element(CellStyle).Background("#212529").AlignRight().Text($"{invoice.Amount:C}").FontColor(Colors.White).Bold();
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        if (!string.IsNullOrEmpty(invoice.Notes)) {
                            col.Item().PaddingTop(10).Text("Uwagi:").Bold();
                            col.Item().Text(invoice.Notes);
                        }

                        col.Item().PaddingTop(10).Text($"Status płatności: {TranslateStatus(invoice.Status)}").Bold();

                        if (invoice.PaymentDate != default(DateTime)) {
                            col.Item().Text($"Data płatności: {invoice.PaymentDate:dd MMMM yyyy}");
                        }
                    });

                    page.Footer().AlignCenter().Text("Dziękujemy za korzystanie z usług Malash Airlines! Życzymy udanej podróży.").Italic();
                });
            })
            .GeneratePdf(outputPath);
        }

        private void GenerateDefaultInvoice(User user, Flight flight, Reservation reservation, string outputPath) {
            // Generowanie domyślnej faktury, gdy nie ma zapisanej w bazie
            decimal amount = flight.Price;

            // Zastosuj rabat dla klientów biznesowych
            if (user.CustomerType?.ToLower() == "business") {
                amount *= 0.8m; // 20% rabatu
            }

            Document.Create(container => {
                container.Page(page => {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(ComposeInvoiceHeader);

                    page.Content().PaddingVertical(10).Column(col => {
                        col.Item().LineHorizontal(2).LineColor("#212529");

                        col.Item().Text($"Pasażer: {user.Name}").FontSize(14).Bold();
                        col.Item().Text($"E-mail: {user.Email}");
                        col.Item().Text($"Numer lotu: {flight.ID}");
                        col.Item().Text($"Miejsce: {reservation.SeatNumber}");
                        col.Item().Text($"Numer faktury: INV-{DateTime.Now:yyyyMMddHHmmss}");
                        col.Item().Text($"Data wystawienia: {DateTime.Now:dd MMMM yyyy}");
                        col.Item().Text($"Termin płatności: {DateTime.Now.AddDays(7):dd MMMM yyyy}");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Invoice Table
                        col.Item().Table(table => {
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header => {
                                header.Cell().Element(CellStyle).Background("#212529").Text("Opis").FontColor(Colors.White);
                                header.Cell().Element(CellStyle).Background("#212529").AlignRight().Text("Kwota").FontColor(Colors.White);
                            });

                            // Cena podstawowa (80% całości)
                            decimal basePrice = Math.Round(amount * 0.8m, 2);
                            // Podatki i opłaty (15%)
                            decimal taxes = Math.Round(amount * 0.15m, 2);
                            // Opłata serwisowa (5%)
                            decimal serviceFee = Math.Round(amount * 0.05m, 2);

                            table.Cell().Element(CellStyle).Text($"Bilet lotniczy ({flight.Departure} → {flight.Destination})");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{basePrice:C}");

                            table.Cell().Element(CellStyle).Text("Podatki i opłaty lotniskowe");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{taxes:C}");

                            table.Cell().Element(CellStyle).Text("Opłata serwisowa");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{serviceFee:C}");

                            table.Cell().Element(CellStyle).Background("#212529").Text("RAZEM").FontColor(Colors.White).Bold();
                            table.Cell().Element(CellStyle).Background("#212529").AlignRight().Text($"{amount:C}").FontColor(Colors.White).Bold();
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingTop(10).Text($"Status płatności: Nieopłacona").Bold();
                    });

                    page.Footer().AlignCenter().Text("Dziękujemy za korzystanie z usług Malash Airlines! Życzymy udanej podróży.").Italic();
                });
            })
            .GeneratePdf(outputPath);
        }

        private void ComposeTicketHeader(IContainer container) {
            container.Row(row => {
                if (File.Exists(_logoPath)) {
                    row.RelativeItem(1).Image(_logoPath, ImageScaling.FitHeight);
                }
                row.RelativeItem(3).AlignCenter().Text("BILET LOTNICZY").FontSize(24).Bold().FontColor("#003366");
            });
        }

        private void ComposeInvoiceHeader(IContainer container) {
            container.Row(row => {
                if (File.Exists(_logoPath)) {
                    row.RelativeItem(1).Image(_logoPath, ImageScaling.FitHeight);
                }
                row.RelativeItem(3).AlignCenter().Text("FAKTURA").FontSize(26).Bold().FontColor("#212529");
            });
        }

        private static IContainer CellStyle(IContainer container) {
            return container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        }

        private string TranslateStatus(string status) {
            return status.ToLower() switch {
                "paid" => "Opłacona",
                "unpaid" => "Nieopłacona",
                "pending" => "Oczekująca",
                "cancelled" => "Anulowana",
                _ => status
            };
        }
    }
}
