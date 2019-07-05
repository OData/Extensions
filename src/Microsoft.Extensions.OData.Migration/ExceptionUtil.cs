// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.OData.Edm;
    using Microsoft.Spatial;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;

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
