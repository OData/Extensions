//---------------------------------------------------------------------
// <copyright file="ODataErrorFactory.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.OData.Extensions.Errors
{
    /// <summary>
    /// Generates an <see cref="T:Microsoft.OData.ODataError" /> object with a preset error hierarchy.
    /// </summary>
    public class ODataErrorFactory
    {
        /// <summary>
        /// Creates an <see cref="T:Microsoft.OData.ODataError" /> object with populated properties.
        /// This factory sets the hierarchy of error codes and messages to drive consistency across
        /// workloads in Microsoft Graph. See https://developer.microsoft.com/en-us/graph/docs/concepts/errors
        /// for details on what these errors are. Do not overwrite the ODataError.ErrorCode and ODataError.Message.
        /// </summary>
        /// <returns>An <see cref="T:Microsoft.OData.ODataError" /> object with populated properties.</returns>
        /// <param name="graphError">
        /// The enum indicates the hierarchy of error messages that will be nested in the resulting
        /// <see cref="T:Microsoft.OData.ODataError" /> object.
        /// </param>
        /// <param name="odataInnerError">
        /// The workload's <see cref="T:Microsoft.OData.ODataInnerError" />. The inner error may contain nested inner
        /// errors to hold more information.
        /// </param>
        public ODataError Create(Error error, ODataInnerError odataInnerError)
        {
            ODataError resultingError = new ODataError
            {
                ErrorCode = error.ErrorCode,
                Message = error.ErrorMessage,
                InnerError = odataInnerError
            };

            return resultingError;
        }
    }
}
