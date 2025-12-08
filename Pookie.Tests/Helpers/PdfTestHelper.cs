using System;
using System.IO;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AFUT.Tests.Helpers
{
    public static class PdfTestHelper
    {
        /// <summary>
        /// Finds the most recently downloaded PDF file in the specified directory.
        /// </summary>
        /// <param name="downloadDirectory">Directory to search for PDF files</param>
        /// <param name="timeoutSeconds">Maximum time to wait for a new PDF file</param>
        /// <param name="minFileSizeBytes">Minimum file size to consider valid (default 1KB)</param>
        /// <returns>FileInfo of the most recent PDF, or null if not found</returns>
        public static FileInfo? FindLatestPdf(string downloadDirectory, int timeoutSeconds = 30, int minFileSizeBytes = 1024)
        {
            var startTime = DateTime.Now;
            FileInfo? latestPdf = null;

            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                var dir = new DirectoryInfo(downloadDirectory);
                if (!dir.Exists)
                {
                    Thread.Sleep(500);
                    continue;
                }

                var pdfs = dir.GetFiles("*.pdf")
                    .Where(f => f.Length >= minFileSizeBytes && !f.Name.EndsWith(".crdownload") && !f.Name.EndsWith(".tmp"))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                if (pdfs.Any())
                {
                    latestPdf = pdfs.First();
                    // Wait a bit to ensure file is fully written
                    Thread.Sleep(1000);
                    
                    // Check if file is still being written
                    try
                    {
                        using var fs = File.Open(latestPdf.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
                        // If we can open exclusively, it's done
                        break;
                    }
                    catch (IOException)
                    {
                        // Still being written, wait more
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    Thread.Sleep(500);
                }
            }

            return latestPdf;
        }

        /// <summary>
        /// Extracts the heading text from the first page of a PDF (typically first 5-10 lines).
        /// </summary>
        /// <param name="pdfFilePath">Full path to the PDF file</param>
        /// <param name="maxLines">Maximum number of lines to extract from the top (default 10)</param>
        /// <returns>Heading text as a single string</returns>
        public static string ExtractPdfHeading(string pdfFilePath, int maxLines = 10)
        {
            if (!File.Exists(pdfFilePath))
            {
                throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
            }

            using var pdf = PdfDocument.Open(pdfFilePath);
            
            if (pdf.NumberOfPages == 0)
            {
                throw new InvalidOperationException("PDF has no pages");
            }

            var firstPage = pdf.GetPage(1);
            var words = firstPage.GetWords().OrderBy(w => w.BoundingBox.Top).ThenBy(w => w.BoundingBox.Left).ToList();

            if (!words.Any())
            {
                throw new InvalidOperationException("PDF first page has no text");
            }

            // Group words into lines based on vertical position (Y coordinate)
            var lines = new System.Collections.Generic.List<string>();
            var currentLine = new StringBuilder();
            double? currentY = null;
            const double lineThreshold = 5.0; // pixels tolerance for same line

            foreach (var word in words)
            {
                var wordY = word.BoundingBox.Top;

                if (currentY == null || Math.Abs(wordY - currentY.Value) <= lineThreshold)
                {
                    // Same line
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(" ");
                    }
                    currentLine.Append(word.Text);
                    currentY = wordY;
                }
                else
                {
                    // New line
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                    }
                    currentLine.Append(word.Text);
                    currentY = wordY;

                    if (lines.Count >= maxLines)
                    {
                        break;
                    }
                }
            }

            // Add the last line
            if (currentLine.Length > 0 && lines.Count < maxLines)
            {
                lines.Add(currentLine.ToString());
            }

            return string.Join("\n", lines.Take(maxLines));
        }

        /// <summary>
        /// Extracts all text from the PDF for full-text search assertions.
        /// </summary>
        /// <param name="pdfFilePath">Full path to the PDF file</param>
        /// <returns>Full PDF text content</returns>
        public static string ExtractFullPdfText(string pdfFilePath)
        {
            if (!File.Exists(pdfFilePath))
            {
                throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
            }

            using var pdf = PdfDocument.Open(pdfFilePath);
            var fullText = new StringBuilder();

            for (int i = 1; i <= pdf.NumberOfPages; i++)
            {
                var page = pdf.GetPage(i);
                fullText.AppendLine(page.Text);
            }

            return fullText.ToString();
        }

        /// <summary>
        /// Sanitizes a report name to match expected filename patterns.
        /// </summary>
        /// <param name="reportName">Original report name from UI</param>
        /// <returns>Sanitized name suitable for filename matching</returns>
        public static string SanitizeReportName(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName))
            {
                return string.Empty;
            }

            // Remove special characters typically stripped from filenames
            var sanitized = reportName
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_")
                .Replace("*", "_")
                .Replace("?", "_")
                .Replace("\"", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("|", "_")
                .Replace("'", "")
                .Replace(",", "")
                .Trim();

            return sanitized;
        }

        /// <summary>
        /// Cleans up old PDF files from the download directory (older than specified hours).
        /// </summary>
        /// <param name="downloadDirectory">Directory to clean</param>
        /// <param name="olderThanHours">Delete files older than this many hours (default 24)</param>
        public static void CleanupOldPdfs(string downloadDirectory, int olderThanHours = 24)
        {
            var dir = new DirectoryInfo(downloadDirectory);
            if (!dir.Exists)
            {
                return;
            }

            var cutoffTime = DateTime.Now.AddHours(-olderThanHours);
            var oldPdfs = dir.GetFiles("*.pdf")
                .Where(f => f.LastWriteTime < cutoffTime)
                .ToList();

            foreach (var pdf in oldPdfs)
            {
                try
                {
                    pdf.Delete();
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        }
    }
}

