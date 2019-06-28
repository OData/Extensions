using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.OData.Migration
{
    /// <summary>
    /// Methods to assist in null checking and other auxiliary functions
    /// </summary>
    internal class EdmUtil
    {
        internal static T IfArgumentNullThrowException<T>(T value, string paramName, string message) where T : class
        {
            if (null == value)
            {
                throw new ArgumentNullException(paramName, message);
            }

            return value;
        }

        internal static T IfNullThrowException<T>(T value, string message) where T : class
        {
            if (null == value)
            {
                throw new NullReferenceException(message);
            }

            return value;
        }
    }
}
