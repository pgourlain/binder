using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace GeniusBinding.Core
{
    [Serializable]
    public class CompiledBindingException : Exception
    {
        public CompiledBindingException(string message)
            : base(message)
        {
        }

        public CompiledBindingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public CompiledBindingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
