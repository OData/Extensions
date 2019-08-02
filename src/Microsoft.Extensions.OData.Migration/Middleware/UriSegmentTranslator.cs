// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// UriSegmentTranslator contains logic to translate every kind of V3 path segment into
    /// its corresponding V4 counterpart path segment.
    /// </summary>
    internal class UriSegmentTranslator : Data.OData.Query.SemanticAst.PathSegmentTranslator<ODataPathSegment>
    {
        private readonly IEdmModel v4model;

        /// <summary>
        /// Initialize UriSegmentTranslator
        /// </summary>
        /// <param name="v4model">v4 model is used to look up corresponding entities/etc. from v3 to create v4 segments</param>
        public UriSegmentTranslator(IEdmModel v4model)
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
            IEdmType v4Type = v4model.FindType(segment.EdmType.GetFullTypeName());
            ExceptionUtil.IfNullThrowException(v4Type, "Unable to locate equivalent v4 type for " + segment.EdmType.GetFullTypeName());

            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            ExceptionUtil.IfNullThrowException(v4navigationSource, "Unable to locate equivalent v4 entity set for: " + segment.EntitySet.Name);

            return new TypeSegment(v4Type, v4navigationSource);
        }

        /// <summary>
        /// Translates a v3 NavigationPropertySegment (TODO: EXAMPLE) into v4 NavigationPropertySegment
        /// </summary>
        /// <param name="segment">OData V3 NavigationPropertySegment</param>
        /// <returns>OData V4 NavigationPropertySegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.NavigationPropertySegment segment)
        {
            // Look up corresponding V4 type in model
            IEdmStructuredType v4Type = v4model.GetV4Definition(segment.NavigationProperty.DeclaringType) as IEdmStructuredType;
            ExceptionUtil.IfNullThrowException(v4Type, "Unable to locate equivalent v4 type for declaring type: " + segment.NavigationProperty.DeclaringType.GetFullTypeName());

            // Use corresponding V4 type to look up corresponding NavigationProperty
            IEdmNavigationProperty v4navigationProperty = v4Type.FindProperty(segment.NavigationProperty.Name) as IEdmNavigationProperty;
            ExceptionUtil.IfNullThrowException(v4navigationProperty, "Unable to locate equivalent v4 property for " + segment.NavigationProperty.Name);
               
            // Separately, look up navigation source (entity set)
            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            ExceptionUtil.IfNullThrowException(v4navigationSource, "Unable to locate navigation source for entity set: " + segment.EntitySet.Name);

            return new NavigationPropertySegment(v4navigationProperty, v4navigationSource);
        }

        /// <summary>
        /// Translates a V3 EntitySetSegment (e.g. "/Boxes") to a V4 EntitySetSegment
        /// </summary>
        /// <param name="segment">OData V3 EntitySetSegment</param>
        /// <returns>OData V4 EntitySetSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.EntitySetSegment segment)
        {
            ExceptionUtil.IfNullThrowException(segment.EntitySet, "v3 entity set not found");

            IEdmEntitySet v4entitySet = v4model.FindDeclaredEntitySet(segment.EntitySet.Name);
            ExceptionUtil.IfNullThrowException(v4entitySet, "Unable to locate equivalent v4 entity set for " + segment.EntitySet.Name + " (element type " + segment.EntitySet.ElementType.GetFullTypeName() + ")");
        
            return new EntitySetSegment(v4entitySet);
        }

        /// <summary>
        /// Translates a V3 KeySegment (e.g. "/Boxes(1)") to a V4 KeySegment
        /// </summary>
        /// <param name="segment">OData V3 KeySegment</param>
        /// <returns>OData V4 KeySegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.KeySegment segment)
        {
            ExceptionUtil.IfArgumentNullThrowException(segment.EntitySet, "segment.EntitySet", "v3 entity set not found");

            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            ExceptionUtil.IfNullThrowException(v4navigationSource, "Unable to locate equivalent v4 entity set for " + segment.EntitySet.Name + " (element type " + segment.EntitySet.ElementType.GetFullTypeName() + ")");

            IEdmEntityType v4type = v4model.GetV4Definition(segment.EdmType) as IEdmEntityType;
            ExceptionUtil.IfNullThrowException(v4type, "Unable to locate equivalent v4 type for v3 type: " + segment.EdmType.GetFullTypeName());

            return new KeySegment(segment.Keys, v4type, v4navigationSource);
        }

        /// <summary>
        /// Translates at V3 PropertySegment (e.g. "/Boxes(1)/Name") into a V4 PropertySegment
        /// </summary>
        /// <param name="segment">OData V3 PropertySegment</param>
        /// <returns>OData V4 PropertySegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.PropertySegment segment)
        {
            // Look up equivalent declaring type in V4 model
            IEdmStructuredType v4Type = v4model.GetV4Definition(segment.Property.DeclaringType) as IEdmStructuredType;
            ExceptionUtil.IfNullThrowException(v4Type, "Unable to locate equivalent v4 type for " + segment.Property.DeclaringType.GetFullTypeName());

            // Look up corresponding Property in V4 Type by name
            IEdmStructuralProperty v4Property = v4Type.FindProperty(segment.Property.Name) as IEdmStructuralProperty;
            ExceptionUtil.IfNullThrowException(v4Property, "Unable to locate equivalent v4 property for " + segment.Property.Name);

            return new PropertySegment(v4Property);
        }

        /// <summary>
        /// (TODO: example)
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
                IEnumerable<Data.Edm.IEdmFunctionParameter> entityParams = op.Parameters.Where(p => p.Type.Definition.GetType().GetInterfaces().Any(i => i.Name == "IEdmEntityType"));
                foreach (var p in entityParams)
                {
                    // Find bounded operations
                    IEnumerable<IEdmOperation> boundOps = v4model.FindBoundOperations(v4model.FindType(p.Type.Definition.GetFullTypeName()));
                    v4BoundOps.AddRange(boundOps.Where(boundOp => boundOp.FullName() == op.Container.Namespace + "." + op.Name));
                }

                // Find v4 operation imports
                IEnumerable<IEdmOperationImport> foundOperationImports = v4model.FindDeclaredOperationImports(op.Container.Namespace + "." + op.Name);
                v4OpImports.AddRange(foundOperationImports);

                // Find v4 unbounded operations
                IEnumerable<IEdmOperation> foundOperations = v4model.FindOperations(op.Container.Namespace + "." + op.Name);
                v4UnboundOps.AddRange(foundOperations);
            }


            ODataPathSegment result;
            // If function imports are found, return OperationImportSegment
            if (v4OpImports.Any())
            {
                result = new OperationImportSegment(v4OpImports.First(), v4entitySet);
            }
            // Otherwise some other function has been found, so return OperationSegment
            else if (v4BoundOps.Any() || v4UnboundOps.Any())
            {
                v4BoundOps.AddRange(v4UnboundOps);
                result = new OperationSegment(v4BoundOps.First(), v4entitySet);
            }
            else
            {
                throw new ArgumentException("Unable to locate any equivalent v4 operations from v3 operation segment");
            }
            return result;
        }

        /// <summary>
        /// Translates a V3 OpenPropertySegment into its V4 equivalent: a DynamicPathSegment.
        /// </summary>
        /// <param name="segment">V3 OpenPropertySegment</param>
        /// <returns>V4 DynamicPathSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.OpenPropertySegment segment)
        {
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
            ExceptionUtil.IfArgumentNullThrowException(segment.EntitySet, "segment.EntitySet", "V3 entity set not found");
            IEdmStructuredType v4Type = v4model.GetV4Definition(segment.NavigationProperty.DeclaringType) as IEdmStructuredType;
            IEdmNavigationProperty v4navigationProperty = v4Type.FindProperty(segment.NavigationProperty.Name) as IEdmNavigationProperty;
            ExceptionUtil.IfNullThrowException(v4Type, "Unable to find equivalent V4 property of V3 property: " + segment.NavigationProperty.Name);

            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(segment.EntitySet.Name);
            ExceptionUtil.IfNullThrowException(v4navigationSource, "Unable to find equivalent V4 entity set of V3 Entity set " + segment.EntitySet.Name);
            return new NavigationPropertyLinkSegment(v4navigationProperty, v4navigationSource);
        }

        /// <summary>
        /// Translates a v3 ValueSegment (e.g. /Boxes(1)/Name/$value) into a v4 ValueSegment
        /// </summary>
        /// <param name="segment">OData V3 ValueSegment</param>
        /// <returns>OData V4 ValueSegment</returns>
        public override ODataPathSegment Translate(Data.OData.Query.SemanticAst.ValueSegment segment)
        {
            ExceptionUtil.IfArgumentNullThrowException(segment, "segment", "Value segment is invalid");
            ExceptionUtil.IfArgumentNullThrowException(segment.EdmType, "segment.EdmType", "Value segment has invalid type");

            IEdmType v4prevType = v4model.FindType(segment.EdmType.GetFullTypeName());

            ExceptionUtil.IfNullThrowException(v4prevType, "Unable to locate equivalent type for V3 type segment in V4");
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
            ExceptionUtil.IfArgumentNullThrowException(segment.EntitySet, "segment.EntitySet", "null EntitySet in BatchReferenceSegment");
            IEdmEntitySet v4entitySet = v4model.FindDeclaredEntitySet(segment.EntitySet.Name);
            IEdmType v4type = v4model.GetV4Definition(segment.EdmType);

            ExceptionUtil.IfNullThrowException(v4type, "Unable to locate equivalent type for V3 type segment in V4 model");
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
