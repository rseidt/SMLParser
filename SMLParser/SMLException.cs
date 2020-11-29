using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SMLParser
{
    public class SMLException : ApplicationException
    {
        public SMLException()
        {
        }

        public SMLException(string message) : base(message)
        {
        }

        public SMLException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SMLException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
