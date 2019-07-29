using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.OData.Migration
{
    internal static class SerializationExtensions
    {
        /// <summary>
        /// Replace the inner HTTP response stream with substituteStream using reflection
        /// </summary>
        /// <param name="writer">ODataMessageWriter which has not written yet</param>
        /// <param name="substituteStream">Replacement stream</param>
        public static Stream SubstituteResponseStream(this ODataMessageWriter writer, Stream substituteStream)
        {
            FieldInfo messageField = writer.GetType().GetField("message", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object message = messageField.GetValue(writer);
            FieldInfo requestMessageField = message.GetType().GetField("responseMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            object requestMessage = requestMessageField.GetValue(message);
            FieldInfo streamField = requestMessage.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            Stream originalStream = (Stream)streamField.GetValue(requestMessage);
            streamField.SetValue(requestMessage, substituteStream);
            return originalStream;
        }

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
