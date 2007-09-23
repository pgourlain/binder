using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace GeniusBinding.Core
{
    /// <summary>
    /// some functions to get / set value in collections
    /// </summary>
    class ArrayUtil
    {
        public static object GetItem(object collection, int index)
        {
            IList nonTypedList = collection as IList;

            if (nonTypedList != null)
            {
                if (nonTypedList.Count > index)
                    return nonTypedList[index];
                return null;
            }
            return null;
        }
    }
}
