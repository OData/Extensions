// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.OData.Extensions.Query
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable querying using the OData query
    /// syntax.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class EnableQueryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionExecutedContext)
        {
            // TODO:
        }

        /// <summary>
        /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
        /// </summary>
        /// <param name="actionExecutedContext">The context related to this action, including the response message,
        /// request message and HttpConfiguration etc.</param>
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            // TODO:
        }
    }
}
