using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Exceptions
{
    public class SaveException : Exception
    {
        public SaveException()
        {
        }

        public SaveException(string message) : base(message)
        {
        }

        public SaveException(String msg, Exception inner) : base(msg, inner)
        {

        }
    }
}
