using Microsoft.OData.Extensions.Errors;

namespace Microsoft.OData.OneAPI
{
    public class OneAPIErrorFactory : ODataErrorFactory
    {

        /// <summary>
        /// Creates an <see cref="T:Microsoft.OData.ODataError" /> object with populated properties.
        /// This factory sets the hierarchy of error codes and messages to drive consistency across
        /// errors. See https://developer.microsoft.com/en-us/graph/docs/concepts/errors
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
        public new static ODataError Create(Error error, ODataInnerError odataInnerError)
        {
            //This factory should serve only OneAPIErrors.

            OneAPIError oneApiError = error as OneAPIError;
            if (oneApiError == null)
            {
                return null;
            }

            ODataInnerError innerError = HandleInnerError(error, odataInnerError);

            ODataError resultingError = new ODataError
            {
                ErrorCode = error.ErrorCode,
                Message = error.ErrorMessage,
                InnerError = innerError
            };

            return resultingError;
        }

        /// <summary>
        /// Creates a chain of <see cref="T:Microsoft.OData.ODataInnerError" /> based on the input and appends the workload's inner error at the end.
        /// </summary>
        /// <returns>An <see cref="T:Microsoft.OData.ODataInnerError" /> object nested inner errors.</returns>
        /// <param name="innerErrorsChain">Chain of error strings to use.</param>
        /// <param name="workloadInnerError">Workload's inner error. This may be null.</param>
        internal static ODataInnerError HandleInnerError(Error mainInnerError, ODataInnerError workloadInnerError)
        {
            OneAPIError error = mainInnerError as OneAPIError;
            if (error == null)
            {
                return null;
            }

            ODataInnerError chainResult = null;

            if (!error.IsTopLevel)
            {
                chainResult = new ODataInnerError()
                {
                    Message = error.InnerErrorMessage
                };
            }

            // Append the workload's inner error at the end
            if (chainResult != null)
            {
                chainResult.InnerError = workloadInnerError;
            }
            else
            {
                chainResult = workloadInnerError;
            }

            return chainResult;
        }
    }
}
