using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.dto
{
    public class FileDto
    {
        public string threadId { get; set; }
        public IFormFile pdfFile { get; set; }
    }
}
