using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// interface d�finissant la notification d'une propri�t�
    /// </summary>
    interface IOneNotify : IDisposable
    {
        void Fire(EqualityWeakReference weak);

        bool Enabled { get; set;}

        Delegate OnChanged { get; set;}

        EventHandler PropertyChangedEvent { get; }
    }
}
