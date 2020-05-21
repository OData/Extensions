//---------------------------------------------------------------------
// <copyright file="ExceptionUtil.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;

namespace Microsoft.OData.Extensions.Migration
{
    /// <summary>
    /// Methods to assist in null checking and other auxiliary functions
    /// </summary>
    internal static class ExceptionUtil
    {
        internal static T IfArgumentNullThrowException<T>(T value, string paramName, string message) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, message);
            }

            return value;
        }

        internal static T IfNullThrowException<T>(T value, string message) where T : class
        {
            if (value == null)
            {
                throw new NullReferenceException(message);
            }

            return value;
        }
    }
}
