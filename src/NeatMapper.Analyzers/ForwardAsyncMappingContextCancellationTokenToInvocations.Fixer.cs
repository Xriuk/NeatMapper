using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Analyzers {
	/// <remarks>Adapted from https://github.com/dotnet/roslyn-analyzers/blob/bc8aca08e3d3b12f22ec2fb850d9a13f121ed2b2/src/NetAnalyzers/Core/Microsoft.NetCore.Analyzers/Runtime/ForwardCancellationTokenToInvocations.Fixer.cs</remarks>
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public sealed class ForwardAsyncMappingContextCancellationTokenToInvocationsFixer : CodeFixProvider {
		private class TypeNameVisitor : SymbolVisitor<TypeSyntax> {
			public static TypeSyntax GetTypeSyntaxForSymbol(INamespaceOrTypeSymbol symbol) {
				return symbol.Accept(new TypeNameVisitor()).WithAdditionalAnnotations(Simplifier.Annotation);
			}

			public override TypeSyntax DefaultVisit(ISymbol symbol)
				=> throw new NotImplementedException();

			public override TypeSyntax VisitAlias(IAliasSymbol symbol) {
				return AddInformationTo(ToIdentifierName(symbol.Name));
			}

			public override TypeSyntax VisitDynamicType(IDynamicTypeSymbol symbol) {
				return AddInformationTo(SyntaxFactory.IdentifierName("dynamic"));
			}

			public override TypeSyntax VisitNamedType(INamedTypeSymbol symbol) {
				/*if (TryCreateNativeIntegerType(symbol, out var typeSyntax))
					return typeSyntax;*/

				var typeSyntax = CreateSimpleTypeSyntax(symbol);
				if (!(typeSyntax is SimpleNameSyntax simpleNameSyntax))
					return typeSyntax;

				if (symbol.ContainingType != null) {
					if (symbol.ContainingType.TypeKind != TypeKind.Submission) {
						var containingTypeSyntax = symbol.ContainingType.Accept(this);
						if (containingTypeSyntax is NameSyntax name) {
							typeSyntax = AddInformationTo(
								SyntaxFactory.QualifiedName(name, simpleNameSyntax));
						}
						else {
							typeSyntax = AddInformationTo(simpleNameSyntax);
						}
					}
				}
				else if (symbol.ContainingNamespace != null) {
					if (symbol.ContainingNamespace.IsGlobalNamespace) {
						if (symbol.TypeKind != TypeKind.Error) {
							typeSyntax = AddGlobalAlias(simpleNameSyntax);
						}
					}
					else {
						var container = symbol.ContainingNamespace.Accept(this);
						typeSyntax = AddInformationTo(SyntaxFactory.QualifiedName(
							(NameSyntax)container,
							simpleNameSyntax));
					}
				}

				/*if (symbol.NullableAnnotation == NullableAnnotation.Annotated &&
					!symbol.IsValueType) {
					typeSyntax = AddInformationTo(NullableType(typeSyntax));
				}*/

				return typeSyntax;
			}

			public override TypeSyntax VisitNamespace(INamespaceSymbol symbol) {
				var syntax = AddInformationTo(ToIdentifierName(symbol.Name));
				if (symbol.ContainingNamespace == null) {
					return syntax;
				}

				if (symbol.ContainingNamespace.IsGlobalNamespace) {
					return AddGlobalAlias(syntax);
				}
				else {
					var container = symbol.ContainingNamespace.Accept(this);
					return AddInformationTo(SyntaxFactory.QualifiedName(
						(NameSyntax)container,
						syntax));
				}
			}

			public override TypeSyntax VisitTypeParameter(ITypeParameterSymbol symbol) {
				TypeSyntax typeSyntax = AddInformationTo(ToIdentifierName(symbol.Name));
				/*if (symbol.NullableAnnotation == NullableAnnotation.Annotated)
					typeSyntax = AddInformationTo(NullableType(typeSyntax));*/

				return typeSyntax;
			}

			private TypeSyntax CreateSimpleTypeSyntax(INamedTypeSymbol symbol) {
				if (symbol.IsTupleType && symbol.TupleUnderlyingType != null && !SymbolEqualityComparer.Default.Equals(symbol, symbol.TupleUnderlyingType)) {
					return CreateSimpleTypeSyntax(symbol.TupleUnderlyingType);
				}

				if (string.IsNullOrEmpty(symbol.Name) || symbol.IsAnonymousType) {
					return CreateSystemObject();
				}

				if (symbol.TypeParameters.Length == 0) {
					if (symbol.TypeKind == TypeKind.Error && symbol.Name == "var") {
						return CreateSystemObject();
					}

					return ToIdentifierName(symbol.Name);
				}

				var typeArguments = symbol.IsUnboundGenericType
					? Enumerable.Repeat((TypeSyntax)SyntaxFactory.OmittedTypeArgument(), symbol.TypeArguments.Length)
					: symbol.TypeArguments.Select(GetTypeSyntaxForSymbol);

				return SyntaxFactory.GenericName(
					ToIdentifierToken(symbol.Name),
					SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));
			}

			private static QualifiedNameSyntax CreateSystemObject() {
				return SyntaxFactory.QualifiedName(
					SyntaxFactory.AliasQualifiedName(
						CreateGlobalIdentifier(),
						SyntaxFactory.IdentifierName("System")),
					SyntaxFactory.IdentifierName("Object"));
			}

			private static TTypeSyntax AddInformationTo<TTypeSyntax>(TTypeSyntax syntax)
				where TTypeSyntax : TypeSyntax {
				syntax = syntax.WithLeadingTrivia(SyntaxFactory.ElasticMarker).WithTrailingTrivia(SyntaxFactory.ElasticMarker);
				return syntax;
			}

			/// <summary>
			/// We always unilaterally add "global::" to all named types/namespaces.  This
			/// will then be trimmed off if possible by the simplifier
			/// </summary>
			private static TypeSyntax AddGlobalAlias(SimpleNameSyntax syntax) {
				return AddInformationTo(SyntaxFactory.AliasQualifiedName(CreateGlobalIdentifier(), syntax));
			}

			private static IdentifierNameSyntax ToIdentifierName(string identifier)
				=> SyntaxFactory.IdentifierName(ToIdentifierToken(identifier));

			private static IdentifierNameSyntax CreateGlobalIdentifier()
				=> SyntaxFactory.IdentifierName(SyntaxFactory.Token(SyntaxKind.GlobalKeyword));

			/*private static bool TryCreateNativeIntegerType(INamedTypeSymbol symbol, out TypeSyntax syntax) {
				if (symbol.IsNativeIntegerType) {
					syntax = SyntaxFactory.IdentifierName(symbol.SpecialType == SpecialType.System_IntPtr ? "nint" : "nuint");
					return true;
				}

				syntax = null;
				return false;
			}*/

			private static SyntaxToken ToIdentifierToken(string identifier) {
				var escaped = EscapeIdentifier(identifier);

				if (escaped.Length == 0 || escaped[0] != '@') {
					return SyntaxFactory.Identifier(escaped);
				}

				var unescaped = identifier.StartsWith("@", StringComparison.Ordinal)
					? identifier.Substring(1)
					: identifier;

				var token = SyntaxFactory.Identifier(
					default, SyntaxKind.None, "@" + unescaped, unescaped, default);

				if (!identifier.StartsWith("@", StringComparison.Ordinal)) {
					token = token.WithAdditionalAnnotations(Simplifier.Annotation);
				}

				return token;
			}

			private static string EscapeIdentifier(string identifier) {
				var nullIndex = identifier.IndexOf('\0');
				if (nullIndex >= 0) {
					identifier = identifier.Substring(0, nullIndex);
				}

				var needsEscaping = SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None;

				return needsEscaping ? "@" + identifier : identifier;
			}
		}


		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(NeatMapperRules.ForwardAsyncMappingContextCancellationTokenToInvocations);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		private static async ValueTask<SyntaxNode> GetRequiredSyntaxRootAsync(Document document, CancellationToken cancellationToken) {
			if (document.TryGetSyntaxRoot(out var root))
				return root;

			root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			return root ?? throw new InvalidOperationException("SyntaxTree is required to accomplish the task but is not supported by document");
		}

		private static async ValueTask<SemanticModel> GetRequiredSemanticModelAsync(Document document, CancellationToken cancellationToken) {
			if (document.TryGetSemanticModel(out var semanticModel))
				return semanticModel;

			semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			return semanticModel ?? throw new InvalidOperationException("SyntaxTree is required to accomplish the task but is not supported by document");
		}

		private bool TryGetInvocation(
			SemanticModel model,
			SyntaxNode node,
			CancellationToken ct,
			out IInvocationOperation invocation) {
			// If the method was invoked using nullability for the case of attempting to dereference a possibly null reference,
			// then the node.Parent.Parent is the actual invocation (and it will contain the dot as well)

			var operation = node.Parent.IsKind(SyntaxKind.MemberBindingExpression)
				? model.GetOperation(node.Parent.Parent, ct)
				: model.GetOperation(node.Parent, ct);

			invocation = operation as IInvocationOperation;

			return invocation != null;
		}

		private bool TryGetExpressionAndArguments(
			SyntaxNode invocationNode,
			out SyntaxNode expression,
			out ImmutableArray<ArgumentSyntax> arguments) {

			if (invocationNode is InvocationExpressionSyntax invocationExpression) {
				expression = invocationExpression.Expression;
				arguments = invocationExpression.ArgumentList.Arguments.ToImmutableArray();
				return true;
			}

			expression = null;
			arguments = ImmutableArray<ArgumentSyntax>.Empty;
			return false;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			Document doc = context.Document;
			CancellationToken ct = context.CancellationToken;
			SyntaxNode root = await GetRequiredSyntaxRootAsync(doc, ct).ConfigureAwait(false);

			if (!(root.FindNode(context.Span, getInnermostNodeForTie: true) is SyntaxNode node)) {
				return;
			}

			SemanticModel model = await GetRequiredSemanticModelAsync(doc, ct).ConfigureAwait(false);

			// The analyzer created the diagnostic on the IdentifierNameSyntax, and the parent is the actual invocation
			if (!TryGetInvocation(model, node, ct, out IInvocationOperation invocation)) {
				return;
			}

			ImmutableDictionary<string, string> properties = context.Diagnostics[0].Properties;

			if (!properties.TryGetValue(ForwardAsyncMappingContextCancellationTokenToInvocationsAnalyzer.ShouldFix, out var shouldFix) ||
				string.IsNullOrEmpty(shouldFix) ||
				shouldFix.Equals("0", StringComparison.InvariantCultureIgnoreCase)) {
				return;
			}

			// The name that identifies the object that is to be passed
			if (!properties.TryGetValue(ForwardAsyncMappingContextCancellationTokenToInvocationsAnalyzer.ArgumentName, out var argumentName) ||
				string.IsNullOrEmpty(argumentName)) {
				return;
			}

			// If the invocation requires the token to be passed with a name, use this
			if (!properties.TryGetValue(ForwardAsyncMappingContextCancellationTokenToInvocationsAnalyzer.ParameterName, out var parameterName)) {
				return;
			}

			string title = ForwardAsyncMappingContextCancellationTokenToInvocationsAnalyzer.Title;

			if (!TryGetExpressionAndArguments(invocation.Syntax, out SyntaxNode expression, out ImmutableArray<ArgumentSyntax> newArguments)) {
				return;
			}

			var paramsArrayType = invocation.Arguments.SingleOrDefault(a => a.ArgumentKind == ArgumentKind.ParamArray)?.Value.Type as IArrayTypeSymbol;
			Task<Document> CreateChangedDocumentAsync(CancellationToken _) {
				SyntaxNode newRoot = TryGenerateNewDocumentRoot(doc, root, invocation, argumentName, parameterName, expression, newArguments, paramsArrayType);
				Document newDocument = doc.WithSyntaxRoot(newRoot);
				return Task.FromResult(newDocument);
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					CreateChangedDocumentAsync,
					equivalenceKey: title),
				context.Diagnostics);
		}

		private SyntaxNode TryGenerateNewDocumentRoot(
			Document doc,
			SyntaxNode root,
			IInvocationOperation invocation,
			string invocationTokenArgumentName,
			string ancestorContextParameterName,
			SyntaxNode expression,
			ImmutableArray<ArgumentSyntax> currentArguments,
			IArrayTypeSymbol paramsArrayType) {

			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

			ImmutableArray<SyntaxNode> newArguments;
			if (paramsArrayType != null) {
				// current callsite is a params array, we need to wrap all these arguments to preserve semantics
				var typeSyntax = TypeNameVisitor.GetTypeSyntaxForSymbol(paramsArrayType.ElementType);
				var expressions = currentArguments.Select(x => x.Expression);
				newArguments = ImmutableArray.Create(generator.ArrayCreationExpression(typeSyntax, expressions));
			}
			else {
				// not a params array just pass the existing arguments along
				newArguments = currentArguments.CastArray<SyntaxNode>();
			}

			SyntaxNode identifier = generator.MemberAccessExpression(generator.IdentifierName(invocationTokenArgumentName), "CancellationToken");
			SyntaxNode asyncMappingContextArgument;
			if (!string.IsNullOrEmpty(ancestorContextParameterName)) {
				asyncMappingContextArgument = generator.Argument(ancestorContextParameterName, RefKind.None, identifier);
			}
			else {
				asyncMappingContextArgument = generator.Argument(identifier);
			}

			newArguments = newArguments.Add(asyncMappingContextArgument);

			// Insert the new arguments to the new invocation
			SyntaxNode newInvocationWithArguments = generator.InvocationExpression(expression, newArguments).WithTriviaFrom(invocation.Syntax);

			return generator.ReplaceNode(root, invocation.Syntax, newInvocationWithArguments);
		}
	}
}
