using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// utiliser pour v�hiculer le converter de mani�re g�n�rique
    /// </summary>
    public interface IBinderConverter
    {
    }

    /// <summary>
    /// utiliser pour effectuer une conversion, lors du binding
    /// </summary>
    /// <typeparam name="TResult">Type de la propri�t� destination</typeparam>
    /// <typeparam name="TValue">Type de la propri�t� source</typeparam>
    public interface IBinderConverter<TResult, TValue> : IBinderConverter
    {
        TResult Convert(TValue value);
    }
}
