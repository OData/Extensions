//---------------------------------------------------------------------
// <copyright file="ODataV3ClientOptions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.Extensions.V3Client
{
    /// <summary>
    /// An options class for configuring the default IODataV3ClientFactory.
    /// </summary>
    public class ODataV3ClientOptions
    {
        /// <summary>
        /// The default logic name if no name is specified for the client when creating client from factory.
        /// </summary>
        public const string DefaultName = "ODataV3Client";

        /// <summary>
        /// Gets a list of operations used to configure an IODataV3ClientFactory.
        /// </summary>
        public IList<IODataV3ClientHandler> ODataHandlers { get; } = new List<IODataV3ClientHandler>();
    }
}
