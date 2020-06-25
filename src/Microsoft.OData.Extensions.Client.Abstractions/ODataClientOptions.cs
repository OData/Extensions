//---------------------------------------------------------------------
// <copyright file="ODataClientOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.Extensions.Client
{
    /// <summary>
    /// An options class for configuring the default IODataClientFactory.
    /// </summary>
    public class ODataClientOptions
    {
        /// <summary>
        /// The default logic name if no name is specified for the client when creating client from factory.
        /// </summary>
        public const string DefaultName = "ODataClient";

        /// <summary>
        /// Gets a list of operations used to configure an IODataClientFactory.
        /// </summary>
        public IList<IODataClientHandler> ODataHandlers { get; } = new List<IODataClientHandler>();
    }
}
