using chatgptbot.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Services.Interface
{
    public interface IDocumentService
    {
        public Task<String> ComposeDocument(string text);
        public void RemoveDirectory();
    }
}
