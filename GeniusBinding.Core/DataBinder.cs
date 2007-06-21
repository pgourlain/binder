using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Reflection;

namespace GeniusBinding.Core
{
    internal delegate void OnChangeDelegate<TValue>(TValue value);

    public class DataBinder
    {
        #region pour ne garder que des r�f�rences faibles sur les objets bind�s
        private sealed class EqualityWeakReference : WeakReference
        {
            // Fields
            private int _hashCode;

            // Methods
            internal EqualityWeakReference(object o)
                : base(o)
            {
                this._hashCode = o.GetHashCode();
            }

            public override bool Equals(object o)
            {
                if (o == null)
                {
                    return false;
                }
                if (o.GetHashCode() != this._hashCode)
                {
                    return false;
                }
                if ((o != this) && (!this.IsAlive || !object.ReferenceEquals(o, this.Target)))
                {
                    return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                return this._hashCode;
            }
        }
        #endregion


        #region gestion du binding sur une propri�t�
        abstract class OneBindingBase : IDisposable
        {
            public abstract void Fire(WeakReference weak);

            #region IDisposable Members

            public abstract void Dispose();
            #endregion
        }

        class OneBinding<T> : OneBindingBase
        {
            GetHandlerDelegate<T> _OnGet;
            public OnChangeDelegate<T> _OnChanged;

            public OneBinding(GetHandlerDelegate<T> getter, OnChangeDelegate<T> onchanged)
            {
                _OnGet = getter;
                _OnChanged = onchanged;
            }

            public override void Fire(WeakReference weak)
            {
                if (weak.IsAlive)
                    _OnChanged(_OnGet(weak.Target));
            }

            public override void Dispose()
            {
                _OnChanged = null;
            }
        }
        #endregion

        static Dictionary<WeakReference, Dictionary<string, OneBindingBase>> _SourceChanged = new Dictionary<WeakReference, Dictionary<string, OneBindingBase>>();
        /// <summary>
        /// gestion de la notification sur INotifyPropertyChanged
        /// </summary>
        /// <typeparam name="T">type de la propri�t� observ�e</typeparam>
        /// <param name="source">source</param>
        /// <param name="propName">property � observer</param>
        /// <param name="gethandler">"get" a appeler a chaque changements</param>
        /// <param name="OnValueChange">la d�l�gu�e � appel�e apr�s chaque changed de la source</param>
        internal static void AddNotify<T>(object source, string propName, GetHandlerDelegate<T> gethandler, OnChangeDelegate<T> OnValueChange)
        {
            INotifyPropertyChanged inotify = source as INotifyPropertyChanged;
            if (inotify != null)
            {
                WeakReference weak = new EqualityWeakReference(inotify);
                //le but est ici de ne s'abonner qu'une fois
                Dictionary<string, OneBindingBase> dico = null;
                if (_SourceChanged.ContainsKey(weak))
                {
                    dico = _SourceChanged[weak];
                }
                else
                {   
                    dico = new Dictionary<string, OneBindingBase>();
                    _SourceChanged.Add(weak, dico);
                    inotify.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                        {
                            //je minimise le nombre de get, en founissant la valeur qui � chang�
                            if (dico.ContainsKey(e.PropertyName))
                                dico[e.PropertyName].Fire(weak);
                        };
                }
                ///plusieurs abonnement si n�cessaire pour une propri�t� binding de 1 prop vers plusieurs dest
                OneBinding<T> binding = null;
                if (dico.ContainsKey(propName))
                {
                    binding = (OneBinding<T>)dico[propName];
                    binding._OnChanged = (OnChangeDelegate<T>)Delegate.Combine(binding._OnChanged, OnValueChange);
                }
                else
                {
                    binding = new OneBinding<T>(gethandler, OnValueChange);
                    dico[propName] = binding;
                }
            }
        }

        private static IGetSetHelper GetHelper(object source, string name, object destination, string nameDest, out PropertyInfo piSource, out PropertyInfo piDest)
        {
            piSource = source.GetType().GetProperty(name);
            piDest = destination.GetType().GetProperty(nameDest);
            Type t = typeof(GetSetHelper<,,,>);
            Type helperType = t.MakeGenericType(source.GetType(), piSource.PropertyType, destination.GetType(), piDest.PropertyType);
            return (IGetSetHelper)Activator.CreateInstance(helperType);
        }

        #region public methods
        public static void AddCompiledBinding(object source, string name, object destination, string nameDest)
        {
            PropertyInfo piSource;
            PropertyInfo piDest;
            IGetSetHelper helper = GetHelper(source, name, destination, nameDest, out piSource, out piDest);
            helper.AddBinding(source, piSource, destination, piDest);
        }

        public static void AddCompiledBinding<TResult, TSource>(object source, string name, object destination, string nameDest, IBinderConverter<TResult, TSource> converter)
        {
            PropertyInfo piSource;
            PropertyInfo piDest;

            IGetSetHelper helper = GetHelper(source, name, destination, nameDest, out piSource, out piDest);
            if (converter == null)
                helper.AddBinding(source, piSource, destination, piDest);
            else
                helper.AddBinding(source, piSource, destination, piDest, converter);
        }

        public static void RemoveCompiledBinding(object source, string propName)
        {
            INotifyPropertyChanged inotify = source as INotifyPropertyChanged;
            if (inotify != null)
            {
                WeakReference weak = new EqualityWeakReference(inotify);
                if (_SourceChanged.ContainsKey(weak))
                {
                    Dictionary<string, OneBindingBase> dico = _SourceChanged[weak];
                    if (dico.ContainsKey(propName))
                    {
                        dico[propName].Dispose();
                        dico.Remove(propName);
                    }
                }
            }
        }
        #endregion
    }
}