using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Malash_Airlines
{
    public class PDFGenerationService
    {
        // Configuration for document paths
        private readonly string _documentFolder;
        private readonly string _logoPath;

        public PDFGenerationService(string documentFolder = null, string logoPath = null)
        {
            _documentFolder = documentFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _logoPath = logoPath ?? Path.Combine(Directory.GetCurrentDirectory(), "logo.png");
        }

        public PDFGenerationResult GenerateDocuments(FlightTicketInfo ticketInfo)
        {
            string ticketPath = Path.Combine(_documentFolder, $"FlightTicket_{ticketInfo.PassengerName}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            string invoicePath = Path.Combine(_documentFolder, $"Invoice_{ticketInfo.PassengerName}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            GenerateFlightTicket(ticketInfo, ticketPath);
            GenerateInvoice(ticketInfo, invoicePath);

            return new PDFGenerationResult
            {
                TicketPath = ticketPath,
                InvoicePath = invoicePath
            };
        }

        private void GenerateFlightTicket(FlightTicketInfo ticketInfo, string outputPath)
        {
            Document doc = new Document(PageSize.A5.Rotate(), 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));
            doc.Open();

            // Fonts
            Font titleFont = FontFactory.GetFont("Arial", 24, Font.BOLD, new BaseColor(0, 51, 102)); // Dark Blue
            Font subtitleFont = FontFactory.GetFont("Arial", 14, Font.ITALIC, BaseColor.GRAY);
            Font labelFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.BLACK);
            Font valueFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.DARK_GRAY);

            // Header with logo
            AddHeaderWithLogo(doc, writer, "FLIGHT TICKET", titleFont);

            // Decorative line
            AddDecorativeLine(writer, new BaseColor(0, 51, 102), 2f, doc.Top - 60);

            // Ticket Info Box
            PdfPTable table = CreateTicketInfoTable(ticketInfo, labelFont, valueFont);
            doc.Add(table);

            // Footer
            AddFooterNote(doc, "Please arrive at the gate 45 minutes before departure.", subtitleFont);

            // Additional decorative line at bottom
            AddDecorativeLine(writer, BaseColor.LIGHT_GRAY, 1f, 40);

            doc.Close();
        }

        private void GenerateInvoice(FlightTicketInfo ticketInfo, string outputPath)
        {
            Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));
            doc.Open();

            // Fonts
            Font headerFont = FontFactory.GetFont("Arial", 26, Font.BOLD, new BaseColor(33, 37, 41)); // Dark Charcoal
            Font labelFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.BLACK);
            Font valueFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.DARK_GRAY);
            Font footerFont = FontFactory.GetFont("Arial", 10, Font.ITALIC, BaseColor.GRAY);

            // Header with logo
            AddHeaderWithLogo(doc, writer, "FLIGHT INVOICE", headerFont);

            // Decorative line under header
            AddDecorativeLine(writer, new BaseColor(33, 37, 41), 2f, doc.Top - 60);

            // Customer Details
            PdfPTable detailsTable = CreateCustomerDetailsTable(ticketInfo, labelFont, valueFont);
            doc.Add(detailsTable);

            // Invoice Table
            PdfPTable invoiceTable = CreateInvoiceTable(ticketInfo);
            doc.Add(invoiceTable);

            // Footer with decorative line
            AddDecorativeLine(writer, BaseColor.LIGHT_GRAY, 1f, 80);

            AddFooterNote(doc, "Thank you for flying with us! Safe travels.", footerFont);

            doc.Close();
        }

        private void AddHeaderWithLogo(Document doc, PdfWriter writer, string title, Font titleFont)
        {
            if (!File.Exists(_logoPath))
            {
                Paragraph header = new Paragraph(title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(header);
            }
            else
            {
                Image logo = Image.GetInstance(_logoPath);
                logo.ScaleToFit(50f, 50f);
                logo.Alignment = Element.ALIGN_LEFT;
                Paragraph header = new Paragraph(title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_MIDDLE });
                headerTable.AddCell(new PdfPCell(header) { Border = Rectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_MIDDLE });
                doc.Add(headerTable);
            }
        }

        private void AddDecorativeLine(PdfWriter writer, BaseColor color, float lineWidth, float yPosition)
        {
            PdfContentByte cb = writer.DirectContent;
            cb.SetColorStroke(color);
            cb.SetLineWidth(lineWidth);
            cb.MoveTo(30, yPosition);
            cb.LineTo(writer.PageSize.Width - 30, yPosition);
            cb.Stroke();
        }

        private void AddFooterNote(Document doc, string note, Font font)
        {
            Paragraph footer = new Paragraph(note, font)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 20f
            };
            doc.Add(footer);
        }

        private PdfPTable CreateTicketInfoTable(FlightTicketInfo ticketInfo, Font labelFont, Font valueFont)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Padding = 10;
            table.DefaultCell.BorderColor = new BaseColor(200, 200, 200);
            table.SpacingBefore = 20f;

            AddInfoRow(table, "Passenger:", ticketInfo.PassengerName, labelFont, valueFont);
            AddInfoRow(table, "Flight No:", ticketInfo.FlightNumber, labelFont, valueFont);
            AddInfoRow(table, "Seat No:", ticketInfo.SeatNumber, labelFont, valueFont);
            AddInfoRow(table, "Date:", DateTime.Now.ToString("dd MMMM yyyy"), labelFont, valueFont);
            AddInfoRow(table, "Boarding Time:", "10:00 AM", labelFont, valueFont);
            AddInfoRow(table, "Destination:", "New York (JFK)", labelFont, valueFont);

            return table;
        }

        private PdfPTable CreateCustomerDetailsTable(FlightTicketInfo ticketInfo, Font labelFont, Font valueFont)
        {
            PdfPTable detailsTable = new PdfPTable(2);
            detailsTable.WidthPercentage = 100;
            detailsTable.SpacingBefore = 20f;
            detailsTable.DefaultCell.Border = Rectangle.NO_BORDER;

            AddInfoRow(detailsTable, "Passenger:", ticketInfo.PassengerName, labelFont, valueFont);
            AddInfoRow(detailsTable, "Flight Number:", ticketInfo.FlightNumber, labelFont, valueFont);
            AddInfoRow(detailsTable, "Seat Number:", ticketInfo.SeatNumber, labelFont, valueFont);
            AddInfoRow(detailsTable, "Invoice Number:", $"INV-{DateTime.Now:yyyyMMddHHmmss}", labelFont, valueFont);
            AddInfoRow(detailsTable, "Date:", DateTime.Now.ToString("dd MMMM yyyy"), labelFont, valueFont);

            return detailsTable;
        }

        private PdfPTable CreateInvoiceTable(FlightTicketInfo ticketInfo)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SpacingBefore = 25f;
            table.SpacingAfter = 15f;
            float[] widths = { 70f, 30f };
            table.SetWidths(widths);

            BaseColor headerColor = new BaseColor(33, 37, 41);
            AddInvoiceHeader(table, "Description", headerColor);
            AddInvoiceHeader(table, "Amount", headerColor);

            AddInvoiceRow(table, "Flight Ticket", "$199.99");
            AddInvoiceRow(table, "Airport Taxes & Fees", "$50.00");
            AddInvoiceRow(table, "Service Fee", "$10.00");

            PdfPCell totalLabel = CreateTotalCell("TOTAL", true, headerColor);
            PdfPCell totalAmount = CreateTotalCell("$259.99", false, headerColor);

            table.AddCell(totalLabel);
            table.AddCell(totalAmount);

            return table;
        }

        private void AddInfoRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            table.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Padding = 5, Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Padding = 5, Border = Rectangle.NO_BORDER });
        }

        private void AddInvoiceHeader(PdfPTable table, string text, BaseColor bgColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
            {
                BackgroundColor = bgColor,
                Padding = 10,
                BorderColor = BaseColor.LIGHT_GRAY
            };
            table.AddCell(cell);
        }

        private void AddInvoiceRow(PdfPTable table, string description, string amount)
        {
            table.AddCell(new PdfPCell(new Phrase(description, new Font(Font.FontFamily.HELVETICA, 12))) { Padding = 8, BorderColor = BaseColor.LIGHT_GRAY });
            table.AddCell(new PdfPCell(new Phrase(amount, new Font(Font.FontFamily.HELVETICA, 12))) { Padding = 8, HorizontalAlignment = Element.ALIGN_RIGHT, BorderColor = BaseColor.LIGHT_GRAY });
        }

        private PdfPCell CreateTotalCell(string text, bool isLabel, BaseColor bgColor)
        {
            return new PdfPCell(new Phrase(text, new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
            {
                BackgroundColor = bgColor,
                Padding = 10,
                HorizontalAlignment = isLabel ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT,
                BorderColor = BaseColor.LIGHT_GRAY
            };
        }
    }

    // Data transfer object for flight ticket information
    public class FlightTicketInfo
    {
        public string PassengerName { get; set; }
        public string FlightNumber { get; set; }
        public string SeatNumber { get; set; }
    }

    // Result of PDF generation
    public class PDFGenerationResult
    {
        public string TicketPath { get; set; }
        public string InvoicePath { get; set; }
    }
}