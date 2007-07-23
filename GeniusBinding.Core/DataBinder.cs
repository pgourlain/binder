using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.ObjectModel;

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
        #region gestion de la notification d'une propriété
        /// <summary>
        /// interface définissant 
        /// </summary>
        interface IOneNotify : IDisposable
        {
            void Fire(WeakReference weak);

            void EnableDisable(bool value);

            Delegate OnChanged { get; set;}
        }

        /// <summary>
        /// classe générique pour le typage fort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class OneNotify<T> : IOneNotify
        {
            private bool _HasCalled;
            GetHandlerDelegate<T> _OnGet;
            public OnChangeDelegate<T> _OnChanged;
            public string PropertyName = string.Empty;
            private bool _BindingEnabled = true;

            public OneNotify(GetHandlerDelegate<T> getter, OnChangeDelegate<T> onchanged)
            {
                _OnGet = getter;
                _OnChanged = onchanged;
            }

            #region IOneNotify
            public void Fire(WeakReference weak)
            {
                if (weak.IsAlive && _BindingEnabled)
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

            public void EnableDisable(bool value)
            {
                _BindingEnabled = value;
            }

            public void Dispose()
            {
                _OnChanged = null;
            }
            public Delegate OnChanged
            {
                get { return _OnChanged; }
                set
                {
                    _OnChanged = (OnChangeDelegate<T>)value;
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// liste des abonnements aux sources de données
        /// </summary>
        static Dictionary<WeakReference, Dictionary<string, IOneNotify>> _SourceChanged = new Dictionary<WeakReference, Dictionary<string, IOneNotify>>();
        static List<IPropertyPathBinding> _Bindings = new List<IPropertyPathBinding>();
        
        #region notification
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
                Dictionary<string, IOneNotify> dico = null;
                if (_SourceChanged.ContainsKey(weak))
                {
                    dico = _SourceChanged[weak];
                }
                else
                {
                    dico = new Dictionary<string, IOneNotify>();
                    _SourceChanged.Add(weak, dico);
                    inotify.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                        {
                            //je minimise le nombre de get, en founissant la valeur qui à changé
                            if (dico.ContainsKey(e.PropertyName))
                                dico[e.PropertyName].Fire(weak);
                        };
                }
                ///plusieurs abonnement si nécessaire pour une propriété binding de 1 prop vers plusieurs dest
                OneNotify<T> binding = null;
                if (dico.ContainsKey(propName))
                {
                    binding = (OneNotify<T>)dico[propName];
                    binding._OnChanged = (OnChangeDelegate<T>)Delegate.Combine(binding._OnChanged, OnValueChange);
                }
                else
                {
                    binding = new OneNotify<T>(gethandler, OnValueChange);
                    binding.PropertyName = propName;
                    dico[propName] = binding;
                }
            }
        }

        internal static void RemoveNotify(object source, string propName, Delegate del)
        {
            INotifyPropertyChanged inotify = source as INotifyPropertyChanged;
            if (inotify != null)
            {
                WeakReference weak = new EqualityWeakReference(inotify);
                //le but est ici de ne s'abonner qu'une fois
                Dictionary<string, IOneNotify> dico = null;
                if (_SourceChanged.ContainsKey(weak))
                {
                    dico = _SourceChanged[weak];
                    if (dico.ContainsKey(propName))
                    {
                        dico[propName].OnChanged = Delegate.Remove(dico[propName].OnChanged, del);
                        if (dico[propName].OnChanged == null)
                            dico.Remove(propName);
                    }
                }
            }
        }

        internal static void RemoveBinding(IPropertyPathBinding binding)
        {
            _Bindings.Remove(binding);
        }
        #endregion

        #region public methods
        public static void AddCompiledBinding(object source, string propertypath, object destination, string destPropertypath)
        {
            Check.IsNotNull("source", source);
            Check.IsNotNull("propertypath", propertypath);
            Check.IsNotNull("destination", destination);
            Check.IsNotNull("destPropertypath", destPropertypath);
            string[] pathItems = propertypath.Split('.');

            
            PropertyPathBindingItem item = new PropertyPathBindingItem(source, propertypath, destination, destPropertypath, null);
            _Bindings.Add(item);
        }

        /// <summary>
        /// ajoute un binding
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertypath"></param>
        /// <param name="destination"></param>
        /// <param name="destPropertypath"></param>
        /// <param name="converter"></param>
        public static void AddCompiledBinding(object source, string propertypath, object destination, string destPropertypath, IBinderConverter converter)
        {
            Check.IsNotNull("source", source);
            Check.IsNotNull("propertypath", propertypath);
            Check.IsNotNull("destination", destination);
            Check.IsNotNull("destPropertypath", destPropertypath);
            string[] pathItems = propertypath.Split('.');


            PropertyPathBindingItem item = new PropertyPathBindingItem(source, propertypath, destination, destPropertypath, converter);
            _Bindings.Add(item);
        }

        /// <summary>
        /// enlève un binding
        /// </summary>
        /// <param name="source">instance de la source concernée</param>
        /// <param name="propertypath"></param>
        /// <param name="destination"></param>
        /// <param name="destPropertypath"></param>
        public static void RemoveCompiledBinding(object source, string propertypath, object destination, string destPropertypath)
        {
            List<IPropertyPathBinding> lToUnbind = new List<IPropertyPathBinding>();
            //find it and unbind
            foreach (IPropertyPathBinding item in Bindings)
            {
                if (item.PropertyPathSource == propertypath && item.PropertyPathDestination == destPropertypath &&
                    item.Source == source && item.Destination == destination)
                {
                    lToUnbind.Add(item);
                }
            }
            //
            foreach (IPropertyPathBinding item in lToUnbind)
                item.UnBind();
        }

        /// <summary>
        /// enlève un binding
        /// </summary>
        /// <param name="binding"></param>
        public static void RemoveCompiledBinding(IPropertyPathBinding binding)
        {
            binding.UnBind();
        }

        /// <summary>
        /// Active/Désctive un binding à partir de sa source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void EnableDisableBinding(object source, string name, bool value)
        {
            EqualityWeakReference src =new EqualityWeakReference(source);
            if (_SourceChanged.ContainsKey(src))
            {
                Dictionary<string, IOneNotify> dico = _SourceChanged[src];
                if (dico.ContainsKey(name))
                {
                    dico[name].EnableDisable(value);
                }
            }
        }

        /// <summary>
        /// Liste complète des bindings
        /// </summary>
        public static ReadOnlyCollection<IPropertyPathBinding> Bindings
        {
            get
            {
                return new ReadOnlyCollection<IPropertyPathBinding>(_Bindings);
            }
        }
        #endregion
    }
}
