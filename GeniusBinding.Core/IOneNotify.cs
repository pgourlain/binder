using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// interface définissant la notification d'une propriété
    /// </summary>
    interface IOneNotify : IDisposable
    {
        void Fire(WeakReference weak);

        bool Enabled { get; set;}

        Delegate OnChanged { get; set;}

        EventHandler PropertyChangedEvent { get; }
    }
}
