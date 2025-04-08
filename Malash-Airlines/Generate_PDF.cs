using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;

namespace Malash_Airlines
{
    public class FlightTicketInfo
    {
        public string PassengerName { get; set; }
        public string FlightNumber { get; set; }
        public string SeatNumber { get; set; }
    }

    public class PDFGenerationResult
    {
        public string TicketPath { get; set; }
        public string InvoicePath { get; set; }
    }

    public class PDFGenerationService
    {

        private readonly string _documentFolder;
        private readonly string _logoPath;

        public PDFGenerationService(string documentFolder = null, string logoPath = null)
        {
            _documentFolder = documentFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _logoPath = logoPath ?? Path.Combine(Directory.GetCurrentDirectory(), "logo.png");
        }

        public PDFGenerationResult GenerateDocuments(Reservation reservation, User user, Flight flight) {
            string ticketPath = Path.Combine(_documentFolder, $"FlightTicket_{user.Name}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            string invoicePath = Path.Combine(_documentFolder, $"Invoice_{user.Name}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            // Konwersja danych dla zachowania kompatybilności z metodą GenerateInvoice
            var ticketInfo = new FlightTicketInfo {
                PassengerName = user.Name,
                FlightNumber = flight.ID.ToString(),
                SeatNumber = reservation.SeatNumber
            };

            GenerateFlightTicket(reservation, user, flight, ticketPath);
            GenerateInvoice(ticketInfo, invoicePath);

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
                        col.Item().Text($"Passenger: {user.Name}").FontSize(14).Bold();
                        col.Item().Text($"Flight Number: {flight.ID}");
                        col.Item().Text($"Seat Number: {reservation.SeatNumber}");
                        col.Item().Text($"Date: {flight.Date:dd MMMM yyyy}");
                        col.Item().Text($"Boarding Time: {flight.Time}");
                        col.Item().Text($"Destination: {flight.Destination}");
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Footer().AlignCenter().Text("Please arrive at the gate 45 minutes before departure.").Italic();
                });
            })
            .GeneratePdf(outputPath);
        }

        private void GenerateInvoice(FlightTicketInfo ticketInfo, string outputPath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(ComposeInvoiceHeader);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().LineHorizontal(2).LineColor("#212529");

                        col.Item().Text($"Passenger: {ticketInfo.PassengerName}").FontSize(14).Bold();
                        col.Item().Text($"Flight Number: {ticketInfo.FlightNumber}");
                        col.Item().Text($"Seat Number: {ticketInfo.SeatNumber}");
                        col.Item().Text($"Invoice Number: INV-{DateTime.Now:yyyyMMddHHmmss}");
                        col.Item().Text($"Date: {DateTime.Now:dd MMMM yyyy}");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Invoice Table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Background("#212529").Text("Description").FontColor(Colors.White);
                                header.Cell().Element(CellStyle).Background("#212529").AlignRight().Text("Amount").FontColor(Colors.White);
                            });

                            table.Cell().Element(CellStyle).Text("Flight Ticket");
                            table.Cell().Element(CellStyle).AlignRight().Text("$199.99");

                            table.Cell().Element(CellStyle).Text("Airport Taxes & Fees");
                            table.Cell().Element(CellStyle).AlignRight().Text("$50.00");

                            table.Cell().Element(CellStyle).Text("Service Fee");
                            table.Cell().Element(CellStyle).AlignRight().Text("$10.00");

                            table.Cell().Element(CellStyle).Background("#212529").Text("TOTAL").FontColor(Colors.White).Bold();
                            table.Cell().Element(CellStyle).Background("#212529").AlignRight().Text("$259.99").FontColor(Colors.White).Bold();
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Footer().AlignCenter().Text("Thank you for flying with us! Safe travels.").Italic();
                });
            })
            .GeneratePdf(outputPath);
        }

        private void ComposeTicketHeader(IContainer container)
        {
            container.Row(row =>
            {
                if (File.Exists(_logoPath))
                {
                    row.RelativeItem(1).Image(_logoPath, ImageScaling.FitHeight);
                }
                row.RelativeItem(3).AlignCenter().Text("FLIGHT TICKET").FontSize(24).Bold().FontColor("#003366");
            });
        }

        private void ComposeInvoiceHeader(IContainer container)
        {
            container.Row(row =>
            {
                if (File.Exists(_logoPath))
                {
                    row.RelativeItem(1).Image(_logoPath, ImageScaling.FitHeight);
                }
                row.RelativeItem(3).AlignCenter().Text("FLIGHT INVOICE").FontSize(26).Bold().FontColor("#212529");
            });
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        }
    }
}
