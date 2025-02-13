using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace chatgptbot.Util
{
    public class PdfReaderHelper
    {
        public static string ExtractTextFromPdf(string filePath)
        {
            StringBuilder extractedText = new StringBuilder();

            using (var pdf = PdfDocument.Open(filePath))
            {
                foreach (var page in pdf.GetPages())
                {
                    extractedText.AppendLine(page.Text);
                }
            }

            return extractedText.ToString();
        }

    }
}
