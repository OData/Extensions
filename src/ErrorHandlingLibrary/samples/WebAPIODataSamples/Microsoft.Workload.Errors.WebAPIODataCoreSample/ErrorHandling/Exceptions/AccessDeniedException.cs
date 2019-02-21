//---------------------------------------------------------------------
// <copyright file="NotAllowedException.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataCoreSample.ErrorHandling
{
    using System;

    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; set; }
    }
}