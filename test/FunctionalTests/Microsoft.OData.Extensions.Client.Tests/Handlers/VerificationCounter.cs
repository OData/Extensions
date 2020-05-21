//---------------------------------------------------------------------
// <copyright file="VerificationCounter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.Extensions.Client.Tests.Netcore.Handlers
{
    public class VerificationCounter
    {
        public int ODataInvokeCount { get; internal set; }

        public int HttpInvokeCount { get; internal set; }
        public IDictionary<string, object> HttpRequestProperties { get; internal set; } = new Dictionary<string, object>();
    }
}
