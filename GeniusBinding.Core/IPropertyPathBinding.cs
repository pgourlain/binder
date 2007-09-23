using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    public enum PropertyPathBindingState 
    {
        /// <summary>
        /// �tat inconnu ne devrait pas arriver
        /// </summary>
        Unknown, 
        /// <summary>
        /// le binding est complet r�solu
        /// </summary>
        Ok, 
        /// <summary>
        /// la source n'est pas compl�tement bind� (un ou plusieurs sont peut �tre � null, donc inaccessible)
        /// </summary>
        SourceNotBind, 
        /// <summary>
        /// la destination n'est pas compl�tement bind� (un ou plusieurs sont peut �tre � null, donc inaccessible)
        /// </summary>
        DestinationNotBind 
    };
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

        /// <summary>
        /// renvoi true, si l'objet source et destination son encore en vie
        /// </summary>
        bool IsAlive { get;}

        /// <summary>
        /// Retourne l'�tat du binding
        /// cet �tat varie au cours du temps (d�pendant des objets bind�s)
        /// </summary>
        PropertyPathBindingState State { get;}

        /// <summary>
        /// Retourne l'�tat du binding, avec plus d'informations
        /// cet �tat varie au cours du temps (d�pendant des objets bind�s)
        /// </summary>
        /// <returns></returns>
        string GetStateAsString();
    }
}
