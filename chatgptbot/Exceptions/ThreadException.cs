using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Exceptions
{
    public class ThreadException : Exception
    {
        public ThreadException()
        {
        }

        public ThreadException(string message) : base(message)
        {
        }

        public ThreadException(String msg, Exception inner) : base(msg, inner)
        {

        }
    }
}
