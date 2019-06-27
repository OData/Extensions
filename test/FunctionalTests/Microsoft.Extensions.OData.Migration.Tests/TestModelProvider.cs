// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Xsl;
    internal class TestModelProvider
    {

        /// <summary>
        /// Provides a middleware between the V3 and V4 versions of the official V3 OData svc model
        /// </summary>
        /// <returns>TranslationMiddleware for testing</returns>
        internal static TranslationMiddleware ODataSvcSampleMiddleware(Uri serviceRoot)
        {
            var v3model = LoadV3ODataSvcModel();
            var v4model = LoadV4ODataSvcModel();
            return new TranslationMiddleware(v3model, v4model, new ODataMigrationOptions()
            {
                ServiceRoot = serviceRoot
            });
        }

        /// <summary>
        /// Load V3 OData.Svc model (metadata found here: https://services.odata.org/V3/OData/OData.svc/$metadata)
        /// </summary>
        /// <returns>V3 OData.Svc model</returns>
        private static Data.Edm.IEdmModel LoadV3ODataSvcModel ()
        {
            return LoadTestV3Model("V3ODataSvc.edmx");
        }

        /// <summary>
        /// Load V4 OData.Svc model equivalent (V3 metadata found here: https://services.odata.org/V3/OData/OData.svc/$metadata)
        /// </summary>
        /// <returns>V4 OData.Svc model</returns>
        private static Microsoft.OData.Edm.IEdmModel LoadV4ODataSvcModel ()
        {
            string v4edmx = TransformODataV3EdmxToODataV4Edmx("V3ODataSvc.edmx", "V2-to-V4-CSDL-NoSap.xsl");
            return LoadTestV4Model(v4edmx);
        }
        /// <summary>
        /// Creates a v3 model from v3 edmx file
        /// </summary>
        /// <param name="modelPath">path to filename</param>
        /// <returns>V3 model representation</returns>
        private static Data.Edm.IEdmModel LoadTestV3Model(string modelPath)
        {
            Data.Edm.IEdmModel model;
            using (XmlReader reader = XmlReader.Create(modelPath))
            {
                model = Data.Edm.Csdl.EdmxReader.Parse(reader);
            }
            return model;
        }

        /// <summary>
        /// Creates v4 model from v4edmx string
        /// </summary>
        /// <param name="v4Edmx">v4 edmx string representation</param>
        /// <returns>V4 model</returns>
        private static Microsoft.OData.Edm.IEdmModel LoadTestV4Model (string v4Edmx)
        {
            Microsoft.OData.Edm.IEdmModel model;
            using (StringReader stringReader = new StringReader(v4Edmx))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    model = Microsoft.OData.Edm.Csdl.CsdlReader.Parse(reader);
                }
            }
            return model;
        }

        /// <summary>
        /// Uses xsl transform stylesheet to convert a V3 edmx to V4 edmx string.
        /// </summary>
        /// <param name="v3EdmxPath">v3 edmx file path</param>
        /// <param name="xslTransformPath">file location of XSL transform stylesheet</param>
        /// <returns>V4 edmx equivalent of V3 edmx</returns>
        private static string TransformODataV3EdmxToODataV4Edmx(string v3EdmxPath, string xslTransformPath)
        {
            string v3Edmx = File.ReadAllText(v3EdmxPath);

            StringBuilder v4EdmxStringBuilder = new StringBuilder();

            using (XmlReader stylesheetReader = XmlReader.Create(xslTransformPath))
            {
                XslCompiledTransform xmlCompiledTransform = new XslCompiledTransform();
                xmlCompiledTransform.Load(stylesheetReader);

                using (StringReader stringReader = new StringReader(v3Edmx))
                {
                    using (XmlReader xmlReader = XmlReader.Create(stringReader))
                    {
                        using (XmlWriter xmlWriter = XmlWriter.Create(v4EdmxStringBuilder, xmlCompiledTransform.OutputSettings))
                        {
                            xmlCompiledTransform.Transform(xmlReader, xmlWriter);
                        }
                   }
                }
            }
            return v4EdmxStringBuilder.ToString();
        }
    }
}
