// ------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright © Microsoft Corporation. All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Extensions.OData.Migration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.OData.UriParser;
    using Microsoft.OData.Edm;
    using System.Collections.ObjectModel;
    using System.Globalization;
    internal class QueryNodeTranslator : Data.OData.Query.SemanticAst.QueryNodeVisitor<QueryNode>
    {
        private readonly IEdmModel v4model;
        public QueryNodeTranslator(IEdmModel v4model)
        {
            this.v4model = v4model;
        }

        /// <summary>
        /// Filter down the generic Query Node to one of the more specific translation methods listed.
        /// </summary>
        /// <param name="nodeIn">V3 Query Node</param>
        /// <returns>Result of any of the V4 Translation processes.</returns>
        public QueryNode VisitNode(Data.OData.Query.SemanticAst.QueryNode nodeIn)
        {
            if (nodeIn is Data.OData.Query.SemanticAst.AllNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.AllNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.AnyNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.AnyNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.BinaryOperatorNode) return Visit((nodeIn as Data.OData.Query.SemanticAst.BinaryOperatorNode));
            else if (nodeIn is Data.OData.Query.SemanticAst.CollectionNavigationNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.CollectionNavigationNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.CollectionPropertyAccessNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.CollectionPropertyAccessNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.ConstantNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.ConstantNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.ConvertNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.ConvertNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.EntityCollectionCastNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.EntityCollectionCastNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.EntityRangeVariableReferenceNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.EntityRangeVariableReferenceNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.NonentityRangeVariableReferenceNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.NonentityRangeVariableReferenceNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.SingleEntityCastNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.SingleEntityCastNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.SingleNavigationNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.SingleNavigationNode);
            else if (nodeIn is Data.OData.Query.SingleEntityFunctionCallNode) return Visit(nodeIn as Data.OData.Query.SingleEntityFunctionCallNode);
            else if (nodeIn is Data.OData.Query.SingleValueFunctionCallNode) return Visit(nodeIn as Data.OData.Query.SingleValueFunctionCallNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.EntityCollectionFunctionCallNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.EntityCollectionFunctionCallNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.CollectionFunctionCallNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.CollectionFunctionCallNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.SingleValueOpenPropertyAccessNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.SingleValueOpenPropertyAccessNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.SingleValuePropertyAccessNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.SingleValuePropertyAccessNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.UnaryOperatorNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.UnaryOperatorNode);
            else if (nodeIn is Data.OData.Query.SemanticAst.NamedFunctionParameterNode) return Visit(nodeIn as Data.OData.Query.SemanticAst.NamedFunctionParameterNode);
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Translates a V3 AllNode to V4 AllNode
        /// </summary>
        /// <param name="nodeIn">V3 AllNode</param>
        /// <returns>V4 AllNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.AllNode nodeIn)
        {
            IEnumerable<RangeVariable> translated = nodeIn.RangeVariables.Select(r => TranslateRangeVariable(r));
            AllNode ret = new AllNode(new Collection<RangeVariable>(translated.ToList()), TranslateRangeVariable(nodeIn.CurrentRangeVariable));
            ret.Source = VisitNode(nodeIn.Source) as CollectionNode;
            ret.Body = VisitNode(nodeIn.Body) as SingleValueNode;
            return ret;
        }

        /// <summary>
        /// Translates a V3 AnyNode to V4 AnyNode
        /// </summary>
        /// <param name="nodeIn">V3 AnyNode</param>
        /// <returns>V4 AnyNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.AnyNode nodeIn)
        {
            List<RangeVariable> translated = nodeIn.RangeVariables.Select(r => TranslateRangeVariable(r)).ToList();
            AnyNode ret = new AnyNode(new Collection<RangeVariable>(translated), TranslateRangeVariable(nodeIn.CurrentRangeVariable));
            ret.Source = VisitNode(nodeIn.Source) as CollectionNode;
            ret.Body = VisitNode(nodeIn.Body) as SingleValueNode;
            return ret;
        }

        /// <summary>
        /// Translates V3 BinaryOperatorNode to V4 BinaryOperatorNode
        /// </summary>
        /// <param name="nodeIn">V3 BinaryOperatorNode</param>
        /// <returns>V4 BinaryOperatorNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.BinaryOperatorNode nodeIn)
        {
            BinaryOperatorKind kind = (BinaryOperatorKind)(int)nodeIn.OperatorKind;
            return new BinaryOperatorNode(kind, VisitNode(nodeIn.Left) as SingleValueNode, VisitNode(nodeIn.Right) as SingleValueNode);
        }


        /// <summary>
        /// Translates V3 CollectionNavigationNode to V4 CollectionNavigationNode
        /// </summary>
        /// <param name="nodeIn">V3 CollectionNavigationNode</param>
        /// <returns>V4 CollectionNavigationNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.CollectionNavigationNode nodeIn)
        {
            Console.WriteLine("Collection navigation node");
            IEdmPathExpression pathExpr = null;
            if (nodeIn.EntitySet != null)
            {
                IEdmNavigationSource navigationSource = v4model.FindDeclaredNavigationSource(nodeIn.EntitySet.Name);
                pathExpr = navigationSource.Path;
            }
            IEdmStructuredType v4Type = v4model.FindType(nodeIn.NavigationProperty.DeclaringType.GetFullTypeName()) as IEdmStructuredType;
            IEdmNavigationProperty v4navProperty = v4Type.FindProperty(nodeIn.NavigationProperty.Name) as IEdmNavigationProperty;
            return new CollectionNavigationNode(VisitNode(nodeIn.Source) as SingleResourceNode, v4navProperty, pathExpr);
        }

        /// <summary>
        /// Translates a V3 CollectionPropertyAccessNode to a V4 CollectionPropertyAccessNode 
        /// </summary>
        /// <param name="nodeIn">V3 CollectionPropertyAccessNode</param>
        /// <returns>V4 CollectionPropertyAccessNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.CollectionPropertyAccessNode nodeIn)
        {
            Console.WriteLine("CollectionPropertyAccessnode");
            IEdmType v4CollectionType = v4model.FindType(nodeIn.CollectionType.Definition.GetFullTypeName());
            EdmUtil.IfNullThrowException(v4CollectionType, "Unable to locate v4 collection type " + nodeIn.CollectionType.Definition.GetFullTypeName());

            IEdmProperty v4Property = (v4CollectionType as IEdmStructuredType).FindProperty(nodeIn.Property.Name);
            EdmUtil.IfNullThrowException(v4Property, "Unable to locate v4 property " + nodeIn.Property.Name);
            return new CollectionPropertyAccessNode(VisitNode(nodeIn.Source) as SingleValueNode, v4Property);
        }

        /// <summary>
        /// Translates a V3 ConstantNode to V4 ConstantNode.  Since node to text translation for Constant nodes
        /// is based on literal text, a type-check based conversion is necessary to make the literal text V4 compliant
        /// </summary>
        /// <param name="nodeIn">V3 ConstantNode</param>
        /// <returns>V4 ConstantNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.ConstantNode nodeIn)
        {
            Console.WriteLine("Constant node");

            // Within OData libraries, constant nodes are treated as literal text.
            // To translate, we need to check the type ourselves.
            var transformations = new Dictionary<Func<object, bool>, Func<object, string>>
            {
                { v => v is Guid, v => nodeIn.Value.ToString() },
                { v => v is Int64, v => nodeIn.Value.ToString() },
                { v => v is DateTime, v => ((DateTime)nodeIn.Value).ToString("s", CultureInfo.InvariantCulture) },
                { v => true, v => nodeIn.LiteralText }
            };

            string literalText = transformations.First(pair => pair.Key(nodeIn.Value)).Value(nodeIn.Value);
            return new ConstantNode(nodeIn.Value, literalText);
        }

        /// <summary>
        /// Translates a V3 ConvertNode to V4 ConvertNode 
        /// </summary>
        /// <param name="nodeIn">V3 ConvertNode</param>
        /// <returns>V4 ConvertNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.ConvertNode nodeIn)
        {
            Console.WriteLine("convert node:");
            IEdmTypeReference v4typeReference = v4model.GetV4Definition(nodeIn.TypeReference);
            EdmUtil.IfNullThrowException(v4typeReference, "Unable to locate v4 type reference " + nodeIn.TypeReference.Definition.GetFullTypeName());
            return new ConvertNode(VisitNode(nodeIn.Source) as SingleValueNode, v4typeReference);
        }

        /// <summary>
        /// Translates a V3 EntityCollectionCastNode to V4 CollectionResourceCastNode (equivalent)
        /// </summary>
        /// <param name="nodeIn">V3 EntityCollectionCastNode</param>
        /// <returns>V4 CollectionResourceCastNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.EntityCollectionCastNode nodeIn)
        {
            Console.WriteLine("EntityCollectionCastNode");
            IEdmStructuredType v4Type = v4model.GetV4Definition(nodeIn.ItemType.Definition) as IEdmStructuredType;
            EdmUtil.IfNullThrowException(v4Type, "Unable to locate v4 type " + nodeIn.ItemType.Definition.GetFullTypeName());
            return new CollectionResourceCastNode(VisitNode(nodeIn.Source) as CollectionResourceNode, v4Type);
        }

        /// <summary>
        /// Translates a V3 EntityRangeVariableReferenceNode to V4 ResourceRangeVariableReferenceNode (equivalent)
        /// </summary>
        /// <param name="nodeIn">V3 EntityRangeVariableReferenceNode</param>
        /// <returns>V4 ResourcerangeVariableReferenceNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.EntityRangeVariableReferenceNode nodeIn)
        {
            return new ResourceRangeVariableReferenceNode(nodeIn.Name, TranslateRangeVariable(nodeIn.RangeVariable) as ResourceRangeVariable);
        }

        /// <summary>
        /// Translates a V3 NonEntityRangeVariableNode to V4 NonResourceRangeVariableReferenceNode (equivalent)
        /// </summary>
        /// <param name="nodeIn">V3 NonEntityRangeVariableNode</param>
        /// <returns>V4 NonResourcerangeVariableReferenceNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.NonentityRangeVariableReferenceNode nodeIn)
        {
            return new NonResourceRangeVariableReferenceNode(nodeIn.Name, TranslateRangeVariable(nodeIn.RangeVariable) as NonResourceRangeVariable);
        }

        /// <summary>
        /// Translates a V3 SingleEntityCastNode to V4 SingleResourceCastNode (equivalent)
        /// </summary>
        /// <param name="nodeIn">V3 SingleEntityCastNode</param>
        /// <returns>V4 SingleResourceCastNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.SingleEntityCastNode nodeIn)
        {
            Console.WriteLine("SingleEntityCastNode");
            IEdmStructuredType v4Type = v4model.GetV4Definition(nodeIn.TypeReference.Definition) as IEdmStructuredType;
            EdmUtil.IfNullThrowException(v4Type, "Unable to locate v4 type " + nodeIn.TypeReference.Definition.GetFullTypeName());
            return new SingleResourceCastNode(VisitNode(nodeIn.Source) as SingleResourceNode, v4Type);
        }

        /// <summary>
        /// Translates a V3 SingleNavigationNode to V4 SingleNavigationNode
        /// </summary>
        /// <param name="nodeIn">V3 SingleNavigationNode</param>
        /// <returns>V4 SingleNavigationNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.SingleNavigationNode nodeIn)
        {
            Console.WriteLine("Single navigation node");
            IEdmStructuredType v4Type = v4model.GetV4Definition(nodeIn.TypeReference.Definition) as IEdmStructuredType;
            EdmUtil.IfNullThrowException(v4Type, "Unable to locate v4 type " + nodeIn.TypeReference.Definition.GetFullTypeName());

            IEdmNavigationProperty v4navProperty = v4Type.FindProperty(nodeIn.NavigationProperty.Name) as IEdmNavigationProperty;
            EdmUtil.IfNullThrowException(v4Type, "Unable to locate v4 navigation property " + nodeIn.NavigationProperty.Name);

            // TODO set binding path to null for now (doesn't appear to be a V3 equivalent inside nodeIn, 
            // but there is a way if I had an IEdmNavigationSource
            return new SingleNavigationNode(VisitNode(nodeIn.Source) as SingleResourceNode, v4navProperty, null);
        }

        /// <summary>
        /// Translates a V3 SingleEntityFunctionCallNode to V4 SingleResourceFunctionCallNode (equivalent)
        /// </summary>
        /// <param name="nodeIn">V3 SingleEntityFunctionCallNode</param>
        /// <returns>V4 SingleResourceFunctionCallNode</returns>
        public override QueryNode Visit(Data.OData.Query.SingleEntityFunctionCallNode nodeIn)
        {
            IEnumerable<QueryNode> v4nodes = nodeIn.Arguments.Select(v3node => VisitNode(v3node));
            // Navigation source can be null
            IEdmNavigationSource v4navigationSource = v4model.FindDeclaredNavigationSource(nodeIn.EntitySet?.Name ?? "");
            IEdmStructuredTypeReference v4TypeRef = v4model.GetV4Definition(nodeIn.TypeReference) as IEdmStructuredTypeReference;
            EdmUtil.IfNullThrowException(v4TypeRef, "Unable to locate v4 type reference for " + nodeIn.TypeReference.Definition.GetFullTypeName());
            return new SingleResourceFunctionCallNode(nodeIn.Name, v4nodes, v4TypeRef, v4navigationSource);
        }

        /// <summary>
        /// Translates a V3 SingleValueFunctionCallNode to V4 SingleValueFunctionCallNode
        /// </summary>
        /// <param name="nodeIn">V3 SingleValueFunctionCallNode</param>
        /// <returns>V4 SingleValueFunctionCallNode</returns>
        public override QueryNode Visit(Data.OData.Query.SingleValueFunctionCallNode nodeIn)
        {
            Console.WriteLine("SingleValueFunctionCAllNode");
            IEnumerable<QueryNode> v4nodes = nodeIn.Arguments.Select(v3node => VisitNode(v3node));
            IEdmType v4Type = v4model.GetV4Definition(nodeIn.TypeReference.Definition);
            EdmUtil.IfNullThrowException(v4Type, "Unable to locate v4 type " + nodeIn.TypeReference.Definition.GetFullTypeName());

            return new SingleValueFunctionCallNode(nodeIn.Name, v4nodes, v4Type.GetReference());
        }

        /// <summary>
        /// Translates a V3 EntityCollectionFunctionCallNode to V4 CollectionResourceFunctionCallNode
        /// </summary>
        /// <param name="nodeIn">V3 EntityCollectionFunctionCallNode</param>
        /// <returns>V4 CollectionResourceFunctionCallNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.EntityCollectionFunctionCallNode nodeIn)
        {
            EdmUtil.IfArgumentNullThrowException(nodeIn.EntitySet, "nodeIn.EntitySet", "v3 entity set not found");
            IEnumerable<QueryNode> v4params = nodeIn.Parameters.Select(v3node => VisitNode(v3node));
            QueryNode v4source = VisitNode(nodeIn.Source);

            IEdmCollectionTypeReference v4typeRef = v4model.GetV4Definition(nodeIn.CollectionType) as IEdmCollectionTypeReference;
            EdmUtil.IfNullThrowException(v4typeRef, "Unable to locate v4 type reference for " + nodeIn.CollectionType.Definition.GetFullTypeName());

            IEdmEntitySetBase v4navigationSource = v4model.FindDeclaredNavigationSource(nodeIn.EntitySet.Name) as IEdmEntitySetBase;
            EdmUtil.IfNullThrowException(v4navigationSource, "Unable to locate v4 navigation source for " + nodeIn.EntitySet.Name);

            IEnumerable<IEdmFunction> v4functions = nodeIn.FunctionImports.Select(funcImport =>
            {
                IEdmTypeReference v4Reference = v4model.GetV4Definition(funcImport.ReturnType);
                return new EdmFunction(funcImport.Container.Namespace, funcImport.Name, v4Reference);
            });
            return new CollectionResourceFunctionCallNode(nodeIn.Name, v4functions, v4params, v4typeRef, v4navigationSource, v4source);
        }

        /// <summary>
        /// Translates a V3 CollectionFunctionCallNode to a V4 CollectionFunctionCallNode
        /// </summary>
        /// <param name="nodeIn">V3 CollectionFunctionCallNode</param>
        /// <returns>V4 CollectionFunctionCallNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.CollectionFunctionCallNode nodeIn)
        {
            IEnumerable<QueryNode> v4params = nodeIn.Parameters.Select(v3node => VisitNode(v3node));
            QueryNode v4source = VisitNode(nodeIn.Source);
            IEdmCollectionTypeReference v4typeRef = v4model.GetV4Definition(nodeIn.CollectionType) as IEdmCollectionTypeReference;
            EdmUtil.IfNullThrowException(v4typeRef, "Unable to locate v4 type reference for " + nodeIn.CollectionType.Definition.GetFullTypeName());

            IEnumerable<IEdmFunction> v4functions = nodeIn.FunctionImports.Select(funcImport =>
            {
                IEdmTypeReference v4Reference = v4model.GetV4Definition(funcImport.ReturnType);
                return new EdmFunction(funcImport.Container.Namespace, funcImport.Name, v4Reference);
            });
            return new CollectionFunctionCallNode(nodeIn.Name, v4functions, v4params, v4typeRef, v4source);
        }

        /// <summary>
        /// Translates a V3 SingleValueOpenPropertyAccessNode to a V4 SingleValueOpenPropertyAccessNode
        /// </summary>
        /// <param name="nodeIn">V3 SingleValueOpenPropertyAccessNode</param>
        /// <returns>V4 SingleValueOpenPropertyAccessNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.SingleValueOpenPropertyAccessNode nodeIn)
        {
            Console.WriteLine("Single value open property access node");
            return new SingleValueOpenPropertyAccessNode(VisitNode(nodeIn.Source) as SingleValueNode, nodeIn.Name);
        }

        /// <summary>
        /// Translates a V3 SingleValuePropertyAccessNode to a V4 SingleValuePropertyAccessNode
        /// Searches for property in V4 equivalent of V3 parent type in nodeIn
        /// </summary>
        /// <param name="nodeIn">V3 SingleValuePropertyAccessNode</param>
        /// <returns>V4 SingleValuePropertyAccessNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.SingleValuePropertyAccessNode nodeIn)
        {
            SingleValueNode parent = VisitNode(nodeIn.Source) as SingleValueNode;
            IEdmProperty v4Property = (parent.TypeReference.Definition as IEdmStructuredType).FindProperty(nodeIn.Property.Name);
            EdmUtil.IfNullThrowException(v4Property, "Unable to locate v4 property " + nodeIn.Property.Name);
            return new SingleValuePropertyAccessNode(parent, v4Property);
        }

        /// <summary>
        /// Translates a V3 UnaryOperatorNode to V4 UnaryOperatorNode
        /// </summary>
        /// <param name="nodeIn">V3 UnaryOperatorNode</param>
        /// <returns>V4 UnaryOperatorNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.UnaryOperatorNode nodeIn)
        {
            UnaryOperatorKind kind = (UnaryOperatorKind)(int)nodeIn.Kind;
            return new UnaryOperatorNode(kind, VisitNode(nodeIn.Operand) as SingleValueNode);
        }

        /// <summary>
        /// Translates a V3 NamedFunctionParameterNode to V4 NamedFunctionParameterNode
        /// </summary>
        /// <param name="nodeIn">V3 NamedFunctionParameterNode</param>
        /// <returns>V4 NamedFunctionParameterNode</returns>
        public override QueryNode Visit(Data.OData.Query.SemanticAst.NamedFunctionParameterNode nodeIn)
        {
            Console.WriteLine("Named function parameter node");
            return new NamedFunctionParameterNode(nodeIn.Name, VisitNode(nodeIn.Value));
        }

        /// <summary>
        /// Translates a V3 RangeVariable to a V4 RangeVariable by handling the two cases: entity and nonentity
        /// </summary>
        /// <param name="rangeVariable">V3 RangeVariable</param>
        /// <param name="collectionNode">EXPERIMENTAL - MIGHT NEED FOR ANY/ALL NODE, MIGHT NOT</param>
        /// <returns>V4 RangeVariable</returns>
        public RangeVariable TranslateRangeVariable(Data.OData.Query.SemanticAst.RangeVariable rangeVariable)
        {
            // Range variables in v3 are either EntityRangeVariables or NonentityRangeVariables.
            if (rangeVariable is Data.OData.Query.SemanticAst.EntityRangeVariable)
            {
                // Attempt to locate IEdmNavigationSource based on EntityCollectionNode and EntitySet
                var entityRangeVar = (Data.OData.Query.SemanticAst.EntityRangeVariable)rangeVariable;
                Data.OData.Query.SemanticAst.EntityCollectionNode entityCollectionNode = entityRangeVar.EntityCollectionNode;
                bool hasEntityCollectionNodeEntitySet = entityCollectionNode != null && entityCollectionNode.EntitySet != null;
                bool hasEntityRangeVariableEntitySet = entityRangeVar.EntitySet != null;

                IEdmNavigationSource navigationSource = null;
                if (hasEntityCollectionNodeEntitySet)
                {
                    navigationSource = v4model.FindDeclaredNavigationSource(entityCollectionNode.EntitySet.Name);
                }
                else if (hasEntityRangeVariableEntitySet)
                {
                    navigationSource = v4model.FindDeclaredNavigationSource(entityRangeVar.EntitySet.Name);
                }

                IEdmEntityType v4Type = v4model.GetV4Definition(rangeVariable.TypeReference.Definition) as IEdmEntityType;
                IEdmStructuredTypeReference refType = new EdmEntityTypeReference(v4Type, true);
                return new ResourceRangeVariable(rangeVariable.Name, refType, navigationSource);

            }
            else if (rangeVariable is Data.OData.Query.SemanticAst.NonentityRangeVariable)
            {
                Console.WriteLine("Non entity range variable");

                IEdmTypeReference v4TypeRef = v4model.GetV4Definition(rangeVariable.TypeReference);
                QueryNode collectionNode = null;
                if ((rangeVariable as Data.OData.Query.SemanticAst.NonentityRangeVariable).CollectionNode != null)
                {
                    collectionNode = VisitNode((rangeVariable as Data.OData.Query.SemanticAst.NonentityRangeVariable).CollectionNode);
                }
                return new NonResourceRangeVariable(rangeVariable.Name, v4TypeRef, collectionNode as CollectionNode);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
