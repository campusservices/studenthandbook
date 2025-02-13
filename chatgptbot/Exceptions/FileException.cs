using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Exceptions
{
    public class FileException : Exception
    {
        public FileException()
        {
        }

        public FileException(string message) : base(message)
        {
        }

        public FileException(String msg, Exception inner) : base(msg, inner)
        {

        }
    }
}
