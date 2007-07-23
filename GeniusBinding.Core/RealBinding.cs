using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GeniusBinding.Core
{
    /// <summary>
    /// interface représentant le lien entre un propriété source et une propriété destination
    /// </summary>
    interface IRealBinding
    {
        void Bind(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter);
        void UnBind();
        void SetDestination(object destination);
    }

    /// <summary>
    /// classe concrête
    /// </summary>
    /// <typeparam name="TValueSource"></typeparam>
    /// <typeparam name="TValueDest"></typeparam>
    class RealBinding<TValueSource, TValueDest> : IRealBinding
    {
        WeakReference weakSrc;
        WeakReference weakDst;
        SetHandlerDelegate<TValueSource> sethandler;
        SetHandlerDelegate<TValueDest> sethandlerDest;
        private string _PropNameSource;
        IBinderConverter _Converter;
        OnChangeDelegate<TValueSource> _CurrentChanged;

        public void Bind(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter)
        {
            weakSrc = new WeakReference(source);
            weakDst = new WeakReference(destination);
            _Converter = converter;
            _PropNameSource = piSource.Name;

            GetHandlerDelegate<TValueSource> gethandler = GetSetUtils.CreateGetHandler<TValueSource>(piSource);

            //le set handler devrait être sur le type destination

            if (_Converter != null)
            {
                _CurrentChanged = OnValueChanged1;
                sethandlerDest = GetSetUtils.CreateSetHandler<TValueDest>(piDest);
            }
            else
            {
                _CurrentChanged = OnValueChanged;
                sethandler = GetSetUtils.CreateSetHandler<TValueSource>(piDest);
            }

            DataBinder.AddNotify<TValueSource>(source, piSource.Name, gethandler, _CurrentChanged);
        }

        void OnValueChanged(TValueSource value)
        {
            if (weakDst != null && weakDst.IsAlive)
            {
                sethandler(weakDst.Target, value);
            }
        }

        void OnValueChanged1(TValueSource value)
        {
            if (weakDst != null && weakDst.IsAlive)
            {
                if (_Converter != null)
                {
                    IBinderConverter<TValueDest, TValueSource> cv = _Converter as IBinderConverter<TValueDest, TValueSource>;
                    if (cv == null)
                        throw new Exception(string.Format("converter must implement 'IBinderConverter<{0},{1}>'", typeof(TValueDest), typeof(TValueSource)));
                    sethandlerDest(weakDst.Target, cv.Convert(value));
                }
            }
        }

        public void UnBind()
        {
            DataBinder.RemoveNotify(weakSrc.Target, _PropNameSource, _CurrentChanged);
        }
        
        public void SetDestination(object destination)
        {
            weakDst.Target = destination;
        }
    }

}
