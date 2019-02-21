//---------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OneAPI.ODataSample
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Microsoft.OData;
    using Microsoft.OData.OneAPI;

    /// <summary>
    /// This sample demonstrates how to integrate the Microsoft.Workload.Errors library
    /// into your workload that integrates Microsoft.OData.Core. Note: if your service
    /// uses WebAPI OData (Microsoft.AspNet.OData or Microsoft.AspNetCore.OData), you
    /// should see the other samples.
    /// 
    /// This is a standalone console application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The code in this function belongs in the error handler of your service.
        /// </summary>
        /// <remarks>
            /*
                Output:
                {
                  "error":
                  {
                    "code":"BadRequest",
                    "message":"Cannot process the request because it is malformed or incorrect.",
                    "innererror":
                    {
                      "message":"Cannot process the request because the body could be parsed correctly.",
                      "type":"",
                      "stacktrace":"",
                      "internalexception":
                      {
                        "message":"An invalid resource has been parsed starting at position '14' in 'service/entity/Address/'.",
                        "type":"InvalidResourceException",
                        "stacktrace":"InvalidResourceException at SomeNamespace.SomeClass.SomeFunction:SomeLine"
                      }
                    },
                    "@workloadName.correlationId":"A3B6D2EE-73CB-4CE8-813C-41F4B69A1131",
                    "@workloadName.date":"1993-05-16T11:11:11"
                  }
                }
            */
        /// 
        /// 
        /// </remarks>
        
        //   static class GraphError
        //{
        //   // public Errors Http400BadRequest = privider.FindError(...);
        //}
        

        static void Main(string[] args)
        {

            // Initialize
            MemoryStream stream = new MemoryStream();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            IODataResponseMessage messageToWrite = new MyODataResponseMessage(headers, stream)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings { EnableMessageStreamDisposal = false };
            writerSettings.SetContentType(ODataFormat.Json);

            // Write the response payload
            using (var messageWriter = new ODataMessageWriter(messageToWrite, writerSettings))
            {
                ODataInnerError odataInnerError = new ODataInnerError()
                {
                    TypeName = "InvalidResourceException",
                    Message = "An invalid resource has been parsed starting at position '14' in 'service/entity/Address/'.",
                    StackTrace = "InvalidResourceException at SomeNamespace.SomeClass.SomeFunction:SomeLine"
                };

                ODataError error = OneAPIErrorFactory.Create(OneAPIErrors.BadRequest.InvalidBody, odataInnerError);

                // Add service-specific information
                error.InstanceAnnotations.Add(new ODataInstanceAnnotation("workloadName.correlationId", new ODataPrimitiveValue("A3B6D2EE-73CB-4CE8-813C-41F4B69A1131")));
                error.InstanceAnnotations.Add(new ODataInstanceAnnotation("workloadName.date", new ODataPrimitiveValue("1993-05-16T11:11:11")));

                messageWriter.WriteError(error, includeDebugInformation: true);
            }

            messageToWrite.GetStream().Position = 0;
            string payload = (new StreamReader(stream)).ReadToEnd();
            Console.WriteLine(payload);
        }
    }
}
