using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// interface représentant un binding
    /// </summary>
    public interface IPropertyPathBinding
    {
        /// <summary>
        /// nom de la propriété source
        /// </summary>
        string PropertyPathSource { get;}
        /// <summary>
        /// nom de la propriété destinaion
        /// </summary>
        string PropertyPathDestination { get;}
        /// <summary>
        /// instance de l'objet source
        /// </summary>
        object Source { get;}
        /// <summary>
        /// instance de l'objet destination
        /// </summary>
        object Destination { get;}

        /// <summary>
        /// enlève le binding
        /// </summary>
        void UnBind();

        /// <summary>
        /// Active / désactive le binding
        /// </summary>
        bool Enabled { get; set;}        
    }
}
