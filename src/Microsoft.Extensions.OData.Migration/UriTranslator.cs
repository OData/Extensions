using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.OData.Migration
{
    /// <summary>
    /// UriTranslator contains logic to translate every kind of V3 path segment into
    /// its corresponding V4 counterpart path segment.
    /// </summary>
    public class UriTranslator : Data.OData.Query.SemanticAst.PathSegmentTranslator<ODataPathSegment>
    {
        private readonly IEdmModel v4model;

        /// <summary>
        /// Initialize UriTranslator
        /// </summary>
        /// <param name="v4model">v4 model is used to look up corresponding entities/etc. from v3 to create v4 segments</param>
        public UriTranslator(IEdmModel v4model)
        {
            this.v4model = v4model;
        }

        /// <summary>
        /// Translate a v3 TypeSegment (TODO: EXAMPLE) into v4 TypeSegment
        /// </summary>
        /// <param name="segment">OData V3 TypeSegment</param>
        /// <returns>OData V4 TypeSegment</returns>        
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.TypeSegment segment)
        {
            // Look up v4 type by name of v3 type.
            IEdmType v4type = v4model.FindType(segment.EdmType.FullTypeName());
            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            return new TypeSegment(v4type, v4navigationSource);
        }

        /// <summary>
        /// Translates a v3 NavigationPropertySegment (TODO: EXAMPLE) into v4 NavigationPropertySegment
        /// </summary>
        /// <param name="segment">OData V3 NavigationPropertySegment</param>
        /// <returns>OData V4 NavigationPropertySegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.NavigationPropertySegment segment)
        {
            // Obtain the V3 Navigation Property's declared type
            Data.Edm.IEdmEntityType v3Type = Data.Edm.ExtensionMethods.DeclaringEntityType(segment.NavigationProperty);

            // Extract declared type as string
            string v3StructuredTypeName = v3Type.FullTypeName();

            // Look up corresponding V4 type in model (guaranteed to be structured, if found)
            IEdmStructuredType v4Type = v4model.FindType(v3StructuredTypeName) as IEdmStructuredType;

            // Use corresponding V4 type to look up corresponding NavigationProperty
            IEdmNavigationProperty v4navigationProperty = v4Type.FindProperty(segment.NavigationProperty.Name) as IEdmNavigationProperty;
               
            // Separately, look up navigation source (entity set)
            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);

            return new NavigationPropertySegment(v4navigationProperty, v4navigationSource);
        }

        /// <summary>
        /// Translates a V3 EntitySetSegment (e.g. "/Boxes") to a V4 EntitySetSegment
        /// </summary>
        /// <param name="segment">OData V3 EntitySetSegment</param>
        /// <returns>OData V4 EntitySetSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.EntitySetSegment segment)
        {
            IEdmEntitySet v4entitySet = v4model.FindDeclaredEntitySet(segment.EntitySet.Name);
            EntitySetSegment v4segment = new EntitySetSegment(v4entitySet);
            return v4segment;
        }

        /// <summary>
        /// Translates a V3 KeySegment (e.g. "/Boxes(1)") to a V4 KeySegment
        /// </summary>
        /// <param name="segment">OData V3 KeySegment</param>
        /// <returns>OData V4 KeySegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.KeySegment segment)
        {
            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            IEdmEntityType v4type = (IEdmEntityType)v4model.FindType(segment.EdmType.FullTypeName());
            KeySegment v4segment = new KeySegment(segment.Keys, v4type, v4navigationSource);
            return v4segment;
        }

        /// <summary>
        /// Translates at V3 PropertySegment (e.g. "/Boxes(1)/Name") into a V4 PropertySegment
        /// </summary>
        /// <param name="segment">OData V3 PropertySegment</param>
        /// <returns>OData V4 PropertySegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.PropertySegment segment)
        {
            // Extract the DeclaringType (e.g. EntitySet) name from V3 PropertySegment
            string v3typeName = segment.Property.DeclaringType.FullTypeName();

            // Look up V4 Structured Type (guaranteed if found) in V4 model
            IEdmStructuredType v4Type = v4model.FindType(v3typeName) as IEdmStructuredType;

            // Look up corresponding Property in V4 Type by name
            IEdmStructuralProperty v4Property = v4Type.FindProperty(segment.Property.Name) as IEdmStructuralProperty;

            return new PropertySegment(v4Property);
        }

        /// <summary>
        /// Translates a V3 OperationSegment to V4 OperationSegment by locating all matching function imports in the v4 model
        /// and constructing a new V4 OperationSegment with all found operation segments.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.OperationSegment segment)
        {
            List<IEdmOperationImport> v4OpImports = new List<IEdmOperationImport>();
            List<IEdmOperation> v4UnboundOps = new List<IEdmOperation>();
            List<IEdmOperation> v4BoundOps = new List<IEdmOperation>();

            IEdmEntitySet v4entitySet = segment.EntitySet != null ? v4model.FindDeclaredEntitySet(segment.EntitySet.Name) : null;

            // For each operation in V3 operation segment, determine corresponding action/function/function imports in v4
            foreach (Data.Edm.IEdmFunctionImport op in segment.Operations)
            {
                IEnumerable<Data.Edm.IEdmFunctionParameter> entityParams = op.Parameters.Where(p => p.Type.Definition.GetType().GetInterfaces().Select(iface => iface.Name).Contains("IEdmEntityType"));
                foreach (var p in entityParams)
                {
                    // Find bounded operations
                    IEnumerable<IEdmOperation> boundOps = v4model.FindBoundOperations(v4model.FindType(p.Type.Definition.FullTypeName()));
                    v4BoundOps.AddRange(boundOps.Where(boundOp => boundOp.FullName() == op.Container.Namespace + "." + op.Name));
                }

                // Should I be searching also by container namespace and name?
                string searchQuery = op.Name;

                // Find v4 operation imports
                IEnumerable<IEdmOperationImport> foundOperationImports = v4model.FindDeclaredOperationImports(searchQuery);
                v4OpImports.AddRange(foundOperationImports);

                // Find v4 unbounded operations
                IEnumerable<IEdmOperation> foundOperations = v4model.FindOperations(searchQuery);
                v4UnboundOps.AddRange(foundOperations);
            }

            // If function imports are found, return OperationImportSegment
            if (v4OpImports.Count() > 0)
            {
                return new OperationImportSegment(v4OpImports.First(), v4entitySet);
            }
            // Otherwise some other function has been found, so return OperationSegment
            else
            {
                v4BoundOps.AddRange(v4UnboundOps);
                return new OperationSegment(v4BoundOps.First(), v4entitySet);
            }
        }

        /// <summary>
        /// Translates a V3 OpenPropertySegment into its V4 equivalent: a DynamicPathSegment.
        /// </summary>
        /// <param name="segment">V3 OpenPropertySegment</param>
        /// <returns>V4 DynamicPathSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.OpenPropertySegment segment)
        {
            // Doesn't include type, but since these will be built into strings anyway it doesn't seem to matter.
            return new DynamicPathSegment(segment.PropertyName);
        }

        /// <summary>
        /// Translates V3 CountSegment to V4 CountSegment by simply returning the V4 CountSegment instance,
        /// which is what is needed in all cases.
        /// </summary>
        /// <param name="segment">OData V3 CountSegment</param>
        /// <returns>OData V4 CountSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.CountSegment segment)
        {
            return CountSegment.Instance;
        }

        /// <summary>
        /// Translates a v3 NavigationPropertyLinkSegment (TODO: EXAMPLE) into v4 NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">OData V3 NavigationPropertyLinkSegment</param>
        /// <returns>OData V4 NavigationPropertyLinkSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.NavigationPropertyLinkSegment segment)
        {
            // Conduct same process as Data.OData.Query.SemanticAst.NavigationProperty segment translation
            Data.Edm.IEdmEntityType v3Type = Data.Edm.ExtensionMethods.DeclaringEntityType(segment.NavigationProperty);
            string v3StructuredTypeName = v3Type.FullTypeName();
            IEdmStructuredType v4Type = v4model.FindType(v3StructuredTypeName) as IEdmStructuredType;
            IEdmNavigationProperty v4navigationProperty = v4Type.FindProperty(segment.NavigationProperty.Name) as IEdmNavigationProperty;
            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            return new NavigationPropertyLinkSegment(v4navigationProperty, v4navigationSource);
        }

        /// <summary>
        /// Translates a v3 ValueSegment (e.g. /Boxes(1)/Name/$value) into a v4 ValueSegment
        /// </summary>
        /// <param name="segment">OData V3 ValueSegment</param>
        /// <returns>OData V4 ValueSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.ValueSegment segment)
        {
            string v3typeName = segment.EdmType.FullTypeName();
            IEdmType v4prevType = v4model.FindType(v3typeName);
            return new ValueSegment(v4prevType);
        }

        /// <summary>
        /// Translates V3 BatchSegment to V4 BatchSegment by simply returning the V4 BatchSegment instance,
        /// which is what is needed in all cases.
        /// </summary>
        /// <param name="segment">OData V3 BatchSegment</param>
        /// <returns>OData V4 BatchSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.BatchSegment segment)
        {
            return BatchSegment.Instance;
        }

        /// <summary>
        /// Translates a V3 BatchReferenceSegment to V4 BatchReferenceSegment.
        /// </summary>
        /// <param name="segment">OData V3 BatchReferenceSegment</param>
        /// <returns>OData V4 BatchReferenceSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.BatchReferenceSegment segment)
        {
            IEdmEntitySet v4entitySet = v4model.FindDeclaredEntitySet(segment.EntitySet.Name);
            IEdmType v4type = v4model.FindType(segment.EdmType.FullTypeName());
            return new BatchReferenceSegment(segment.ContentId, v4type, v4entitySet);
        }

        /// <summary>
        /// Translates V3 MetadataSegment to V4 MetadataSegment by simply returning the V4 MetadataSegment instance,
        /// which is what is needed in all cases.
        /// </summary>
        /// <param name="segment">OData V3 MetadataSegment</param>
        /// <returns>OData V4 MetadataSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.MetadataSegment segment)
        {
            return MetadataSegment.Instance;
        }

    }
}
