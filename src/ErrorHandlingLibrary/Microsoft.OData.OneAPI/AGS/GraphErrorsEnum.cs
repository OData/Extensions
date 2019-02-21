//---------------------------------------------------------------------
// <copyright file="GraphErrors.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors
{
    /// <summary>Enumeration of errors seen in responses from Microsoft Graph.</summary>
    /// <remarks>
    /// See https://developer.microsoft.com/en-us/graph/docs/concepts/errors#code-property for details.
    /// Format: HttpCode_ErrorCode_ErrorMessage
    /// </remarks>
    public enum GraphErrors
    {
        #region HTTP 400 Bad Request
        /// <summary>
        /// Cannot process the request because it is malformed or incorrect.
        /// </summary>
        Http400_BadRequest,

        /// <summary>
        /// Cannot process the request because a required field or parameter is missing.
        /// </summary>
        Http400_BadRequest_RequiredFieldOrParameterMissing,

        /// <summary>
        /// Cannot process the request because the combination of fields is invalid.
        /// </summary>
        Http400_BadRequest_InvalidCombinationOfFields,

        /// <summary>
        /// Cannot process the request because the body could be parsed correctly.
        /// </summary>
        Http400_BadRequest_InvalidBody,
        #endregion

        #region HTTP 401 Unauthorized
        /// <summary>
        /// Required authentication information is either missing or not valid for the resource.
        /// </summary>
        Http401_Unauthorized,

        /// <summary>
        /// Required authentication information is missing.
        /// </summary>
        Http401_Unauthorized_MissingAuthenticationInfo,

        /// <summary>
        /// Required authentication information is invalid for the resource.
        /// </summary>
        Http401_Unauthorized_InvalidAuthenticationInfo,

        /// <summary>
        /// The authentication token has expired.
        /// </summary>
        Http401_Unauthorized_ExpiredAuthenticationToken,
        #endregion

        #region HTTP 403 Access Denied
        /// <summary>
        /// Access is denied to the requested resource.
        /// </summary>
        Http403_AccessDenied,

        /// <summary>
        /// Access is denied to the requested resource due to conditional permissions.
        /// </summary>
        Http403_AccessDenied_ConditionalAccess,

        /// <summary>
        /// Access is denied to the requested resource due to law restrictions for certain countries.
        /// </summary>
        Http403_AccessDenied_CountryForbidden,
        #endregion

        #region HTTP 404 Resource Not Found
        /// <summary>
        /// The requested resource does not exist.
        /// </summary>
        Http404_ResourceNotFound,
        #endregion

        #region HTTP 405 Method Not Allowed
        /// <summary>
        /// The HTTP method in the request is not allowed on the resource.
        /// </summary>
        Http405_MethodNotAllowed,
        #endregion

        #region HTTP 406 Format Not Acceptable
        /// <summary>
        /// The format requested in the 'Accept' header is not accepted.
        /// </summary>
        Http406_FormatNotAcceptable,
        #endregion

        #region HTTP 409 Conflict
        /// <summary>
        /// The current state conflicts with what the request expects.
        /// </summary>
        Http409_Conflict,

        /// <summary>
        /// The service failed to commit the change due to a conflict from a concurrent request to the same resource.
        /// </summary>
        Http409_Conflict_ConcurrencyIssue,
        #endregion

        #region HTTP 410 Resource Gone
        /// <summary>
        /// The requested resource is no longer available at the server.
        /// </summary>
        Http410_ResourceGone,
        #endregion

        #region HTTP 411 Content Length Required
        /// <summary>
        /// A 'Content-Length' header is required on the request.
        /// </summary>
        Http411_ContentLengthRequired,
        #endregion

        #region HTTP 412 Precondition Failed
        /// <summary>
        /// A precondition provided in the request (such as an if-match header) does not match the resource's current state.
        /// </summary>
        Http412_PreconditionFailed,

        /// <summary>
        /// A precondition provided in the request (such as an if-match header) does not match the resource's current state. The resource had been modified.
        /// </summary>
        Http412_PreconditionFailed_ResourceModified,

        /// <summary>
        /// A precondition provided in the request (such as an if-match header) does not match the resource's current state. A resource re-sync is required.
        /// </summary>
        Http412_PreconditionFailed_ResyncRequired,
        #endregion

        #region HTTP 413 Request Size Exceeded
        /// <summary>
        /// The request size exceeds the maximum limit.
        /// </summary>
        Http413_RequestSizeExceeded,
        #endregion

        #region HTTP 415 Content Type Not Supported
        /// <summary>
        /// The content type of the request is a format that is not supported by the service.
        /// </summary>
        Http415_ContentTypeNotSupported,
        #endregion

        #region HTTP 416 Invalid Range
        /// <summary>
        /// The specified byte range is invalid or unavailable.
        /// </summary>
        Http416_InvalidRange,
        #endregion

        #region HTTP 418 Teapot
        /// <summary>
        /// I'm a teapot.
        /// </summary>
        Http418_Teapot,
        #endregion

        #region HTTP 422 Unprocessable Entity
        /// <summary>
        /// Cannot process the request because it is semantically incorrect.
        /// </summary>
        Http422_UnprocessableEntity,

        /// <summary>
        /// Cannot process the request because the payload is invalid or unaccepted.
        /// </summary>
        Http422_UnprocessableEntity_InvalidOrUnacceptedUpload,
        #endregion

        #region HTTP 429 Activity Limit Reached
        /// <summary>
        /// The client application has been throttled for reaching an activity limit. The request may be repeated after a delay, the length of which may be specified in a 'Retry-After' header.
        /// </summary>
        Http429_ActivityLimitReached,
        #endregion

        #region HTTP 500 Internal Server Error
        /// <summary>
        /// There was an internal server error while processing the request.
        /// </summary>
        Http500_InternalServerError,
        #endregion

        #region HTTP 501 Not Implemented
        /// <summary>
        /// The requested feature is not implemented.
        /// </summary>
        Http501_NotImplemented,
        #endregion

        #region HTTP 503 Service Unavailable
        /// <summary>
        /// The service is temporarily unavailable.
        /// </summary>
        Http503_ServiceUnavailable,

        /// <summary>
        /// The service is temporarily unavailable due to maintenance.
        /// </summary>
        Http503_ServiceUnavailable_Maintenance,

        /// <summary>
        /// The service is temporarily unavailable due to overloaded resources.
        /// </summary>
        Http503_ServiceUnavailable_Overload,

        /// <summary>
        /// The service is temporarily unavailable. The request may be repeated after a delay, the length of which may be specified in a 'Retry-After' header.
        /// </summary>
        Http503_ServiceUnavailable_Retry,
        #endregion

        #region HTTP 504 Gateway Timeout
        /// <summary>
        /// An internal service or proxy was unresponsive to complete the request.
        /// </summary>
        Http504_GatewayTimeout,
        #endregion

        #region HTTP 507 Insufficient Storage
        /// <summary>
        /// The maximum storage quota has been reached.
        /// </summary>
        Http507_InsufficientStorage,
        #endregion

        #region HTTP 509 Bandwidth Limit Exceeded
        /// <summary>
        /// The client application has been throttled for exceeding the maximum bandwidth limit. The request may be repeated after a delay, the length of which may be specified in a 'Retry-After' header.
        /// </summary>
        Http509_BandwidthLimitExceeded,
        #endregion
    }
}