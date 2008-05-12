using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace GeniusBinding.Core
{
    /// <summary>
    /// Exception pour le binding compilé
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
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
#if !SILVERLIGHT
        public CompiledBindingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

    }
}
