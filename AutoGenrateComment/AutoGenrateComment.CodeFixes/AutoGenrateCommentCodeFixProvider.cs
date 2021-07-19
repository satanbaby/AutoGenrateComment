using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoGenrateComment
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoGenrateCommentCodeFixProvider)), Shared]
    public class AutoGenrateCommentCodeFixProvider : CodeFixProvider
    {
        // The name as it will appear in the light bulb menu
        private const string title = "Add comparison type";

        // The list of rules the code fix can handle
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AutoGenrateCommentAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => FixAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> FixAsync(Document document, PropertyDeclarationSyntax invocationExpr, CancellationToken cancellationToken)
        {
            //// Create a new list of arguments with System.StringComparison.Ordinal
            //invocationExpr.InsertTriviaAfter(Trivia(new xml));
            //var arguments = invocationExpr.ArgumentList.AddArguments(
            //    Argument(
            //            MemberAccessExpression(
            //                SyntaxKind.SimpleMemberAccessExpression,
            //                QualifiedName(IdentifierName("System"), IdentifierName("StringComparison")),
            //                IdentifierName("Ordinal"))));

            //// Indicate to format the list with the current coding style
            //var formattedLocal = arguments.WithAdditionalAnnotations(Formatter.Annotation);

            //// Replace the old local declaration with the new local declaration.
            //var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            //var newRoot = oldRoot.ReplaceNode(invocationExpr.ArgumentList, formattedLocal);

            //return document.WithSyntaxRoot(newRoot);

            var testDocumentation = DocumentationCommentTrivia(
    SyntaxKind.SingleLineDocumentationCommentTrivia,
    List(
        new XmlNodeSyntax[]{
            XmlText()
            .WithTextTokens(
                TokenList(
                    XmlTextLiteral(
                        TriviaList(
                            DocumentationCommentExterior("///")),
                        " ",
                        " ",
                        TriviaList()))),
            XmlElement(
                XmlElementStartTag(
                    XmlName(
                        Identifier("summary"))),
                XmlElementEndTag(
                    XmlName(
                        Identifier("summary"))))
            .WithContent(
                SingletonList<XmlNodeSyntax>(
                    XmlText()
                    .WithTextTokens(
                        TokenList(
                            XmlTextLiteral(
                                TriviaList(),
                                "test",
                                "test",
                                TriviaList()))))),
            XmlText()
            .WithTextTokens(
                TokenList(
                    XmlTextNewLine(
                        TriviaList(),
                        "\n",
                        "\n",
                        TriviaList())))}));


            var newMethodNode = invocationExpr.WithModifiers(
    TokenList(
        new[]{
            Token(
                TriviaList(
                    Trivia(testDocumentation)), // xmldoc
                    SyntaxKind.PublicKeyword, // original 1st token
                    TriviaList()),
            Token(SyntaxKind.StaticKeyword)}));

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(invocationExpr, newMethodNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
