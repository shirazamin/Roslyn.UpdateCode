using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;

namespace AddMethodsToClass
{
    public class ControllerUpdate
    {
        private string _controllerType;

        public ControllerUpdate(string controllerType)
        {
            _controllerType = controllerType;
        }

        public MethodDeclarationSyntax PostAllMethod()
        {
            return GetMethodDeclarationSyntax($"Task<IEnumerable<{_controllerType}>>",
                $"Post{_controllerType}",
                new string[] { $"IEnumerable<{_controllerType}>" },
                new string[] { "items" },
                GetBlockSyntax("return await InsertAsync(items);"),
                true);
        }

        public MemberDeclarationSyntax PatchAllMethod()
        {
            return GetMethodDeclarationSyntax($"Task<IEnumerable<{_controllerType}>>",
                $"Patch{_controllerType}",
                new string[] { $"IEnumerable<Delta<{_controllerType}>>" },
                new string[] { "patches" },
                GetBlockSyntax("return await UpdateAsync(patches);"),
                true);
        }

        public MemberDeclarationSyntax DeleteAllMethod()
        {
            return GetMethodDeclarationSyntax($"Task",
                $"Delete{_controllerType}",
                new string[] { $"IEnumerable<string>" },
                new string[] { "ids" },
                GetBlockSyntax("return DeleteAsync(ids);"));
        }

        public MethodDeclarationSyntax GetMethodDeclarationSyntax(string returnTypeName, string methodName, string[] parameterTypes, string[] paramterNames, BlockSyntax blockSyntax, bool asyncMethod = false)
        {
            var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(GetParametersList(parameterTypes, paramterNames)));

            var modifiers = SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            modifiers = asyncMethod ? modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)) : modifiers;
            return SyntaxFactory.MethodDeclaration(
                          modifiers: modifiers,
                          attributeLists: GetRouteAttributes(),
                          returnType: SyntaxFactory.ParseTypeName(returnTypeName),
                          explicitInterfaceSpecifier: null,
                          identifier: SyntaxFactory.Identifier(methodName),
                          typeParameterList: null,
                          parameterList: parameterList,
                          constraintClauses: SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                          body: blockSyntax,
                          semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                  // Annotate that this node should be formatted
                  .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private IEnumerable<ParameterSyntax> GetParametersList(string[] parameterTypes, string[] paramterNames)
        {
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                yield return SyntaxFactory.Parameter(attributeLists: SyntaxFactory.List<AttributeListSyntax>(),
                                                         modifiers: SyntaxFactory.TokenList(),
                                                         type: SyntaxFactory.ParseTypeName(parameterTypes[i]),
                                                         identifier: SyntaxFactory.Identifier(paramterNames[i]),
                                                         @default: null);
            }
        }

        private BlockSyntax GetBlockSyntax(string blockText)
        {
            StatementSyntax returnStatement = SyntaxFactory.ParseStatement(blockText);
            return SyntaxFactory.Block(returnStatement);
        }

        private AttributeListSyntax GetRouteAttribute(string parameter)
        {
            var name = SyntaxFactory.ParseName("Route");
            var arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{parameter}\")");
            var attribute = SyntaxFactory.Attribute(name, arguments); //Route("some_param")
            var attributeList = new SeparatedSyntaxList<AttributeSyntax>().Add(attribute);
            return SyntaxFactory.AttributeList(attributeList); //[Route("some_param")]
        }

        private SyntaxList<AttributeListSyntax> GetRouteAttributes()
        {
            var apiRouteAttribute = GetRouteAttribute($"api/{_controllerType}/bulk");
            var tableRouteAttribute = GetRouteAttribute($"tables/{_controllerType}/bulk");

            return SyntaxFactory.List<AttributeListSyntax>().Add(apiRouteAttribute).Add(tableRouteAttribute);
        }

        private SyntaxTriviaList GetDocumentation(string summary, string param, string paramName, string returns)
        {
            return SyntaxFactory.TriviaList(
                SyntaxFactory.Trivia(
                    SyntaxFactory.DocumentationCommentTrivia(
                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                        SyntaxFactory.List<XmlNodeSyntax>(
                            new XmlNodeSyntax[]{
                            SyntaxFactory.XmlText()
                            .WithTextTokens(
                                SyntaxFactory.TokenList(
                                    SyntaxFactory.XmlTextLiteral(
                                        SyntaxFactory.TriviaList(
                                            SyntaxFactory.DocumentationCommentExterior("///")),
                                        " ",
                                        " ",
                                        SyntaxFactory.TriviaList()))),
                            SyntaxFactory.XmlExampleElement(
                                SyntaxFactory.SingletonList<XmlNodeSyntax>(
                                    SyntaxFactory.XmlText()
                                    .WithTextTokens(
                                        SyntaxFactory.TokenList(
                                            new []{
                                                SyntaxFactory.XmlTextNewLine(
                                                    SyntaxFactory.TriviaList(),
                                                    "\n",
                                                    "\n",
                                                    SyntaxFactory.TriviaList()),
                                                SyntaxFactory.XmlTextLiteral(
                                                    SyntaxFactory.TriviaList(
                                                        SyntaxFactory.DocumentationCommentExterior("///")),
                                                    " Inserts a record from the given item.",
                                                    " Inserts a record from the given item.",
                                                    SyntaxFactory.TriviaList()),
                                                SyntaxFactory.XmlTextNewLine(
                                                    SyntaxFactory.TriviaList(),
                                                    "\n",
                                                    "\n",
                                                    SyntaxFactory.TriviaList()),
                                                SyntaxFactory.XmlTextLiteral(
                                                    SyntaxFactory.TriviaList(
                                                        SyntaxFactory.DocumentationCommentExterior("///")),
                                                    " ",
                                                    " ",
                                                    SyntaxFactory.TriviaList())}))))
                            .WithStartTag(
                                SyntaxFactory.XmlElementStartTag(
                                    SyntaxFactory.XmlName(
                                        SyntaxFactory.Identifier("summary"))))
                            .WithEndTag(
                                SyntaxFactory.XmlElementEndTag(
                                    SyntaxFactory.XmlName(
                                        SyntaxFactory.Identifier("summary")))),
                            SyntaxFactory.XmlText()
                            .WithTextTokens(
                                SyntaxFactory.TokenList(
                                    new []{
                                        SyntaxFactory.XmlTextNewLine(
                                            SyntaxFactory.TriviaList(),
                                            "\n",
                                            "\n",
                                            SyntaxFactory.TriviaList()),
                                        SyntaxFactory.XmlTextLiteral(
                                            SyntaxFactory.TriviaList(
                                                SyntaxFactory.DocumentationCommentExterior("///")),
                                            " ",
                                            " ",
                                            SyntaxFactory.TriviaList())})),
                            SyntaxFactory.XmlExampleElement()
                            .WithStartTag(
                                SyntaxFactory.XmlElementStartTag(
                                    SyntaxFactory.XmlName(
                                        SyntaxFactory.Identifier("param")))
                                .WithAttributes(
                                    SyntaxFactory.SingletonList<XmlAttributeSyntax>(
                                        SyntaxFactory.XmlNameAttribute(
                                            SyntaxFactory.XmlName(
                                                SyntaxFactory.Identifier("name")),
                                            SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken),
                                            SyntaxFactory.IdentifierName("item"),
                                            SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken)))))
                            .WithEndTag(
                                SyntaxFactory.XmlElementEndTag(
                                    SyntaxFactory.XmlName(
                                        SyntaxFactory.Identifier("param")))),
                            SyntaxFactory.XmlText()
                            .WithTextTokens(
                                SyntaxFactory.TokenList(
                                    new []{
                                        SyntaxFactory.XmlTextNewLine(
                                            SyntaxFactory.TriviaList(),
                                            "\n",
                                            "\n",
                                            SyntaxFactory.TriviaList()),
                                        SyntaxFactory.XmlTextLiteral(
                                            SyntaxFactory.TriviaList(
                                                SyntaxFactory.DocumentationCommentExterior("///")),
                                            " ",
                                            " ",
                                            SyntaxFactory.TriviaList())})),
                            SyntaxFactory.XmlExampleElement()
                            .WithStartTag(
                                SyntaxFactory.XmlElementStartTag(
                                    SyntaxFactory.XmlName(
                                        SyntaxFactory.Identifier("returns"))))
                            .WithEndTag(
                                SyntaxFactory.XmlElementEndTag(
                                    SyntaxFactory.XmlName(
                                        SyntaxFactory.Identifier("returns"))))}))));
        }
    }
}