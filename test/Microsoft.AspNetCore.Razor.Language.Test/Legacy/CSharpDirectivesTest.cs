// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public class CSharpDirectivesTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void DirectiveDescriptor_FileScopedMultipleOccurring_CanHaveDuplicates()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                builder =>
                {
                    builder.Usage = DirectiveUsage.FileScopedMultipleOccurring;
                    builder.AddTypeToken();
                });

            // Act & Assert
            ParseDocumentTest(
@"@custom System.Text.Encoding.ASCIIEncoding
@custom System.Text.Encoding.UTF8Encoding",
                new[] { descriptor },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(descriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                        Factory.Span(SpanKindInternal.None, Environment.NewLine, markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)),
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(descriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.UTF8Encoding", markup: false).AsDirectiveToken(descriptor.Tokens[0])),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void DirectiveDescriptor_FileScopedSinglyOccurring_ErrorsIfDuplicate()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                builder =>
                {
                    builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                    builder.AddTypeToken();
                });
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        Resources.FormatDuplicateDirective("custom"),
                        42 + Environment.NewLine.Length, 1, 0, 7)));

            // Act & Assert
            ParseDocumentTest(
@"@custom System.Text.Encoding.ASCIIEncoding
@custom System.Text.Encoding.UTF8Encoding",
                new[] { descriptor },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(descriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                        Factory.Span(SpanKindInternal.None, Environment.NewLine, markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)),
                    Factory.EmptyHtml(),
                    new DirectiveBlock(chunkGenerator,
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.UTF8Encoding", markup: false).AsDirectiveToken(descriptor.Tokens[0])),
                    Factory.EmptyHtml()));
        }

        [Theory]
        [InlineData(DirectiveUsage.FileScopedSinglyOccurring)]
        [InlineData(DirectiveUsage.FileScopedMultipleOccurring)]
        public void DirectiveDescriptor_FileScoped_CanBeBeneathOtherDirectives(DirectiveUsage directiveUsage)
        {
            // Arrange
            var customDescriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                builder =>
                {
                    builder.Usage = directiveUsage;
                    builder.AddTypeToken();
                });
            var somethingDescriptor = DirectiveDescriptor.CreateDirective(
                "something",
                DirectiveKind.SingleLine,
                builder =>
                {
                    builder.Usage = directiveUsage;
                    builder.AddMemberToken();
                });

            // Act & Assert
            ParseDocumentTest(
@"@custom System.Text.Encoding.ASCIIEncoding
@something Else",
                new[] { customDescriptor, somethingDescriptor },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(customDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(customDescriptor.Tokens[0]),
                        Factory.Span(SpanKindInternal.None, Environment.NewLine, markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)),
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(somethingDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("something").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "Else", markup: false).AsDirectiveToken(somethingDescriptor.Tokens[0])),
                    Factory.EmptyHtml()));
        }

        [Theory]
        [InlineData(DirectiveUsage.FileScopedSinglyOccurring)]
        [InlineData(DirectiveUsage.FileScopedMultipleOccurring)]
        public void DirectiveDescriptor_FileScoped_CanBeBeneathOtherWhiteSpaceCommentsAndDirectives(DirectiveUsage directiveUsage)
        {
            // Arrange
            var customDescriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                builder =>
                {
                    builder.Usage = directiveUsage;
                    builder.AddTypeToken();
                });
            var somethingDescriptor = DirectiveDescriptor.CreateDirective(
                "something",
                DirectiveKind.SingleLine,
                builder =>
                {
                    builder.Usage = directiveUsage;
                    builder.AddMemberToken();
                });

            // Act & Assert
            ParseDocumentTest(
@"@* There are two directives beneath this *@
@custom System.Text.Encoding.ASCIIEncoding

@something Else

<p>This is extra</p>",
                new[] { customDescriptor, somethingDescriptor },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition).Accepts(AcceptedCharactersInternal.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Comment, new HtmlSymbol(" There are two directives beneath this ", HtmlSymbolType.RazorComment)).Accepts(AcceptedCharactersInternal.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar).Accepts(AcceptedCharactersInternal.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition).Accepts(AcceptedCharactersInternal.None)),
                    Factory.Markup(Environment.NewLine),
                    new DirectiveBlock(new DirectiveChunkGenerator(customDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(customDescriptor.Tokens[0]),
                        Factory.Span(SpanKindInternal.None, Environment.NewLine, markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)),
                    Factory.Markup(Environment.NewLine),
                    new DirectiveBlock(new DirectiveChunkGenerator(somethingDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("something").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "Else", markup: false).AsDirectiveToken(somethingDescriptor.Tokens[0]),
                        Factory.Span(SpanKindInternal.None, Environment.NewLine, markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)),
                    Factory.Markup(Environment.NewLine),
                    BlockFactory.MarkupTagBlock("<p>"),
                    Factory.Markup("This is extra"),
                    BlockFactory.MarkupTagBlock("</p>")));
        }

        [Fact]
        public void DirectiveDescriptor_TokensMustBeSeparatedBySpace()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken().AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        Resources.FormatDirectiveTokensMustBeSeparatedByWhitespace("custom"),
                        17, 0, 17, 9)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"string1\"\"string2\"",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"string1\"", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void DirectiveDescriptor_CanHandleEOFIncompleteNamespaceTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddNamespaceToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsNamespace("custom"),
                        8, 0, 8, 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom System.",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_CanHandleEOFInvalidNamespaceTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddNamespaceToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsNamespace("custom"),
                        8, 0, 8, 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom System<",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }
        [Fact]
        public void DirectiveDescriptor_CanHandleIncompleteNamespaceTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddNamespaceToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsNamespace("custom"),
                        8, 0, 8, 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom System." + Environment.NewLine,
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_CanHandleInvalidNamespaceTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddNamespaceToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsNamespace("custom"),
                        8, 0, 8, 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom System<" + Environment.NewLine,
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }
        
        [Fact]
        public void ExtensibleDirectiveDoesNotErorrIfNotAtStartOfLineBecauseOfWhitespace()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken());

            // Act & Assert
            ParseCodeBlockTest(Environment.NewLine + "  @custom System.Text.Encoding.ASCIIEncoding",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.Code(Environment.NewLine + "  ").AsStatement(),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void BuiltInDirectiveDoesNotErorrIfNotAtStartOfLineBecauseOfWhitespace()
        {
            // Act & Assert
            ParseCodeBlockTest(Environment.NewLine + "  @addTagHelper \"*, Foo\"",
                Enumerable.Empty<DirectiveDescriptor>(),
                new DirectiveBlock(
                    Factory.Code(Environment.NewLine + "  ").AsStatement(),
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"*, Foo\"")
                        .AsAddTagHelper(
                            "\"*, Foo\"",
                            "*, Foo",
                            "*",
                            "Foo")));
        }

        [Fact]
        public void BuiltInDirectiveErorrsIfNotAtStartOfLine()
        {
            // Act & Assert
            ParseCodeBlockTest("{  @addTagHelper \"*, Foo\"" + Environment.NewLine + "}",
                Enumerable.Empty<DirectiveDescriptor>(),
                new StatementBlock(
                    Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("  ")
                        .AsStatement()
                        .AutoCompleteWith(autoCompleteString: null, atEndOfSpan: false),
                    new DirectiveBlock(
                        Factory.CodeTransition(),
                        Factory
                            .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                            .Accepts(AcceptedCharactersInternal.None),
                        Factory
                            .Span(SpanKindInternal.Markup, " ", markup: false)
                            .Accepts(AcceptedCharactersInternal.None),
                        Factory.Code("\"*, Foo\"")
                            .AsAddTagHelper(
                                "\"*, Foo\"",
                                "*, Foo",
                                "*",
                                "Foo",
                                new RazorError(Resources.FormatDirectiveMustAppearAtStartOfLine("addTagHelper"), new SourceLocation(4, 0, 4), 12))),
                    Factory.Code(Environment.NewLine).AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ExtensibleDirectiveErorrsIfNotAtStartOfLine()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        Resources.FormatDirectiveMustAppearAtStartOfLine("custom"),
                        new SourceLocation(4, 0, 4),
                        6)));

            // Act & Assert
            ParseCodeBlockTest(
                "{  @custom System.Text.Encoding.ASCIIEncoding" + Environment.NewLine + "}",
                new[] { descriptor },
                new StatementBlock(
                    Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("  ")
                        .AsStatement()
                        .AutoCompleteWith(autoCompleteString: null, atEndOfSpan: false),
                    new DirectiveBlock(chunkGenerator,
                        Factory.CodeTransition(),
                        Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                        Factory.Span(SpanKindInternal.None, Environment.NewLine, markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)),
                    Factory.EmptyCSharp().AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void DirectiveDescriptor_UnderstandsTypeTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom System.Text.Encoding.ASCIIEncoding",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void DirectiveDescriptor_UnderstandsMemberTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddMemberToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom Some_Member",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "Some_Member", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void Parser_ParsesNamespaceDirectiveToken_WithSingleSegment()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddNamespaceToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom BaseNamespace",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "BaseNamespace", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void Parser_ParsesNamespaceDirectiveToken_WithMultipleSegments()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddNamespaceToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom BaseNamespace.Foo.Bar",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "BaseNamespace.Foo.Bar", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void DirectiveDescriptor_UnderstandsStringTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"AString\"",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"AString\"", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void DirectiveDescriptor_StringToken_ParserErrorForUnquotedValue()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsQuotedStringLiteral("custom"),
                        new SourceLocation(8, 0, 8),
                        length: 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom AString",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_StringToken_ParserErrorForNonStringValue()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsQuotedStringLiteral("custom"),
                        new SourceLocation(8, 0, 8),
                        length: 1)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom {foo?}",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_StringToken_ParserErrorForSingleQuotedValue()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsQuotedStringLiteral("custom"),
                        new SourceLocation(8, 0, 8),
                        length: 9)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom 'AString'",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_StringToken_ParserErrorForPartialQuotedValue()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsQuotedStringLiteral("custom"),
                        new SourceLocation(8, 0, 8),
                        length: 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom AString\"",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_UnderstandsMultipleTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken().AddMemberToken().AddStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom System.Text.Encoding.ASCIIEncoding Some_Member \"AString\"",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),

                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0]),

                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "Some_Member", markup: false).AsDirectiveToken(descriptor.Tokens[1]),

                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"AString\"", markup: false).AsDirectiveToken(descriptor.Tokens[2])));
        }

        [Fact]
        public void DirectiveDescriptor_UnderstandsRazorBlocks()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.RazorBlock,
                b => b.AddStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"Header\" { <p>F{o}o</p> }",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"Header\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.AllWhiteSpace),
                    Factory.MetaCode("{")
                        .AutoCompleteWith(null, atEndOfSpan: true)
                        .Accepts(AcceptedCharactersInternal.None),
                    new MarkupBlock(
                        Factory.Markup(" "),
                        new MarkupTagBlock(
                            Factory.Markup("<p>")),
                        Factory.Markup("F", "{", "o", "}", "o"),
                        new MarkupTagBlock(
                            Factory.Markup("</p>")),
                        Factory.Markup(" ")),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void DirectiveDescriptor_UnderstandsCodeBlocks()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.CodeBlock,
                b => b.AddStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"Name\" { foo(); bar(); }",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"Name\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.AllWhiteSpace),
                    Factory.MetaCode("{")
                        .AutoCompleteWith(null, atEndOfSpan: true)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code(" foo(); bar(); ").AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void DirectiveDescriptor_AllowsWhiteSpaceAroundTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken().AddMemberToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom    System.Text.Encoding.ASCIIEncoding       Some_Member    ",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),

                    Factory.Span(SpanKindInternal.Code, "    ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "System.Text.Encoding.ASCIIEncoding", markup: false).AsDirectiveToken(descriptor.Tokens[0]),

                    Factory.Span(SpanKindInternal.Code, "       ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "Some_Member", markup: false).AsDirectiveToken(descriptor.Tokens[1]),

                    Factory.Span(SpanKindInternal.None, "    ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_ErrorsForInvalidMemberTokens()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddMemberToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatDirectiveExpectsIdentifier("custom"),
                        new SourceLocation(8, 0, 8),
                        length: 1)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom -Some_Member",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_NoErrorsSemicolonAfterDirective()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"hello\" ;  ",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"hello\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                    Factory.Span(SpanKindInternal.None, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.MetaCode(";").Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.None, "  ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Theory]
        [InlineData("string?")]
        [InlineData("string?[]")]
        [InlineData("global::System.Int32?")]
        [InlineData("KeyValuePair<string, string>?")]
        [InlineData("KeyValuePair<string, string>?[]")]
        [InlineData("global::System.Collections.Generic.KeyValuePair<string, string>?[]")]
        public void DirectiveDescriptor_AllowsNullableTypes(string expectedType)
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken());

            // Act & Assert
            ParseCodeBlockTest(
                $"@custom {expectedType}",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, expectedType, markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Theory]
        [InlineData("(bool, int)")]
        [InlineData("(int aa, string bb)?")]
        [InlineData("(  int?   q   ,  bool   w   )")]
        [InlineData("( int  ?  q, bool ?w ,(long ?  [])) ?")]
        [InlineData("(List<(int, string)?> aa, string bb)")]
        [InlineData("(string ss, (int u, List<(string, int)> k, (Char c, bool b, List<int> l)), global::System.Int32[] a)")]
        public void DirectiveDescriptor_AllowsTupleTypes(string expectedType)
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken());

            // Act & Assert
            ParseCodeBlockTest(
                $"@custom {expectedType}",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, expectedType, markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void DirectiveDescriptor_AllowsTupleTypes_IgnoresTrailingWhitespace()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddTypeToken());

            // Act & Assert
            ParseCodeBlockTest(
                $"@custom (bool, int?)   ",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "(bool, int?)", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                    Factory.Span(SpanKindInternal.None, "   ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_ErrorsExtraContentAfterDirective()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatUnexpectedDirectiveLiteral("custom", "line break"),
                        new SourceLocation(16, 0, 16),
                        length: 7)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"hello\" \"world\"",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"hello\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),

                    Factory.Span(SpanKindInternal.None, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_ErrorsWhenExtraContentBeforeBlockStart()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.CodeBlock,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatUnexpectedDirectiveLiteral("custom", "{"),
                        new SourceLocation(16, 0, 16),
                        length: 5)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"Hello\" World { foo(); bar(); }",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"Hello\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),

                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.AllWhiteSpace)));
        }

        [Fact]
        public void DirectiveDescriptor_ErrorsWhenEOFBeforeDirectiveBlockStart()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.CodeBlock,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatUnexpectedEOFAfterDirective("custom", "{"),
                        new SourceLocation(15, 0, 15),
                        length: 1)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"Hello\"",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"Hello\"", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void DirectiveDescriptor_ErrorsWhenMissingEndBrace()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.CodeBlock,
                b => b.AddStringToken());
            var chunkGenerator = new DirectiveChunkGenerator(descriptor);
            chunkGenerator.Diagnostics.Add(
                RazorDiagnostic.Create(
                    new RazorError(
                        LegacyResources.FormatParseError_Expected_EndOfBlock_Before_EOF("custom", "}", "{"),
                        new SourceLocation(16, 0, 16),
                        length: 1)));

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"Hello\" {",
                new[] { descriptor },
                new DirectiveBlock(chunkGenerator,
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"Hello\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.AllWhiteSpace),
                    Factory.MetaCode("{")
                        .AutoCompleteWith("}", atEndOfSpan: true)
                        .Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void TagHelperPrefixDirective_DuplicatesCauseError()
        {
            // Arrange
            var expectedDiagnostic = RazorDiagnosticFactory.CreateParsing_DuplicateDirective(
                "tagHelperPrefix",
                new SourceSpan(null, 22 + Environment.NewLine.Length, 1, 0, 16));

            // Act
            var document = ParseDocument(
@"@tagHelperPrefix ""th:""
@tagHelperPrefix ""th""",
                directives: null,
                designTime: false);

            // Assert
            var directive = document.Root.Children.OfType<Block>().Last();
            var erroredSpan = (Span)directive.Children.Last();
            var chunkGenerator = Assert.IsType<TagHelperPrefixDirectiveChunkGenerator>(erroredSpan.ChunkGenerator);
            var diagnostic = Assert.Single(chunkGenerator.Diagnostics);
            Assert.Equal(expectedDiagnostic, diagnostic);
        }

        [Fact]
        public void TagHelperPrefixDirective_NoValueSucceeds()
        {
            ParseBlockTest("@tagHelperPrefix \"\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.TagHelperPrefixKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"\"")
                        .AsTagHelperPrefixDirective("\"\"", string.Empty)));
        }

        [Fact]
        public void TagHelperPrefixDirective_Succeeds()
        {
            ParseBlockTest("@tagHelperPrefix Foo",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.TagHelperPrefixKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo")
                        .AsTagHelperPrefixDirective("Foo", "Foo")));
        }

        [Fact]
        public void TagHelperPrefixDirective_WithQuotes_Succeeds()
        {
            ParseBlockTest("@tagHelperPrefix \"Foo\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.TagHelperPrefixKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"Foo\"")
                        .AsTagHelperPrefixDirective("\"Foo\"", "Foo")));
        }

        [Fact]
        public void TagHelperPrefixDirective_RequiresValue()
        {
            // Arrange 
            var expectedError = new RazorError(
                LegacyResources.FormatParseError_DirectiveMustHaveValue(SyntaxConstants.CSharp.TagHelperPrefixKeyword),
                absoluteIndex: 1, lineIndex: 0, columnIndex: 1, length: 15);

            // Act & Assert
            ParseBlockTest("@tagHelperPrefix ",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.TagHelperPrefixKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.EmptyCSharp()
                        .AsTagHelperPrefixDirective(string.Empty, string.Empty, expectedError)
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void TagHelperPrefixDirective_StartQuoteRequiresDoubleQuotesAroundValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.ParseError_Unterminated_String_Literal,
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 1),
                new RazorError(
                    LegacyResources.FormatParseError_IncompleteQuotesAroundDirective(SyntaxConstants.CSharp.TagHelperPrefixKeyword),
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 4),
                new RazorError(
                    Resources.FormatInvalidTagHelperPrefixValue(SyntaxConstants.CSharp.TagHelperPrefixKeyword, '"', "\"Foo"),
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 4)
            };

            // Act & Assert
            ParseBlockTest("@tagHelperPrefix \"Foo",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.TagHelperPrefixKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"Foo")
                        .AsTagHelperPrefixDirective("\"Foo", "\"Foo", expectedErrors)));
        }

        [Fact]
        public void TagHelperPrefixDirective_EndQuoteRequiresDoubleQuotesAroundValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.ParseError_Unterminated_String_Literal,
                    absoluteIndex: 23, lineIndex: 0, columnIndex: 23, length: 1),
                new RazorError(
                    LegacyResources.FormatParseError_IncompleteQuotesAroundDirective(SyntaxConstants.CSharp.TagHelperPrefixKeyword),
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 7),
                new RazorError(
                    Resources.FormatInvalidTagHelperPrefixValue(SyntaxConstants.CSharp.TagHelperPrefixKeyword, ' ', "Foo   \""),
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 7)
            };

            // Act & Assert
            ParseBlockTest("@tagHelperPrefix Foo   \"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.TagHelperPrefixKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo   \"")
                        .AsTagHelperPrefixDirective("Foo   \"", "Foo   \"", expectedErrors)));
        }

        [Fact]
        public void RemoveTagHelperDirective_NoValue_Invalid()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText(string.Empty),
                    new SourceLocation(18, 0, 18),
                    length: 1)
            };

            ParseBlockTest("@removeTagHelper \"\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"\"")
                        .AsRemoveTagHelper(
                            "\"\"",
                            string.Empty,
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void RemoveTagHelperDirective_InvalidLookupText_AddsError()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("Foo"),
                    new SourceLocation(17, 0, 17),
                    length: 3)
            };

            ParseBlockTest("@removeTagHelper Foo",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo")
                        .AsRemoveTagHelper(
                            "Foo",
                            "Foo",
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void RemoveTagHelperDirective_SingleQuotes_AddsError()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("'*, Foo'"),
                    new SourceLocation(17, 0, 17),
                    length: 8)
            };

            ParseBlockTest("@removeTagHelper '*, Foo'",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("'*, Foo'")
                        .AsRemoveTagHelper(
                            "'*, Foo'",
                            "'*, Foo'",
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void RemoveTagHelperDirective_WithQuotes_InvalidLookupText_AddsError()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("Foo"),
                    new SourceLocation(18, 0, 18),
                    length: 3)
            };

            ParseBlockTest("@removeTagHelper \"Foo\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"Foo\"")
                        .AsRemoveTagHelper(
                            "\"Foo\"",
                            "Foo",
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void RemoveTagHelperDirective_SupportsSpaces()
        {
            ParseBlockTest("@removeTagHelper     Foo,   Bar    ",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, "     ", markup: false)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo,   Bar    ")
                        .AsRemoveTagHelper(
                            "Foo,   Bar",
                            "Foo,   Bar",
                            "Foo",
                            "Bar")
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void RemoveTagHelperDirective_RequiresValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.FormatParseError_DirectiveMustHaveValue(SyntaxConstants.CSharp.RemoveTagHelperKeyword),
                    absoluteIndex: 1, lineIndex: 0, columnIndex: 1, length: 15),
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText(string.Empty),
                    new SourceLocation(17, 0, 17),
                    length: 1),
            };

            // Act & Assert
            ParseBlockTest("@removeTagHelper ",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.EmptyCSharp()
                        .AsRemoveTagHelper(string.Empty, string.Empty, legacyErrors: expectedErrors)
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void RemoveTagHelperDirective_StartQuoteRequiresDoubleQuotesAroundValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.ParseError_Unterminated_String_Literal,
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 1),
                new RazorError(
                    LegacyResources.FormatParseError_IncompleteQuotesAroundDirective(SyntaxConstants.CSharp.RemoveTagHelperKeyword),
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 4),
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("\"Foo"),
                    new SourceLocation(17, 0, 17),
                    length: 4),
            };

            // Act & Assert
            ParseBlockTest("@removeTagHelper \"Foo",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"Foo")
                        .AsRemoveTagHelper("\"Foo", "\"Foo", legacyErrors: expectedErrors)));
        }

        [Fact]
        public void RemoveTagHelperDirective_EndQuoteRequiresDoubleQuotesAroundValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.ParseError_Unterminated_String_Literal,
                    absoluteIndex: 20, lineIndex: 0, columnIndex: 20, length: 1),
                new RazorError(
                    LegacyResources.FormatParseError_IncompleteQuotesAroundDirective(SyntaxConstants.CSharp.RemoveTagHelperKeyword),
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 4),
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("Foo\""),
                    new SourceLocation(17, 0, 17),
                    length: 4),
            };

            // Act & Assert
            ParseBlockTest("@removeTagHelper Foo\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.RemoveTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo\"")
                        .AsRemoveTagHelper("Foo\"", "Foo\"", legacyErrors: expectedErrors)
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void AddTagHelperDirective_NoValue_Invalid()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText(string.Empty),
                    new SourceLocation(15, 0, 15),
                    length: 1)
            };

            ParseBlockTest("@addTagHelper \"\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"\"")
                        .AsAddTagHelper(
                            "\"\"",
                            string.Empty,
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void AddTagHelperDirective_InvalidLookupText_AddsError()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("Foo"),
                    new SourceLocation(14, 0, 14),
                    length: 3)
            };

            ParseBlockTest("@addTagHelper Foo",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo")
                        .AsAddTagHelper(
                            "Foo",
                            "Foo",
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void AddTagHelperDirective_WithQuotes_InvalidLookupText_AddsError()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("Foo"),
                    new SourceLocation(15, 0, 15),
                    length: 3)
            };

            ParseBlockTest("@addTagHelper \"Foo\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"Foo\"")
                        .AsAddTagHelper(
                            "\"Foo\"",
                            "Foo",
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void AddTagHelperDirective_SingleQuotes_AddsError()
        {
            var expectedErrors = new[]
            {
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("'*, Foo'"),
                    new SourceLocation(14, 0, 14),
                    length: 8)
            };

            ParseBlockTest("@addTagHelper '*, Foo'",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory.MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false)
                           .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("'*, Foo'")
                        .AsAddTagHelper(
                            "'*, Foo'",
                            "'*, Foo'",
                            legacyErrors: expectedErrors)));
        }

        [Fact]
        public void AddTagHelperDirective_SupportsSpaces()
        {
            ParseBlockTest("@addTagHelper     Foo,   Bar    ",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, "     ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo,   Bar    ")
                        .AsAddTagHelper(
                            "Foo,   Bar",
                            "Foo,   Bar",
                            "Foo",
                            "Bar")
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void AddTagHelperDirective_RequiresValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.FormatParseError_DirectiveMustHaveValue(SyntaxConstants.CSharp.AddTagHelperKeyword),
                    absoluteIndex: 1, lineIndex: 0, columnIndex: 1, length: 12),
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText(string.Empty),
                    new SourceLocation(14, 0, 14),
                    length: 1),
            };

            // Act & Assert
            ParseBlockTest("@addTagHelper ",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.EmptyCSharp()
                        .AsAddTagHelper(string.Empty, string.Empty, legacyErrors: expectedErrors)
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void AddTagHelperDirective_StartQuoteRequiresDoubleQuotesAroundValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.ParseError_Unterminated_String_Literal,
                    absoluteIndex: 14, lineIndex: 0, columnIndex: 14, length: 1),
                new RazorError(
                    LegacyResources.FormatParseError_IncompleteQuotesAroundDirective(SyntaxConstants.CSharp.AddTagHelperKeyword),
                    absoluteIndex: 14, lineIndex: 0, columnIndex: 14, length: 4),
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("\"Foo"),
                    new SourceLocation(14, 0, 14),
                    length: 4),
            };

            // Act & Assert
            ParseBlockTest("@addTagHelper \"Foo",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("\"Foo")
                        .AsAddTagHelper("\"Foo", "\"Foo", legacyErrors: expectedErrors)));
        }

        [Fact]
        public void AddTagHelperDirective_EndQuoteRequiresDoubleQuotesAroundValue()
        {
            // Arrange
            var expectedErrors = new[]
            {
                new RazorError(
                    LegacyResources.ParseError_Unterminated_String_Literal,
                    absoluteIndex: 17, lineIndex: 0, columnIndex: 17, length: 1),
                new RazorError(
                    LegacyResources.FormatParseError_IncompleteQuotesAroundDirective(SyntaxConstants.CSharp.AddTagHelperKeyword),
                    absoluteIndex: 14, lineIndex: 0, columnIndex: 14, length: 4),
                new RazorError(
                    Resources.FormatInvalidTagHelperLookupText("Foo\""),
                    new SourceLocation(14, 0, 14),
                    length: 4),
            };

            // Act & Assert
            ParseBlockTest("@addTagHelper Foo\"",
                new DirectiveBlock(
                    Factory.CodeTransition(),
                    Factory
                        .MetaCode(SyntaxConstants.CSharp.AddTagHelperKeyword)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory
                        .Span(SpanKindInternal.Markup, " ", markup: false)
                        .Accepts(AcceptedCharactersInternal.None),
                    Factory.Code("Foo\"")
                        .AsAddTagHelper("Foo\"", "Foo\"", legacyErrors: expectedErrors)
                        .Accepts(AcceptedCharactersInternal.AnyExceptNewline)));
        }

        [Fact]
        public void InheritsDirectiveSupportsArrays()
        {
            ParseDocumentTest(
                "@inherits string[[]][]",
                new[] { InheritsDirective.Directive, },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(InheritsDirective.Directive),
                    Factory.CodeTransition(),
                    Factory.MetaCode("inherits").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "string[[]][]", markup: false).AsDirectiveToken(InheritsDirective.Directive.Tokens.First())),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void InheritsDirectiveSupportsNestedGenerics()
        {
            ParseDocumentTest(
                "@inherits System.Web.Mvc.WebViewPage<IEnumerable<MvcApplication2.Models.RegisterModel>>",
                new[] { InheritsDirective.Directive, },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(InheritsDirective.Directive),
                        Factory.CodeTransition(),
                        Factory.MetaCode("inherits").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "System.Web.Mvc.WebViewPage<IEnumerable<MvcApplication2.Models.RegisterModel>>", markup: false)
                            .AsDirectiveToken(InheritsDirective.Directive.Tokens.First())),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void InheritsDirectiveSupportsTypeKeywords()
        {
            ParseDocumentTest(
                "@inherits string",
                new[] { InheritsDirective.Directive, },
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(InheritsDirective.Directive),
                        Factory.CodeTransition(),
                        Factory.MetaCode("inherits").Accepts(AcceptedCharactersInternal.None),
                        Factory.Span(SpanKindInternal.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.WhiteSpace),
                        Factory.Span(SpanKindInternal.Code, "string", markup: false)
                            .AsDirectiveToken(InheritsDirective.Directive.Tokens.First())),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void Parse_FunctionsDirective()
        {
            ParseCodeBlockTest(
                "@functions { foo(); bar(); }",
                new[] { FunctionsDirective.Directive, },
                new DirectiveBlock(new DirectiveChunkGenerator(FunctionsDirective.Directive),
                    Factory.CodeTransition(),
                    Factory.MetaCode("functions").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.AllWhiteSpace),
                    Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharactersInternal.None),
                    Factory.Code(" foo(); bar(); ").AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void EmptyFunctionsDirective()
        {
            ParseCodeBlockTest(
                "@functions { }",
                new[] { FunctionsDirective.Directive, },
                new DirectiveBlock(new DirectiveChunkGenerator(FunctionsDirective.Directive),
                    Factory.CodeTransition(),
                    Factory.MetaCode("functions").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.AllWhiteSpace),
                    Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharactersInternal.None),
                    Factory.Code(" ").AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void Parse_SectionDirective()
        {
            ParseCodeBlockTest(
                "@section Header { <p>F{o}o</p> }",
                new[] { SectionDirective.Directive, },
                new DirectiveBlock(new DirectiveChunkGenerator(SectionDirective.Directive),
                    Factory.CodeTransition(),
                    Factory.MetaCode("section").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "Header", CSharpSymbolType.Identifier)
                        .AsDirectiveToken(SectionDirective.Directive.Tokens.First()),
                    Factory.Span(SpanKindInternal.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharactersInternal.AllWhiteSpace),
                    Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharactersInternal.None),
                    new MarkupBlock(
                        Factory.Markup(" "),
                        new MarkupTagBlock(
                            Factory.Markup("<p>")),
                        Factory.Markup("F", "{", "o", "}", "o"),
                        new MarkupTagBlock(
                            Factory.Markup("</p>")),
                        Factory.Markup(" ")),
                    Factory.MetaCode("}")
                           .Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void OptionalDirectiveTokens_AreSkipped()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddOptionalStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom ",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace)));
        }

        [Fact]
        public void OptionalDirectiveTokens_WithSimpleTokens_AreParsed()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddOptionalStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"simple-value\"",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"simple-value\"", markup: false)
                        .AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void OptionalDirectiveTokens_WithBraces_AreParsed()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddOptionalStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"{formaction}?/{id}?\"",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"{formaction}?/{id}?\"", markup: false)
                        .AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void OptionalDirectiveTokens_WithMultipleOptionalTokens_AreParsed()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "custom",
                DirectiveKind.SingleLine,
                b => b.AddOptionalStringToken().AddOptionalTypeToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@custom \"{formaction}?/{id}?\" System.String",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("custom").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Markup, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "\"{formaction}?/{id}?\"", markup: false).AsDirectiveToken(descriptor.Tokens[0]),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "System.String", markup: false).AsDirectiveToken(descriptor.Tokens.Last())));
        }

        [Fact]
        public void OptionalMemberTokens_WithMissingMember_IsParsed()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "TestDirective",
                DirectiveKind.SingleLine,
                b => b.AddOptionalMemberToken().AddOptionalStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@TestDirective ",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("TestDirective").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, string.Empty, CSharpSymbolType.Unknown)
                        .AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void OptionalMemberTokens_WithMemberSpecified_IsParsed()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "TestDirective",
                DirectiveKind.SingleLine,
                b => b.AddOptionalMemberToken().AddOptionalStringToken());

            // Act & Assert
            ParseCodeBlockTest(
                "@TestDirective PropertyName",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("TestDirective").Accepts(AcceptedCharactersInternal.None),
                    Factory.Span(SpanKindInternal.Code, " ", markup: false).Accepts(AcceptedCharactersInternal.WhiteSpace),
                    Factory.Span(SpanKindInternal.Code, "PropertyName", markup: false).AsDirectiveToken(descriptor.Tokens[0])));
        }

        [Fact]
        public void Directives_CanUseReservedWord_Class()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "class",
                DirectiveKind.SingleLine);

            // Act & Assert
            ParseCodeBlockTest(
                "@class",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("class").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void Directives_CanUseReservedWord_Namespace()
        {
            // Arrange
            var descriptor = DirectiveDescriptor.CreateDirective(
                "namespace",
                DirectiveKind.SingleLine);

            // Act & Assert
            ParseCodeBlockTest(
                "@namespace",
                new[] { descriptor },
                new DirectiveBlock(
                    new DirectiveChunkGenerator(descriptor),
                    Factory.CodeTransition(),
                    Factory.MetaCode("namespace").Accepts(AcceptedCharactersInternal.None)));
        }

        public static TheoryData InvalidTagHelperPrefixData
        {
            get
            {
                var directiveLocation = new SourceLocation(1, 2, 3);

                var invalidTagHelperPrefixValueError =
                    "Invalid tag helper directive '{0}' value. '{1}' is not allowed in prefix '{2}'.";

                return new TheoryData<string, SourceLocation, IEnumerable<RazorError>>
                {
                    {
                        "th ",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    ' ',
                                    "th "),
                                directiveLocation,
                                length: 3)
                        }
                    },
                    {
                        "th\t",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '\t',
                                    "th\t"),
                                directiveLocation,
                                length: 3)
                        }
                    },
                    {
                        "th" + Environment.NewLine,
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    Environment.NewLine[0],
                                    "th" + Environment.NewLine),
                                directiveLocation,
                                length: 2 + Environment.NewLine.Length)
                        }
                    },
                    {
                        " th ",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    ' ',
                                    " th "),
                                directiveLocation,
                                length: 4)
                        }
                    },
                    {
                        "@",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '@',
                                    "@"),
                                directiveLocation,
                                length: 1)
                        }
                    },
                    {
                        "t@h",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '@',
                                    "t@h"),
                                directiveLocation,
                                length: 3)
                        }
                    },
                    {
                        "!",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '!',
                                    "!"),
                                directiveLocation,
                                length: 1)
                        }
                    },
                    {
                        "!th",
                        directiveLocation,
                        new[]
                        {
                            new RazorError(
                                string.Format(
                                    invalidTagHelperPrefixValueError,
                                    SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                                    '!',
                                    "!th"),
                                directiveLocation,
                                length: 3)
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidTagHelperPrefixData))]
        public void ValidateTagHelperPrefix_ValidatesPrefix(
            string directiveText,
            SourceLocation directiveLocation,
            object expectedErrors)
        {
            // Arrange
            var expectedDiagnostics = ((IEnumerable<RazorError>)expectedErrors).Select(RazorDiagnostic.Create);
            var source = TestRazorSourceDocument.Create();
            var options = RazorParserOptions.CreateDefault();
            var context = new ParserContext(source, options);

            var parser = new CSharpCodeParser(context);
            var diagnostics = new List<RazorDiagnostic>();

            // Act
            parser.ValidateTagHelperPrefix(directiveText, directiveLocation, diagnostics);

            // Assert
            Assert.Equal(expectedDiagnostics, diagnostics);
        }

        [Theory]
        [InlineData("foo,assemblyName", 4)]
        [InlineData("foo, assemblyName", 5)]
        [InlineData("   foo, assemblyName", 8)]
        [InlineData("   foo   , assemblyName", 11)]
        [InlineData("foo,    assemblyName", 8)]
        [InlineData("   foo   ,    assemblyName   ", 14)]
        public void ParseAddOrRemoveDirective_CalculatesAssemblyLocationInLookupText(string text, int assemblyLocation)
        {
            // Arrange
            var source = TestRazorSourceDocument.Create();
            var options = RazorParserOptions.CreateDefault();
            var context = new ParserContext(source, options);

            var parser = new CSharpCodeParser(context);

            var directive = new CSharpCodeParser.ParsedDirective()
            {
                DirectiveText = text,
            };

            var diagnostics = new List<RazorDiagnostic>();
            var expected = new SourceLocation(assemblyLocation, 0, assemblyLocation);

            // Act
            var result = parser.ParseAddOrRemoveDirective(directive, SourceLocation.Zero, diagnostics);

            // Assert
            Assert.Empty(diagnostics);
            Assert.Equal("foo", result.TypePattern);
            Assert.Equal("assemblyName", result.AssemblyName);
        }

        [Theory]
        [InlineData("", 1)]
        [InlineData("*,", 2)]
        [InlineData("?,", 2)]
        [InlineData(",", 1)]
        [InlineData(",,,", 3)]
        [InlineData("First, ", 7)]
        [InlineData("First , ", 8)]
        [InlineData(" ,Second", 8)]
        [InlineData(" , Second", 9)]
        [InlineData("SomeType,", 9)]
        [InlineData("SomeAssembly", 12)]
        [InlineData("First,Second,Third", 18)]
        public void ParseAddOrRemoveDirective_CreatesErrorIfInvalidLookupText_DoesNotThrow(string directiveText, int errorLength)
        {
            // Arrange
            var source = TestRazorSourceDocument.Create();
            var options = RazorParserOptions.CreateDefault();
            var context = new ParserContext(source, options);

            var parser = new CSharpCodeParser(context);

            var expectedErrorMessage = string.Format(
                "Invalid tag helper directive look up text '{0}'. The correct look up text " +
                "format is: \"name, assemblyName\".",
                directiveText);

            var directive = new CSharpCodeParser.ParsedDirective()
            {
                DirectiveText = directiveText
            };

            var diagnostics = new List<RazorDiagnostic>();
            var expectedError = RazorDiagnostic.Create(
                new RazorError(
                    expectedErrorMessage,
                    new SourceLocation(1, 2, 3),
                    errorLength));

            // Act
            var result = parser.ParseAddOrRemoveDirective(directive, new SourceLocation(1, 2, 3), diagnostics);

            // Assert
            Assert.Same(directive, result);

            var error = Assert.Single(diagnostics);
            Assert.Equal(expectedError, error);
        }

        internal virtual void ParseCodeBlockTest(
            string document,
            IEnumerable<DirectiveDescriptor> descriptors,
            Block expected,
            params RazorError[] expectedErrors)
        {
            var result = ParseCodeBlock(document, descriptors, designTime: false);

            EvaluateResults(result, expected, expectedErrors.Select(error => RazorDiagnostic.Create(error)).ToList());
        }
    }
}
