// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Formatters.Deserialization
{
    using Microsoft.OData;
    using System.IO;
    using System.Reflection;
    internal static class DeserializationExtensions
    {
        /// <summary>
        /// Replace the inner HTTP request stream with substituteStream using reflection
        /// </summary>
        /// <param name="reader">ODataMessageReader which has not read yet</param>
        /// <param name="substituteStream">Replacement stream</param>
        public static void SubstituteRequestStream(this ODataMessageReader reader, Stream substituteStream)
        {
            FieldInfo messageField = reader.GetType().GetField("message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object message = messageField.GetValue(reader);
            FieldInfo requestMessageField = message.GetType().GetField("requestMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object requestMessage = requestMessageField.GetValue(message);
            FieldInfo streamField = requestMessage.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            streamField.SetValue(requestMessage, substituteStream);
        }
    }
}
