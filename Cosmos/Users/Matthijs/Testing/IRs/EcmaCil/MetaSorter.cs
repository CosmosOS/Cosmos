using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public static class MetaSorter
    {
        /// <summary>
        /// Sorts the given <paramref name="aTypes"/>. Does this using the Debug DataId. Will throw an exception when not in debug mode
        /// </summary>
        /// <param name="aTypes"></param>
        /// <returns></returns>
        public static TypeMeta[] SortTypes(TypeMeta[] aTypes)
        {
#if !DEBUG
            throw new NotSupportedException("Cannot sort in release builds!");
#else

            var xTempResult = (from item in aTypes
                               orderby (string)item.Data[DataIds.DebugMetaId]
                               select item).ToArray();
            foreach (var xItem in xTempResult)
            {
                SortTypes(xItem);
            }
            return xTempResult;
#endif
        }
#if DEBUG
        private static void SortTypes(TypeMeta aItem)
        {
            aItem.Fields = (from item in aItem.Fields
                            orderby (string)item.Data[DataIds.DebugMetaId]
                            select item).ToList();
            aItem.Methods = (from item in aItem.Methods
                             orderby (string)item.Data[DataIds.DebugMetaId]
                             select item).ToList();
            foreach (var xMethod in aItem.Methods)
            {
                SortMethod(xMethod);
            }
            
        }

        private static void SortMethod(MethodMeta aMethod)
        {
            if (aMethod.Body != null)
            {
                aMethod.Body.ExceptionHandlingClauses = (from item in aMethod.Body.ExceptionHandlingClauses
                                                         orderby item.TryStart,
                                                         item.TryEnd,
                                                         item.Flags,
                                                         item.HandlerStart,
                                                         item.HandlerEnd,
                                                         item.FilterStart,
                                                         item.CatchType
                                                         select item).ToArray();
            }
        }
#endif
    }
}