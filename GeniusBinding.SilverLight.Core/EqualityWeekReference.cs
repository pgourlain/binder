using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GeniusBinding.Core
{
    /// <summary>
    /// classe servant à comparer les weakreferences.
    /// </summary>
    sealed class EqualityWeakReference
    {
        private WeakReference _weak;
        // Fields
        private int _hashCode;

        // Methods
        public EqualityWeakReference(object o)            
        {
            _weak = new WeakReference(o);
            this._hashCode = o.GetHashCode();
        }

        public object Target
        {
            get
            {
                return _weak.Target;
            }
        }

        public bool IsAlive
        {
            get
            {
                return _weak.IsAlive;
            }
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
}
