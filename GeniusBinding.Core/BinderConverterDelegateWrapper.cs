using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// implements a wrapping around a BinderConverterDelegate, used by <see cref="DataBinder.CreateConverter"/>
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    class BinderConverterDelegateWrapper<TResult, TValue> : IBinderConverter<TResult, TValue>
    {
        BinderConverterDelegate<TResult, TValue> _converterDelegate;

        public BinderConverterDelegateWrapper(BinderConverterDelegate<TResult, TValue> converterDelegate)
        {
            _converterDelegate = converterDelegate;
        }

        #region IBinderConverter<TResult,TValue> Members

        public TResult Convert(TValue value)
        {
            return _converterDelegate(value);
        }

        #endregion
    }
}
