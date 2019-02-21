using Microsoft.OData.Extensions.Errors;
using System;
using System.Net;

namespace Microsoft.OData.OneAPI
{
    public class OneAPIErrors
    {

        public sealed class BadRequest : OneAPIError
        {
            private BadRequest(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.BadRequest;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http400_BadRequest;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http400_BadRequest;

            public static BadRequest Base { get; } = new BadRequest(Constants.ErrorMessages.Level1.Http400_BadRequest, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
            public static BadRequest RequiredFieldOrParameterMissing { get; } = new BadRequest(Constants.ErrorMessages.Level2.Http400_BadRequest_RequiredFieldOrParameterMissing, _httpStatusCode, Constants.ErrorCodes.Level2.Http400_BadRequest_RequiredFieldOrParameterMissing, _errorCode, _errorMessage);
            public static BadRequest InvalidCombinationOfFields { get; } = new BadRequest(Constants.ErrorMessages.Level2.Http400_BadRequest_InvalidCombinationOfFields, _httpStatusCode, Constants.ErrorCodes.Level2.Http400_BadRequest_InvalidCombinationOfFields, _errorCode, _errorMessage);
            public static BadRequest InvalidBody { get; } = new BadRequest(Constants.ErrorMessages.Level2.Http400_BadRequest_InvalidBody, _httpStatusCode, Constants.ErrorCodes.Level2.Http400_BadRequest_InvalidBody, _errorCode, _errorMessage);
        }

        public sealed class Unauthorized : OneAPIError
        {
            private Unauthorized(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.Unauthorized;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http401_Unauthorized;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http401_Unauthorized;

            public static Unauthorized Base { get; } = new Unauthorized(Constants.ErrorMessages.Level1.Http401_Unauthorized, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
            public static Unauthorized MissingAuthenticationInfo { get; } = new Unauthorized(Constants.ErrorMessages.Level2.Http401_Unauthorized_MissingAuthenticationInfo, _httpStatusCode, Constants.ErrorCodes.Level2.Http401_Unauthorized_MissingAuthenticationInfo, _errorCode, _errorMessage);
            public static Unauthorized InvalidAuthenticationInfo { get; } = new Unauthorized(Constants.ErrorMessages.Level2.Http401_Unauthorized_InvalidAuthenticationInfo, _httpStatusCode, Constants.ErrorCodes.Level2.Http401_Unauthorized_InvalidAuthenticationInfo, _errorCode, _errorMessage);
            public static Unauthorized ExpiredAuthenticationToken { get; } = new Unauthorized(Constants.ErrorMessages.Level2.Http401_Unauthorized_ExpiredAuthenticationToken, _httpStatusCode, Constants.ErrorCodes.Level2.Http401_Unauthorized_ExpiredAuthenticationToken, _errorCode, _errorMessage);
        }

        #region HTTP 403 Access Denied
        public sealed class AccessDenied : OneAPIError
        {
            private AccessDenied(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.Unauthorized;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http403_AccessDenied;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http403_AccessDenied;

            public static AccessDenied Base { get; } = new AccessDenied(Constants.ErrorMessages.Level1.Http403_AccessDenied, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
            public static AccessDenied ConditionalAccess { get; } = new AccessDenied(Constants.ErrorMessages.Level2.Http403_AccessDenied_ConditionalAccess, _httpStatusCode, Constants.ErrorCodes.Level2.Http403_AccessDenied_ConditionalAccess, _errorCode, _errorMessage);
            public static AccessDenied CountryForbidden { get; } = new AccessDenied(Constants.ErrorMessages.Level2.Http403_AccessDenied_CountryForbidden, _httpStatusCode, Constants.ErrorCodes.Level2.Http403_AccessDenied_CountryForbidden, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 404 Resource Not Found
        public sealed class ResouceNotFound : OneAPIError
        {
            private ResouceNotFound(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.NotFound;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http404_ResourceNotFound;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http404_ResourceNotFound;

            public static ResouceNotFound Base { get; } = new ResouceNotFound(Constants.ErrorMessages.Level1.Http404_ResourceNotFound, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 405 Method Not Allowed
        public sealed class MethodNotAllowed : OneAPIError
        {
            private MethodNotAllowed(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.NotFound;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http405_MethodNotAllowed;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http405_MethodNotAllowed;

            public static MethodNotAllowed Base { get; } = new MethodNotAllowed(Constants.ErrorMessages.Level1.Http405_MethodNotAllowed, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 406 Format Not Acceptable
        public sealed class NotAcceptable : OneAPIError
        {
            private NotAcceptable(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.NotAcceptable;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http406_FormatNotAcceptable;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http406_FormatNotAcceptable;

            public static NotAcceptable Base { get; } = new NotAcceptable(Constants.ErrorMessages.Level1.Http406_FormatNotAcceptable, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 409 Conflict
        public sealed class Conflict : OneAPIError
        {
            private Conflict(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.Conflict;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http409_Conflict;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http409_Conflict;

            public static Conflict Base { get; } = new Conflict(Constants.ErrorMessages.Level1.Http406_FormatNotAcceptable, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
            public static Conflict ConcurrencyIssue { get; } = new Conflict(Constants.ErrorMessages.Level2.Http409_Conflict_ConcurrencyIssue, _httpStatusCode, Constants.ErrorCodes.Level2.Http409_Conflict_ConcurrencyIssue, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 410 ResourceGone
        public sealed class ResourceGone : OneAPIError
        {
            private ResourceGone(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.Gone;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http410_ResourceGone;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http410_ResourceGone;

            public static ResourceGone Base { get; } = new ResourceGone(Constants.ErrorMessages.Level1.Http410_ResourceGone, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 411 Content Length Required
        public sealed class LengthRequired : OneAPIError
        {
            private LengthRequired(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.LengthRequired;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http411_ContentLengthRequired;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http411_ContentLengthRequired;

            public static LengthRequired Base { get; } = new LengthRequired(Constants.ErrorMessages.Level1.Http411_ContentLengthRequired, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 412 PreconditionFailed
        public sealed class PreconditionFailed : OneAPIError
        {
            private PreconditionFailed(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.PreconditionFailed;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http412_PreconditionFailed;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http412_PreconditionFailed;

            public static PreconditionFailed Base { get; } = new PreconditionFailed(Constants.ErrorMessages.Level1.Http412_PreconditionFailed, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
            public static PreconditionFailed ResourceModified { get; } = new PreconditionFailed(Constants.ErrorMessages.Level2.Http412_PreconditionFailed_ResourceModified, _httpStatusCode, Constants.ErrorCodes.Level2.Http412_PreconditionFailed_ResourceModified, _errorCode, _errorMessage);
            public static PreconditionFailed ResyncRequired { get; } = new PreconditionFailed(Constants.ErrorMessages.Level2.Http412_PreconditionFailed_ResyncRequired, _httpStatusCode, Constants.ErrorCodes.Level2.Http412_PreconditionFailed_ResyncRequired, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 413 Request Size Exceeded
        public sealed class RequestEntityTooLarge : OneAPIError
        {
            private RequestEntityTooLarge(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.RequestEntityTooLarge;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http413_RequestSizeExceeded;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http413_RequestSizeExceeded;

            public static RequestEntityTooLarge Base { get; } = new RequestEntityTooLarge(Constants.ErrorMessages.Level1.Http413_RequestSizeExceeded, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 415 Request Size Exceeded
        public sealed class UnsupportedMediaType : OneAPIError
        {
            private UnsupportedMediaType(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.UnsupportedMediaType;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http415_ContentTypeNotSupported;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http415_ContentTypeNotSupported;

            public static UnsupportedMediaType Base { get; } = new UnsupportedMediaType(Constants.ErrorMessages.Level1.Http415_ContentTypeNotSupported, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 416 Invalid Range
        public sealed class InvalidRange : OneAPIError
        {
            private InvalidRange(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.RequestedRangeNotSatisfiable;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http416_InvalidRange;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http416_InvalidRange;

            public static InvalidRange Base { get; } = new InvalidRange(Constants.ErrorMessages.Level1.Http415_ContentTypeNotSupported, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 422 Unprocessable Entity
        public sealed class UnprocessableEntity : OneAPIError
        {
            private UnprocessableEntity(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = (HttpStatusCode)422;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http422_UnprocessableEntity;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http422_UnprocessableEntity;

            public static UnprocessableEntity Base { get; } = new UnprocessableEntity(Constants.ErrorMessages.Level1.Http422_UnprocessableEntity, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
            public static UnprocessableEntity InvalidOrUnAcceptedUpload { get; } = new UnprocessableEntity(Constants.ErrorMessages.Level2.Http422_UnprocessableEntity_InvalidOrUnacceptedUpload, _httpStatusCode, Constants.ErrorCodes.Level2.Http422_UnprocessableEntity_InvalidOrUnacceptedUpload, _errorCode, _errorMessage);
        }
        #endregion

        #region HTTP 500 Internal Server Error
        public sealed class InternalServerError : OneAPIError
        {
            private InternalServerError(string errorMessage, HttpStatusCode httpStatusCode, string errorCode, string topLevelErrorCode, string topLevelErrorMessage)
                : base(topLevelErrorMessage, topLevelErrorCode, errorMessage, errorCode, httpStatusCode)
            {

            }

            private const HttpStatusCode _httpStatusCode = HttpStatusCode.InternalServerError;
            private const string _errorCode = Constants.ErrorCodes.Level1.Http500_InternalServerError;
            private const string _errorMessage = Constants.ErrorMessages.Level1.Http500_InternalServerError;

            public static InternalServerError Base { get; } = new InternalServerError(Constants.ErrorMessages.Level1.Http500_InternalServerError, _httpStatusCode, _errorCode, _errorCode, _errorMessage);
        }
        #endregion
    }

}

