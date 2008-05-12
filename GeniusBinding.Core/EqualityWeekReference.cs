using System;
using System.Collections.Generic;
using System.Text;
using System.Security;

namespace GeniusBinding.Core
{
    /// <summary>
    /// le but de cette classe est de ne maintenir que des références "faibles" sur les objets bindés
    /// </summary>
    class EqualityWeakReference : WeakReference
    {
        // Fields
        private int _hashCode;

        // Methods
        public EqualityWeakReference(object o)
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
}
