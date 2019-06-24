using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Microsoft.Extensions.OData.Migration.Tests
{
    internal class SampleModelBuilder
    {
        /// <summary>
        /// Creates a v3 model from v3 edmx file
        /// </summary>
        /// <param name="modelPath">path to filename</param>
        /// <returns>V3 model representation</returns>
        internal static Data.Edm.IEdmModel LoadTestv3Model(string modelPath)
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
        internal static Microsoft.OData.Edm.IEdmModel LoadTestv4Model (string v4Edmx)
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

        /* Custom built models - probably not necessary since online sample models are being used now
        internal static Data.Edm.Library.EdmModel BuildTestv3EdmModel()
        {
            Data.Edm.Library.EdmModel model = new Data.Edm.Library.EdmModel();

            // Add complex type Address with strings Country and City
            Data.Edm.Library.EdmComplexType address = new Data.Edm.Library.EdmComplexType("Sample.NS", "Address");
            address.AddStructuralProperty("Country", Data.Edm.EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", Data.Edm.EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // Add derived complex type SubAddress with string Street
            Data.Edm.Library.EdmComplexType subAddress = new Data.Edm.Library.EdmComplexType("Sample.NS", "SubAddress", address, false);
            address.AddStructuralProperty("Street", Data.Edm.EdmPrimitiveTypeKind.String);
            model.AddElement(subAddress);

            // Add entity set "Bin" with Guid Token
            Data.Edm.Library.EdmEntityType binType = new Data.Edm.Library.EdmEntityType("Sample.NS", "Bin");
            binType.AddKeys(binType.AddStructuralProperty("Token", Data.Edm.EdmPrimitiveTypeKind.Guid, isNullable: false));
            model.AddElement(binType);

            // Add entity set "Box" with Id and string name and Address location
            Data.Edm.Library.EdmEntityType boxType = new Data.Edm.Library.EdmEntityType("Sample.NS", "Box");
            boxType.AddKeys(boxType.AddStructuralProperty("Id", Data.Edm.EdmPrimitiveTypeKind.Int32, isNullable: false));
            boxType.AddStructuralProperty("Name", Data.Edm.EdmPrimitiveTypeKind.String, isNullable: false);
            boxType.AddStructuralProperty("Location", new Data.Edm.Library.EdmComplexTypeReference(address, isNullable: true));
            boxType.AddStructuralProperty("DerivedLocation", new Data.Edm.Library.EdmComplexTypeReference(subAddress, isNullable: true));
            Data.Edm.Library.EdmNavigationProperty binsNavProp = boxType.AddUnidirectionalNavigation(
                new Data.Edm.Library.EdmNavigationPropertyInfo()
                {
                    Name = "Bins",
                    Target = binType,
                    TargetMultiplicity = Data.Edm.EdmMultiplicity.Many // Note how this Bins doesn't conflict with EntitySet Bins

                });
            Data.Edm.Library.EdmNavigationProperty binNavProp = boxType.AddUnidirectionalNavigation(
                new Data.Edm.Library.EdmNavigationPropertyInfo()
                {
                    Name = "Bin",
                    Target = binType,
                    TargetMultiplicity = Data.Edm.EdmMultiplicity.One // Note how this Bin doesn't conflict other NavigationProperty Bins
                });
            model.AddElement(boxType);

            // Add Bound function
            Data.Edm.IEdmTypeReference stringType = Data.Edm.Library.EdmCoreModel.Instance.GetPrimitive(Data.Edm.EdmPrimitiveTypeKind.String, isNullable: false);
            Data.Edm.Library.EdmFunction doBoundFn = new Data.Edm.Library.EdmFunction("Sample.NS", "DoBoundFn", stringType); 
            doBoundFn.AddParameter("box", new Data.Edm.Library.EdmEntityTypeReference(boxType, false));
            model.AddElement(doBoundFn);
            

            // Add Unbound function
            Data.Edm.IEdmTypeReference intType = Data.Edm.Library.EdmCoreModel.Instance.GetPrimitive(Data.Edm.EdmPrimitiveTypeKind.Int32, isNullable: false);
            Data.Edm.Library.EdmFunction doUnboundFn = new Data.Edm.Library.EdmFunction("Sample.NS", "DoUnboundFn", intType);
            model.AddElement(doUnboundFn);

            // Create container and add entity sets;
            Data.Edm.Library.EdmEntityContainer boxContainer = new Data.Edm.Library.EdmEntityContainer("Sample.NS", "BoxContainer");
            Data.Edm.Library.EdmEntitySet boxSet = boxContainer.AddEntitySet("Boxes", // Note how this Bins doesn't conflict with EntitySet Bins
boxType);
            Data.Edm.Library.EdmEntitySet binSet = boxContainer.AddEntitySet("Bins", binType);
            model.AddElement(boxContainer);

            // Add Bound action (TODO functionimport?)  How to add Actions?
            Data.Edm.Library.EdmFunctionImport doBoundAction = boxContainer.AddFunctionImport("DoBoundAction", returnType: null);
            doBoundAction.AddParameter("Id", new Data.Edm.Library.EdmEntityTypeReference(boxType, false));

            // Add Unbound action
            Data.Edm.Library.EdmFunctionImport doUnboundAction = boxContainer.AddFunctionImport("DoUnboundAction", returnType: null);
            doUnboundAction.AddParameter("Id", intType);

            // Add Navigation Targets
            boxSet.AddNavigationTarget(binsNavProp, binSet);
            boxSet.AddNavigationTarget(binNavProp, binSet);

            // Add Function imports
            boxContainer.AddFunctionImport("DoUnboundFn", intType);
            boxContainer.AddFunctionImport("DoBoundFn", stringType);

            return model;
        }

        internal static Microsoft.OData.Edm.EdmModel BuildTestv4EdmModel()
        {
            Microsoft.OData.Edm.EdmModel model = new Microsoft.OData.Edm.EdmModel();
            // Add complex type Address with strings Country and City
            Microsoft.OData.Edm.EdmComplexType address = new Microsoft.OData.Edm.EdmComplexType("Sample.NS", "Address");
            address.AddStructuralProperty("Country", Microsoft.OData.Edm.EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", Microsoft.OData.Edm.EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // Add derived complex type SubAddress with string Street
            Microsoft.OData.Edm.EdmComplexType subAddress = new Microsoft.OData.Edm.EdmComplexType("Sample.NS", "SubAddress", address, false);
            address.AddStructuralProperty("Street", Microsoft.OData.Edm.EdmPrimitiveTypeKind.String);
            model.AddElement(subAddress);

            // Add entity set "Bin" with Guid Token
            Microsoft.OData.Edm.EdmEntityType binType = new Microsoft.OData.Edm.EdmEntityType("Sample.NS", "Bin");
            binType.AddKeys(binType.AddStructuralProperty("Token", Microsoft.OData.Edm.EdmPrimitiveTypeKind.Guid, isNullable: false));
            model.AddElement(binType);

            // Add entity set "Box" with Id and string name
            Microsoft.OData.Edm.EdmEntityType boxType = new Microsoft.OData.Edm.EdmEntityType("Sample.NS", "Box");
            boxType.AddKeys(boxType.AddStructuralProperty("Id", Microsoft.OData.Edm.EdmPrimitiveTypeKind.Int32, isNullable: false));
            boxType.AddStructuralProperty("Name", Microsoft.OData.Edm.EdmPrimitiveTypeKind.String, isNullable: false);
            boxType.AddStructuralProperty("Location", new Microsoft.OData.Edm.EdmComplexTypeReference(address, isNullable: true));
            boxType.AddStructuralProperty("DerivedLocation", new Microsoft.OData.Edm.EdmComplexTypeReference(subAddress, isNullable: true));
            Microsoft.OData.Edm.EdmNavigationProperty binsNavProp = boxType.AddUnidirectionalNavigation(
                new Microsoft.OData.Edm.EdmNavigationPropertyInfo()
                {
                    Name = "Bins",
                    Target = binType,
                    TargetMultiplicity = Microsoft.OData.Edm.EdmMultiplicity.Many // Note how this Bins doesn't conflict with EntitySet Bins
                });
            Microsoft.OData.Edm.EdmNavigationProperty binNavProp = boxType.AddUnidirectionalNavigation(
                new Microsoft.OData.Edm.EdmNavigationPropertyInfo()
                {
                    Name = "Bin",
                    Target = binType,
                    TargetMultiplicity = Microsoft.OData.Edm.EdmMultiplicity.One // Note how this Bin doesn't conflict with other NavigationProperty Bins
                });
            model.AddElement(boxType);

            // Add Bound function
            Microsoft.OData.Edm.IEdmTypeReference stringType = Microsoft.OData.Edm.EdmCoreModel.Instance.GetPrimitive(Microsoft.OData.Edm.EdmPrimitiveTypeKind.String, isNullable: false);
            Microsoft.OData.Edm.EdmFunction doBoundFn = new Microsoft.OData.Edm.EdmFunction("Sample.NS", "DoBoundFn", stringType, isBound: true, entitySetPathExpression: null, isComposable: false);
            doBoundFn.AddParameter("entity", new Microsoft.OData.Edm.EdmEntityTypeReference(boxType, false));
            model.AddElement(doBoundFn);

            //Add Unbound function
            Microsoft.OData.Edm.IEdmTypeReference intType = Microsoft.OData.Edm.EdmCoreModel.Instance.GetPrimitive(Microsoft.OData.Edm.EdmPrimitiveTypeKind.Int32, isNullable: false);
            Microsoft.OData.Edm.EdmFunction doUnboundFn = new Microsoft.OData.Edm.EdmFunction("Sample.NS", "DoUnboundFn", intType, isBound: false, entitySetPathExpression: null, isComposable: false);
            model.AddElement(doUnboundFn);

            // Create container and add entity sets and singleton for testing
            Microsoft.OData.Edm.EdmEntityContainer boxContainer = new Microsoft.OData.Edm.EdmEntityContainer("Sample.NS", "BoxContainer");
            Microsoft.OData.Edm.EdmEntitySet boxSet = boxContainer.AddEntitySet("Boxes", boxType);
            Microsoft.OData.Edm.EdmEntitySet binSet = boxContainer.AddEntitySet("Bins", binType);
            boxContainer.AddSingleton("SingletonBox", boxType);
            model.AddElement(boxContainer);

            // Add Bound action
            Microsoft.OData.Edm.EdmAction doBoundAction = new Microsoft.OData.Edm.EdmAction("Sample.NS", "DoBoundAction", returnType: null);
            doBoundAction.AddParameter("Id", new Microsoft.OData.Edm.EdmEntityTypeReference(boxType, false));
            model.AddElement(doBoundAction);

            // Add Unbound action
            Microsoft.OData.Edm.EdmAction doUnboundAction = new Microsoft.OData.Edm.EdmAction("Sample.NS", "DoUnboundAction", returnType: null);
            doUnboundAction.AddParameter("Id", intType);
            model.AddElement(doUnboundAction);

            // Add Navigation Targets
            boxSet.AddNavigationTarget(binsNavProp, binSet);
            boxSet.AddNavigationTarget(binNavProp, binSet);

            // Add Function Imports
            boxContainer.AddFunctionImport("DoUnboundFn", doUnboundFn);

            return model;
        }*/

        /// <summary>
        /// Sends string edmx representation of V3 model to destination filePath
        /// </summary>
        /// <param name="model">V3 model to be printed</param>
        /// <param name="filePath">Name of file to send model edmx to</param>
        public static void PrintV3Model (Data.Edm.IEdmModel model, string filePath)
        {
            using (XmlWriter writer = XmlWriter.Create(filePath))
            {
                IEnumerable<Data.Edm.Validation.EdmError> errors;
                Data.Edm.Csdl.CsdlWriter.TryWriteCsdl(model, writer, out errors);
            }
        }

        /// <summary>
        /// Uses xsl transform stylesheet to convert a V3 edmx to V4 edmx string.
        /// </summary>
        /// <param name="v3EdmxPath">v3 edmx file path</param>
        /// <param name="xslTransformPath">file location of XSL transform stylesheet</param>
        /// <returns>V4 edmx equivalent of V3 edmx</returns>
        public static string TransformODataV3EdmxToODataV4Edmx(string v3EdmxPath, string xslTransformPath)
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
