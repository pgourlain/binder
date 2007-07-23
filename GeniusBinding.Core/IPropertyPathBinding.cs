using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// interface repr�sentant un binding
    /// </summary>
    public interface IPropertyPathBinding
    {
        /// <summary>
        /// nom de la propri�t� source
        /// </summary>
        string PropertyPathSource { get;}
        /// <summary>
        /// nom de la propri�t� destinaion
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
        /// enl�ve le binding
        /// </summary>
        void UnBind();

        /// <summary>
        /// Active / d�sactive le binding
        /// </summary>
        bool Enabled { get; set;}        
    }
}
