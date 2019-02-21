//---------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

using System.Net;

namespace Microsoft.OData.Extensions.Errors
{
    /// <summary>
    /// Constant values used in this library. These errors are documented on MS Graph docs:
    /// https://developer.microsoft.com/en-us/graph/docs/concepts/errors#code-property
    /// 
    /// They are represented at the top level of an ODataError object, where the serialized JSON form looks like:
    /// {
    ///   "code": "accessDenied",
    ///   "message": "The caller doesn't have permission to perform the action.",
    ///   "innerError": ...
    /// </summary>
    public abstract class Error
    {
        public string ErrorMessage { get; set; }

        public string ErrorCode { get; set; }

        public string InnerErrorMessage { get; set; }

        public string InnerErrorCode { get; set; }
    }
}
