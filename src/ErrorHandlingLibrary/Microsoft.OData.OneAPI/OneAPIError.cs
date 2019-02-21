using System.Net;
using Microsoft.OData.Extensions.Errors;

namespace Microsoft.OData.OneAPI
{
    public class OneAPIError : Error
    {
        public bool IsTopLevel
        {
            get { return this.ErrorCode == this.InnerErrorCode && this.ErrorMessage == this.ErrorMessage; }
        }

        public HttpStatusCode HttpStatus { get; set; }

        public OneAPIError(string errorMessage, string errorCode, string innerErrorMessage, string innerErrorCode, HttpStatusCode httpStatusCode)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            InnerErrorCode = innerErrorCode;
            InnerErrorMessage = innerErrorMessage;
        }
    }
}
