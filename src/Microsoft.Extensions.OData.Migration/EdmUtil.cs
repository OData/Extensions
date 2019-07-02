﻿// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Methods to assist in null checking and other auxiliary functions
    /// </summary>
    internal class EdmUtil
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
