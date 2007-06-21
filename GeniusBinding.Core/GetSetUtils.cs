using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace GeniusBinding.Core
{
    class GetSetUtils
    {
        /// <summary>
        /// cache pour les méthodes dynamiques créées
        /// </summary>
       static Dictionary<MethodInfo, Delegate> _Dico = new Dictionary<MethodInfo, Delegate>();
        /// <summary>
        /// créée une méthode dynamique, pour lire le contenu d'une propriété sans utiliser la réflection
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static GetHandlerDelegate<TValue> CreateGetHandler<TValue, TInstance>(PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod(true);
            if (_Dico.ContainsKey(getMethod))
            {
                return (GetHandlerDelegate<TValue>)_Dico[getMethod];
            }
            DynamicMethod dynamicGet = new DynamicMethod("DynamicGet"+propertyInfo.Name, typeof(TValue), new Type[] { typeof(object) }, typeof(TInstance), true);
            ILGenerator getGenerator = dynamicGet.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Call, getMethod);
            getGenerator.Emit(OpCodes.Ret);

            Type tDelegate = typeof(GetHandlerDelegate<TValue>);
            GetHandlerDelegate<TValue> Result = (GetHandlerDelegate<TValue>)dynamicGet.CreateDelegate(tDelegate);
            _Dico[getMethod] = Result;
            return Result;
        }


        /// <summary>
        /// créée une méthode dynamique pour acceder à une propriété sans la réflection
        /// </summary>
        /// <typeparam name="TValue">type de la valeur</typeparam>
        /// <typeparam name="TInstance">type contenant la propriété</typeparam>
        /// <param name="propertyInfo">propertyInfo de la propriété concernée</param>
        /// <returns></returns>
        internal static SetHandlerDelegate<TValue> CreateSetHandler<TValue, TInstance>(PropertyInfo propertyInfo)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod(true);
            if (_Dico.ContainsKey(setMethod))
            {
                return (SetHandlerDelegate<TValue>)_Dico[setMethod];
            }
            DynamicMethod dynamicSet = new DynamicMethod("DynamicSet" + propertyInfo.Name, typeof(void), new Type[] { typeof(object), typeof(TValue) }, typeof(TInstance), true);
            ILGenerator setGenerator = dynamicSet.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            setGenerator.Emit(OpCodes.Call, setMethod);
            setGenerator.Emit(OpCodes.Ret);

            Type tDelegate = typeof(SetHandlerDelegate<TValue>);
            SetHandlerDelegate<TValue> Result = (SetHandlerDelegate<TValue>)dynamicSet.CreateDelegate(tDelegate);
            _Dico[setMethod] = Result;
            return Result;
        }
    }
}
