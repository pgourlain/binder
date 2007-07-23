using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    class Check
    {
        public static void IsNotNull(string argName, object value)
        {
            if (value == null)
                throw new ArgumentNullException(argName, string.Format("{0} can't be null !", argName));
        }

        public static void IsNotNull(string argName, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(argName, string.Format("{0} can't be null or empty!", argName));
        }
    }
}
