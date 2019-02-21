//---------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.UnitTests
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
    internal static class Constants
    {
        /// <summary>
        /// These strings represent the top level error codes of the response.
        /// </summary>
        internal static class ErrorCodes
        {
            internal static class Level1
            {
                internal const string Http400_BadRequest = "BadRequest";
                internal const string Http401_Unauthorized = "Unauthorized";
                internal const string Http403_AccessDenied = "AccessDenied";
                internal const string Http404_ResourceNotFound = "ResourceNotFound";
                internal const string Http405_MethodNotAllowed = "MethodNotAllowed";
                internal const string Http406_FormatNotAcceptable = "FormatNotAcceptable";
                internal const string Http409_Conflict = "Conflict";
                internal const string Http410_ResourceGone = "ResourceGone";
                internal const string Http411_ContentLengthRequired = "ContentLengthRequired";
                internal const string Http412_PreconditionFailed = "PreconditionFailed";
                internal const string Http413_RequestSizeExceeded = "RequestSizeExceeded";
                internal const string Http415_ContentTypeNotSupported = "ContentTypeNotSupported";
                internal const string Http416_InvalidRange = "InvalidRange";
                internal const string Http418_Teapot = "Teapot";
                internal const string Http422_UnprocessableEntity = "UnprocessableEntity";
                internal const string Http429_ActivityLimitReached = "ActivityLimitReached";
                internal const string Http500_InternalServerError = "InternalServerError";
                internal const string Http501_NotImplemented = "NotImplemented";
                internal const string Http503_ServiceUnavailable = "ServiceUnavailable";
                internal const string Http504_GatewayTimeout = "GatewayTimeout";
                internal const string Http507_InsufficientStorage = "InsufficientStorage";
                internal const string Http509_BandwidthLimitExceeded = "BandwidthLimitExceeded";
            }

            internal static class Level2
            {
                #region HTTP 400 Bad Request
                internal const string Http400_BadRequest_RequiredFieldOrParameterMissing = "RequiredFieldOrParameterMissing";
                internal const string Http400_BadRequest_InvalidCombinationOfFields = "InvalidCombinationOfFields";
                internal const string Http400_BadRequest_InvalidBody = "InvalidBody";
                #endregion

                #region HTTP 401 Unauthorized
                internal const string Http401_Unauthorized_MissingAuthenticationInfo = "MissingAuthenticationInfo";
                internal const string Http401_Unauthorized_InvalidAuthenticationInfo = "InvalidAuthenticationInfo";
                internal const string Http401_Unauthorized_ExpiredAuthenticationToken = "ExpiredAuthenticationToken";
                #endregion

                #region HTTP 403 Access Denied
                internal const string Http403_AccessDenied_ConditionalAccess = "ConditionalAccess";
                internal const string Http403_AccessDenied_CountryForbidden = "CountryForbidden";
                #endregion

                #region HTTP 409 Conflict
                internal const string Http409_Conflict_ConcurrencyIssue = "ConcurrencyIssue";
                #endregion

                #region HTTP 412 Precondition Failed
                internal const string Http412_PreconditionFailed_ResourceModified = "ResourceModified";
                internal const string Http412_PreconditionFailed_ResyncRequired = "ResyncRequired";
                #endregion

                #region HTTP 422 Unprocessable Entity
                internal const string Http422_UnprocessableEntity_InvalidOrUnacceptedUpload = "InvalidOrUnacceptedUpload";
                #endregion

                #region HTTP 503 Service Unavailable
                internal const string Http503_ServiceUnavailable_Maintenance = "Maintenance";
                internal const string Http503_ServiceUnavailable_Overload = "Overload";
                internal const string Http503_ServiceUnavailable_Retry = "Retry";
                #endregion
            }
        }

        /// <summary>
        /// These strings represent the error messages of the response.
        /// </summary>
        internal static class ErrorMessages
        {
            internal static class Level1
            {
                internal const string Http400_BadRequest = "Cannot process the request because it is malformed or incorrect.";
                internal const string Http401_Unauthorized = "Required authentication information is either missing or not valid for the resource.";
                internal const string Http403_AccessDenied = "Access is denied to the requested resource.";
                internal const string Http404_ResourceNotFound = "The requested resource does not exist.";
                internal const string Http405_MethodNotAllowed = "The HTTP method in the request is not allowed on the resource.";
                internal const string Http406_FormatNotAcceptable = "The format requested in the 'Accept' header is not accepted.";
                internal const string Http409_Conflict = "The current state conflicts with what the request expects.";
                internal const string Http410_ResourceGone = "The requested resource is no longer available at the server.";
                internal const string Http411_ContentLengthRequired = "A 'Content-Length' header is required on the request.";
                internal const string Http412_PreconditionFailed = "A precondition provided in the request (such as an if-match header) does not match the resource's current state.";
                internal const string Http413_RequestSizeExceeded = "The request size exceeds the maximum limit.";
                internal const string Http415_ContentTypeNotSupported = "The content type of the request is a format that is not supported by the service.";
                internal const string Http416_InvalidRange = "The specified byte range is invalid or unavailable.";
                internal const string Http418_Teapot = "I'm a teapot.";
                internal const string Http422_UnprocessableEntity = "Cannot process the request because it is semantically incorrect.";
                internal const string Http429_ActivityLimitReached = "The client application has been throttled for reaching an activity limit. The request may be repeated after a delay, the length of which may be specified in a 'Retry-After' header.";
                internal const string Http500_InternalServerError = "There was an internal server error while processing the request.";
                internal const string Http501_NotImplemented = "The requested feature is not implemented.";
                internal const string Http503_ServiceUnavailable = "The service is temporarily unavailable.";
                internal const string Http504_GatewayTimeout = "An internal service or proxy was unresponsive to complete the request.";
                internal const string Http507_InsufficientStorage = "The maximum storage quota has been reached.";
                internal const string Http509_BandwidthLimitExceeded = "The client application has been throttled for exceeding the maximum bandwidth limit. The request may be repeated after a delay, the length of which may be specified in a 'Retry-After' header.";
            }

            internal static class Level2
            {
                #region HTTP 400 Bad Request
                internal const string Http400_BadRequest_RequiredFieldOrParameterMissing = "Cannot process the request because a required field or parameter is missing.";
                internal const string Http400_BadRequest_InvalidCombinationOfFields = "Cannot process the request because the combination of fields is invalid.";
                internal const string Http400_BadRequest_InvalidBody = "Cannot process the request because the body could be parsed correctly.";
                #endregion

                #region HTTP 401 Unauthorized
                internal const string Http401_Unauthorized_MissingAuthenticationInfo = "Required authentication information is missing.";
                internal const string Http401_Unauthorized_InvalidAuthenticationInfo = "Required authentication information is invalid for the resource.";
                internal const string Http401_Unauthorized_ExpiredAuthenticationToken = "The authentication token has expired.";
                #endregion

                #region HTTP 403 Access Denied
                internal const string Http403_AccessDenied_ConditionalAccess = "Access is denied to the requested resource due to conditional permissions.";
                internal const string Http403_AccessDenied_CountryForbidden = "Access is denied to the requested resource due to law restrictions for certain countries.";
                #endregion

                #region HTTP 409 Conflict
                internal const string Http409_Conflict_ConcurrencyIssue = "The service failed to commit the change due to a conflict from a concurrent request to the same resource.";
                #endregion

                #region HTTP 412 Precondition Failed
                internal const string Http412_PreconditionFailed_ResourceModified = "A precondition provided in the request (such as an if-match header) does not match the resource's current state. The resource had been modified.";
                internal const string Http412_PreconditionFailed_ResyncRequired = "A precondition provided in the request (such as an if-match header) does not match the resource's current state. A resource re-sync is required.";
                #endregion

                #region HTTP 422 Unprocessable Entity
                internal const string Http422_UnprocessableEntity_InvalidOrUnacceptedUpload = "Cannot process the request because the payload is invalid or unaccepted.";
                #endregion

                #region HTTP 503 Service Unavailable
                internal const string Http503_ServiceUnavailable_Maintenance = "The service is temporarily unavailable due to maintenance.";
                internal const string Http503_ServiceUnavailable_Overload = "The service is temporarily unavailable due to overloaded resources.";
                internal const string Http503_ServiceUnavailable_Retry = "The service is temporarily unavailable. The request may be repeated after a delay, the length of which may be specified in a 'Retry-After' header.";
                #endregion
            }
        }
    }
}
