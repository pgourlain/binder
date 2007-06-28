using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Reflection;

namespace GeniusBinding.Core
{
    /// <summary>
    /// deleguée utiliseée, pour réaliser un "OnChanged" avec la nouvelle valeur
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    internal delegate void OnChangeDelegate<TValue>(TValue value);

    public class DataBinder
    {
        #region pour ne garder que des références faibles sur les objets bindés
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
                EqualityWeakReference other = o as EqualityWeakReference;
                if ((o != this) && (!this.IsAlive || !object.ReferenceEquals(other.Target, this.Target)))
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


        #region gestion du binding sur une propriété
        /// <summary>
        /// classe de base pour 1 binding
        /// </summary>
        abstract class OneBindingBase : IDisposable
        {
            public abstract void Fire(WeakReference weak);

            #region IDisposable Members

            public abstract void Dispose();
            #endregion
        }

        /// <summary>
        /// classe générique pour le typage fort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class OneBinding<T> : OneBindingBase
        {
            private bool _HasCalled;
            GetHandlerDelegate<T> _OnGet;
            public OnChangeDelegate<T> _OnChanged;
            public string PropertyName = string.Empty;

            public OneBinding(GetHandlerDelegate<T> getter, OnChangeDelegate<T> onchanged)
            {
                _OnGet = getter;
                _OnChanged = onchanged;
            }

            public override void Fire(WeakReference weak)
            {
                if (weak.IsAlive)
                {
                    if (_HasCalled)
                        throw new Exception(string.Format("The current binding has recursion =>{0}('{1}')", weak.Target.GetType().FullName, PropertyName));
                    _HasCalled = true;
                    try
                    {
                        _OnChanged(_OnGet(weak.Target));
                    }
                    finally
                    {
                        _HasCalled = false;
                    }
                }
            }

            public override void Dispose()
            {
                _OnChanged = null;
            }
        }
        #endregion

        /// <summary>
        /// liste des abonnements aux sources de données
        /// </summary>
        static Dictionary<WeakReference, Dictionary<string, OneBindingBase>> _SourceChanged = new Dictionary<WeakReference, Dictionary<string, OneBindingBase>>();
        /// <summary>
        /// gestion de la notification sur INotifyPropertyChanged
        /// </summary>
        /// <typeparam name="T">type de la propriété observée</typeparam>
        /// <param name="source">source</param>
        /// <param name="propName">property à observer</param>
        /// <param name="gethandler">"get" a appeler a chaque changements</param>
        /// <param name="OnValueChange">la déléguée à appelée après chaque changed de la source</param>
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
                            //je minimise le nombre de get, en founissant la valeur qui à changé
                            if (dico.ContainsKey(e.PropertyName))
                                dico[e.PropertyName].Fire(weak);
                        };
                }
                ///plusieurs abonnement si nécessaire pour une propriété binding de 1 prop vers plusieurs dest
                OneBinding<T> binding = null;
                if (dico.ContainsKey(propName))
                {
                    binding = (OneBinding<T>)dico[propName];
                    binding._OnChanged = (OnChangeDelegate<T>)Delegate.Combine(binding._OnChanged, OnValueChange);
                }
                else
                {
                    binding = new OneBinding<T>(gethandler, OnValueChange);
                    binding.PropertyName = propName;
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
        /// <summary>
        /// Ajout d'un binding
        /// </summary>
        /// <param name="source">la source à observer</param>
        /// <param name="name">le nom de la propriété à observer</param>
        /// <param name="destination">l'objet destination</param>
        /// <param name="nameDest">la propriété destination</param>
        public static void AddCompiledBinding(object source, string name, object destination, string nameDest)
        {
            PropertyInfo piSource;
            PropertyInfo piDest;
            IGetSetHelper helper = GetHelper(source, name, destination, nameDest, out piSource, out piDest);
            helper.AddBinding(source, piSource, destination, piDest);
        }

        /// <summary>
        /// Ajout d'un binding
        /// </summary>
        /// <param name="source">la source à observer</param>
        /// <param name="name">le nom de la propriété à observer</param>
        /// <param name="destination">l'objet destination</param>
        /// <param name="nameDest">la propriété destination</param>
        /// <param name="converter">converter à utiliser pour "transformer" le type source en type destination</param>
        /// <exception cref="Exception">
        /// Si le converter n'implémente pas IBinderConverter&lt;Source, Destination&gt;
        /// </exception>
        public static void AddCompiledBinding(object source, string name, object destination, string nameDest, IBinderConverter converter)
        {
            PropertyInfo piSource;
            PropertyInfo piDest;

            IGetSetHelper helper = GetHelper(source, name, destination, nameDest, out piSource, out piDest);
            if (converter == null)
                helper.AddBinding(source, piSource, destination, piDest);
            else
                helper.AddBinding(source, piSource, destination, piDest, converter);
        }

        /// <summary>
        /// Retire l'ensemble des binding d'une propriété
        /// </summary>
        /// <param name="source">objet source</param>
        /// <param name="propName">la propriété concernée</param>
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
