using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    public interface IBinderConverter
    {
    }

    /// <summary>
    /// utiliser pour effectuer une conversion, lors du binding
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IBinderConverter<TResult, TValue> : IBinderConverter
    {
        TResult Convert(TValue value);
    }

    //public delegate TResult TFunc<TResult, TValue>(TValue value);
}
