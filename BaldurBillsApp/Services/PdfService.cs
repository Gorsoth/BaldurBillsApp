using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Drawing;
using System.IO;

namespace BaldurBillsApp.Services
{
    public class PdfService
    {
        public void AddRegistryNumberToPdf(string pdfPath,string pdfEditedPath, string registryNumber, string currencyRate)
        {
            PdfDocument pdf = new PdfDocument(new PdfReader(pdfPath), new PdfWriter(pdfEditedPath));
            Document document = new Document(pdf);

            Paragraph header = new Paragraph(registryNumber)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18);

            Paragraph foot = new Paragraph(currencyRate)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18);

                document.ShowTextAligned(header, 297.5f, 792 - 15, 1, TextAlignment.CENTER, VerticalAlignment.TOP, 0);
            document.ShowTextAligned(foot, 297.5f, 15, 1, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);


            document.Close();
        }

        public byte[] GeneratePdf(string textContent)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                document.Add(new Paragraph(textContent));

                document.Close();
                return ms.ToArray();
            }
        }
    }
}
