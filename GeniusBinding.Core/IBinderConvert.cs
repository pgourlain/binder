using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// utiliser pour véhiculer le converter de manière générique
    /// </summary>
    public interface IBinderConverter
    {
    }

    /// <summary>
    /// utiliser pour effectuer une conversion, lors du binding
    /// </summary>
    /// <typeparam name="TResult">Type de la propriété destination</typeparam>
    /// <typeparam name="TValue">Type de la propriété source</typeparam>
    public interface IBinderConverter<TResult, TValue> : IBinderConverter
    {
        TResult Convert(TValue value);
    }
}
