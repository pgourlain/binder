using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    public enum PropertyPathBindingState 
    {
        /// <summary>
        /// état inconnu ne devrait pas arriver
        /// </summary>
        Unknown, 
        /// <summary>
        /// le binding est complet résolu
        /// </summary>
        Ok, 
        /// <summary>
        /// la source n'est pas complètement bindé (un ou plusieurs sont peut être à null, donc inaccessible)
        /// </summary>
        SourceNotBind, 
        /// <summary>
        /// la destination n'est pas complètement bindé (un ou plusieurs sont peut être à null, donc inaccessible)
        /// </summary>
        DestinationNotBind 
    };
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

        /// <summary>
        /// renvoi true, si l'objet source et destination son encore en vie
        /// </summary>
        bool IsAlive { get;}

        /// <summary>
        /// Retourne l'état du binding
        /// cet état varie au cours du temps (dépendant des objets bindés)
        /// </summary>
        PropertyPathBindingState State { get;}

        /// <summary>
        /// Retourne l'état du binding, avec plus d'informations
        /// cet état varie au cours du temps (dépendant des objets bindés)
        /// </summary>
        /// <returns></returns>
        string GetStateAsString();
    }
}
