// <copyright file="ODataErrorFactoryTests.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.UnitTests
{
    using FluentAssertions;
    using Microsoft.OData;
    using Microsoft.OData.OneAPI;
    using Xunit;

    public class ODataErrorFactoryTests
    {
        [Fact]
        public void ODataErrorFactory_CreateWithAllArgumentsSet_ShouldSuccessfullyReturnODataError()
        {
            // Setup
            const string innerErrorTypeName = "InsufficientPermissionsException";
            const string innerErrorMessage = "User does not have read permissions.";
            const string innerErrorStackTrace = "InsufficientPermissionsException at SomeNamespace.SomeClass.SomeFunction:SomeLine";

            ODataInnerError innerError = new ODataInnerError
            {
                TypeName = innerErrorTypeName,
                Message = innerErrorMessage,
                StackTrace = innerErrorStackTrace
            };

            ODataError odataError = OneAPIErrorFactory.Create(OneAPIErrors.AccessDenied.Base, innerError);

            // Assert
            odataError.Should().NotBeNull();
            odataError.ErrorCode.Should().Be(Constants.ErrorCodes.Level1.Http403_AccessDenied);
            odataError.Message.Should().Be(Constants.ErrorMessages.Level1.Http403_AccessDenied);

            odataError.InnerError.Should().Be(innerError);
        }

        [Fact]
        public void ODataErrorFactory_CreateWithLevel2InnerErrorSet_ShouldSuccessfullyReturnODataError()
        {
            // Setup
            const string innerErrorTypeName = "InsufficientPermissionsException";
            const string innerErrorMessage = "User does not have read permissions.";
            const string innerErrorStackTrace = "InsufficientPermissionsException at SomeNamespace.SomeClass.SomeFunction:SomeLine";

            ODataInnerError innerError = new ODataInnerError
            {
                TypeName = innerErrorTypeName,
                Message = innerErrorMessage,
                StackTrace = innerErrorStackTrace
            };

            // Run
            ODataError odataError = OneAPIErrorFactory.Create(OneAPIErrors.BadRequest.InvalidBody, innerError);

            // Assert
            odataError.Should().NotBeNull();
            odataError.ErrorCode.Should().Be(Constants.ErrorCodes.Level1.Http400_BadRequest);
            odataError.Message.Should().Be(Constants.ErrorMessages.Level1.Http400_BadRequest);

            ODataInnerError odataInnerError = odataError.InnerError;
            odataInnerError.Should().NotBeNull();
            odataInnerError.Message.Should().Be(Constants.ErrorMessages.Level2.Http400_BadRequest_InvalidBody);

            odataInnerError = odataInnerError.InnerError;
            odataInnerError.Should().Be(innerError);
        }

        [Fact]
        public void ODataErrorFactory_CreateWithoutInnerErrorArgument_ShouldSuccessfullyReturnODataError()
        {
            // Run
            //IODataErrorFactory errorFactory = new ODataErrorFactory();
            ODataError odataError = OneAPIErrorFactory.Create(OneAPIErrors.BadRequest.Base, null);

            // Assert
            odataError.Should().NotBeNull();
            odataError.ErrorCode.Should().Be(Constants.ErrorCodes.Level1.Http400_BadRequest);
            odataError.Message.Should().Be(Constants.ErrorMessages.Level1.Http400_BadRequest);

            odataError.InnerError.Should().BeNull();
        }
    }
}
