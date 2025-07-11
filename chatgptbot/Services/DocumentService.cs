using chatgptbot.dto;
using chatgptbot.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Services
{
    public class DocumentService : IDocumentService
    {
        private string tempDir { get; set; }
        public async Task<String> ComposeDocument(string text)
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            tempDir = Path.Combine(appDir, "tempfiles\\"+Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);  // Ensure directory is created

            var filePath = Path.Combine(tempDir, "tempfile.pdf");

            try
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(20));

                        page.Content()
                            .Column(col =>
                            {
                                col.Item().Text(text);
                                col.Item().Image(Placeholders.Image(200, 100));
                            });
                    });
                })
                .GeneratePdf(filePath);
            } catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return await Task.FromResult(filePath);
        }

        public void RemoveDirectory()
        {
            Directory.Delete(tempDir, true);
        }
    }
}
