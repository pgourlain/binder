using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

namespace GeniusBinding.Core
{
    interface IGetSetHelper
    {
        void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest);
        void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter);
    }

    /// <summary>
    /// classe interne pour la gestion du binding fortement typé
    /// </summary>
    /// <typeparam name="TInstanceSource"></typeparam>
    /// <typeparam name="TValueSource"></typeparam>
    /// <typeparam name="TInstanceDest"></typeparam>
    /// <typeparam name="TValueDest"></typeparam>
    class GetSetHelper<TInstanceSource, TValueSource, TInstanceDest, TValueDest>: IGetSetHelper
    {
        #region addbinding
        public void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest)
        {
            WeakReference weak = new WeakReference(destination);
            GetHandlerDelegate<TValueSource> gethandler = GetSetUtils.CreateGetHandler<TValueSource>(piSource);

            //le set handler devrait être sur le type destination
            SetHandlerDelegate<TValueSource> sethandler = GetSetUtils.CreateSetHandler<TValueSource>(piDest);

            DataBinder.AddNotify<TValueSource>(source, piSource.Name, gethandler, delegate(TValueSource value)
                        {
                            if (weak != null && weak.IsAlive)
                            {
                                sethandler(weak.Target, value);                                    
                            }
                        });
        }
        #endregion

        #region addbinding avec converter
        public void AddBinding(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter)
        {
            WeakReference weak = new WeakReference(destination);
            GetHandlerDelegate<TValueSource> gethandler = GetSetUtils.CreateGetHandler<TValueSource>(piSource);

            //le set handler devrait être sur le type destination
            SetHandlerDelegate<TValueDest> sethandler = GetSetUtils.CreateSetHandler<TValueDest>(piDest);

            DataBinder.AddNotify<TValueSource>(source, piSource.Name, gethandler, delegate(TValueSource value)
                        {
                            if (weak != null && weak.IsAlive)
                            {
                                if (converter != null)
                                {
                                    IBinderConverter<TValueDest, TValueSource> cv = converter as IBinderConverter<TValueDest, TValueSource>;
                                    if (cv == null)
                                        throw new Exception(string.Format("converter must implement 'IBinderConverter<{0},{1}>'", typeof(TValueDest), typeof(TValueSource)));  
                                    sethandler(weak.Target, cv.Convert(value));
                                }
                            }
                        });
        }
        #endregion
    }

}
