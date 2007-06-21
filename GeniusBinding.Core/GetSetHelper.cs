using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

namespace GeniusBinding.Core
{
    /// <summary>
    /// Handler generic pour le get
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public delegate TResult GetHandlerDelegate<TResult>(object source);
    /// <summary>
    /// Handler generic pour le set
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="source"></param>
    /// <param name="value"></param>
    public delegate void SetHandlerDelegate<TValue>(object destination, TValue value);

    interface IGetSetHelper
    {
        void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest);
        void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter);
    }

    class GetSetHelper<TInstanceSource, TValueSource, TInstanceDest, TValueDest>: IGetSetHelper
    {

        public void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest)
        {
            WeakReference weak = new WeakReference(destination);
            GetHandlerDelegate<TValueSource> gethandler = GetSetUtils.CreateGetHandler<TValueSource, TInstanceSource>(piSource);

            //le set handler devrait être sur le type destination
            SetHandlerDelegate<TValueSource> sethandler = GetSetUtils.CreateSetHandler<TValueSource, TInstanceDest>(piDest);

            DataBinder.AddNotify<TValueSource>(source, piSource.Name, gethandler, delegate(TValueSource value)
                        {
                            if (weak != null && weak.IsAlive)
                            {
                                sethandler(weak.Target, value);                                    
                            }
                        });
        }

        public void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter)
        {
            WeakReference weak = new WeakReference(destination);
            GetHandlerDelegate<TValueSource> gethandler = GetSetUtils.CreateGetHandler<TValueSource, TInstanceSource>(piSource);

            //le set handler devrait être sur le type destination
            SetHandlerDelegate<TValueDest> sethandler = GetSetUtils.CreateSetHandler<TValueDest, TInstanceDest>(piDest);

            DataBinder.AddNotify<TValueSource>(source, piSource.Name, gethandler, delegate(TValueSource value)
                        {
                            if (weak != null && weak.IsAlive)
                            {
                                if (converter != null)
                                {
                                    IBinderConverter<TValueDest, TValueSource> cv = converter as IBinderConverter<TValueDest, TValueSource>;
                                    //if (cv == null)
                                    //    throw new Exception(string.Format("converter must implement 'IBinderConverter<{0},{1}>'",typeof(TValueDest), typeof(TValueSource)));  
                                    sethandler(weak.Target, cv.Convert(value));
                                }
                            }
                        });
        }

    }

}
