using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations;
using Analyzer.Utilities;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace NeatMapper.Analyzers {
	/// <summary>
	/// NEATMAPPER0001: Analyzer which detects when an async method inside a IAsyncNewMap or IAsyncMergeMap
	/// does not have the AsyncMappingContext.CancellationToken forwarded to it.
	/// </summary>
	/// <remarks>Copied from https://github.com/dotnet/roslyn-analyzers/blob/8dccccec1ce3bd2fb532ec77d7e092ab9d684db7/src/NetAnalyzers/Core/Microsoft.NetCore.Analyzers/Runtime/ForwardCancellationTokenToInvocations.Analyzer.cs</remarks>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class IAsyncNewMapForwardCancellationTokenToInvocationsAnalyzer : DiagnosticAnalyzer {
		private static readonly DiagnosticDescriptor Descriptor =
			new DiagnosticDescriptor(
				NeatMapperRules.IAsyncNewMapForwardCancellationTokenToInvocations,
				new LocalizableResourceString(nameof(Resources.IAsyncNewMapForwardCancellationTokenToInvocationsTitle), Resources.ResourceManager, typeof(Resources)),
				new LocalizableResourceString(nameof(Resources.IAsyncNewMapForwardCancellationTokenToInvocationsMessage), Resources.ResourceManager, typeof(Resources)),
				NeatMapperCategories.Reliability,
				DiagnosticSeverity.Info,
				true,
				new LocalizableResourceString(nameof(Resources.IAsyncNewMapForwardCancellationTokenToInvocationsDescription), Resources.ResourceManager, typeof(Resources)),
				"https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2016");

		internal const string ShouldFix = "ShouldFix";
		internal const string ArgumentName = "ArgumentName";
		internal const string ParameterName = "ParameterName";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
			ImmutableArray.Create(Descriptor);

		override public void Initialize(AnalysisContext context) {
			/*if (!Debugger.IsAttached) {
				Debugger.Launch();
			}*/

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(AnalyzeCompilationStart);
		}


		private void AnalyzeCompilationStart(CompilationStartAnalysisContext context) {
			var typeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
			if (!typeProvider.TryGetOrCreateTypeByMetadataName(typeof(CancellationToken).FullName, out INamedTypeSymbol cancellationTokenType)
				|| !typeProvider.TryGetOrCreateTypeByMetadataName(typeof(ObsoleteAttribute).FullName, out INamedTypeSymbol obsoleteAttribute)
				|| !typeProvider.TryGetOrCreateTypeByMetadataName("NeatMapper.IAsyncNewMap`2", out INamedTypeSymbol interfaceType)
				|| !typeProvider.TryGetOrCreateTypeByMetadataName("NeatMapper.AsyncMappingContext", out INamedTypeSymbol contextType)) {

				return;
			}

			interfaceType = interfaceType.ConstructUnboundGenericType();

			// We don't care if these symbols are not defined in our compilation. They are used to special case the Task<T> <-> ValueTask<T> logic
			typeProvider.TryGetOrCreateTypeByMetadataName(typeof(Task<>).Namespace + typeof(Task<>).Name, out INamedTypeSymbol genericTask);
			typeProvider.TryGetOrCreateTypeByMetadataName(typeof(ValueTask<>).Namespace + typeof(ValueTask<>).Name, out INamedTypeSymbol genericValueTask);

			context.RegisterOperationAction(context1 => {
				IInvocationOperation invocation = (IInvocationOperation)context1.Operation;

				// DEV: check for type implementation
				if (!(context1.ContainingSymbol is IMethodSymbol containingMethod)) {
					return;
				}

				if (!ShouldDiagnose(
					context1.Compilation,
					invocation,
					containingMethod,
					cancellationTokenType,
					genericTask,
					genericValueTask,
					obsoleteAttribute,
					interfaceType,
					contextType,
					out int shouldFix,
					out string cancellationTokenArgumentName,
					out string invocationTokenParameterName)) {
					return;
				}

				// Underline only the method name, if possible
				SyntaxNode nodeToDiagnose = GetInvocationMethodNameNode(context1.Operation.Syntax) ?? context1.Operation.Syntax;

				ImmutableDictionary<string, string>.Builder properties = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
				properties.Add(ShouldFix, $"{shouldFix}");
				properties.Add(ArgumentName, cancellationTokenArgumentName); // The new argument to pass to the invocation, can contain "." if nested
				properties.Add(ParameterName, invocationTokenParameterName); // If the passed argument should be named, then this will be non-null

				context1.ReportDiagnostic(
					Diagnostic.Create(Descriptor, nodeToDiagnose.GetLocation(), properties.ToImmutable(),
						cancellationTokenArgumentName, invocation.TargetMethod.Name ));
			},
			OperationKind.Invocation);
		}

		private SyntaxNode GetInvocationMethodNameNode(SyntaxNode invocationNode) {
			if (invocationNode is InvocationExpressionSyntax invocationExpression) {
				if (invocationExpression.Expression is MemberBindingExpressionSyntax memberBindingExpression) {
					// When using nullability features, specifically attempting to dereference possible null references,
					// the dot becomes part of the member invocation expression, so we need to return just the name,
					// so that the diagnostic gets properly returned in the method name only.
					return memberBindingExpression.Name;
				}

				return invocationExpression.Expression;
			}

			return null;
		}
		private bool ArgumentsImplicitOrNamed(INamedTypeSymbol cancellationTokenType, ImmutableArray<IArgumentOperation> arguments) {
			return arguments.Any(a =>
				(a.IsImplicit && a.Parameter != null && !a.Parameter.Type.Equals(cancellationTokenType)) ||
				(a.Syntax is ArgumentSyntax argumentNode && argumentNode.NameColon != null));
		}

		// Determines if an invocation should trigger a diagnostic for this rule or not.
		private bool ShouldDiagnose(
			Compilation compilation,
			IInvocationOperation invocation,
			IMethodSymbol containingSymbol,
			INamedTypeSymbol cancellationTokenType,
			INamedTypeSymbol genericTask,
			INamedTypeSymbol genericValueTask,
			INamedTypeSymbol obsoleteAttribute,
			INamedTypeSymbol interfaceType,
			INamedTypeSymbol contextType,
			out int shouldFix, out string ancestorTokenParameterName, out string invocationTokenParameterName) {

			shouldFix = 1;
			ancestorTokenParameterName = null;
			invocationTokenParameterName = null;

			IMethodSymbol method = invocation.TargetMethod;

			// Verify that the current invocation is not passing an explicit token already
			if (AnyArgument(
				invocation.Arguments,
				(a, cancellationTokenType1) => cancellationTokenType1.Equals(a.Parameter?.Type) && !a.IsImplicit,
				cancellationTokenType)) {
				return false;
			}

			IMethodSymbol overload = null;
			// Check if the invocation's method has either an optional implicit ct at the end not being used, or a params ct parameter at the end not being used
			if (InvocationMethodTakesAToken(method, invocation.Arguments, cancellationTokenType)) {
				if (ArgumentsImplicitOrNamed(cancellationTokenType, invocation.Arguments)) {
					invocationTokenParameterName = method.Parameters[method.Parameters.Length-1].Name;
				}
			}
			// or an overload that takes a ct at the end
			else if (MethodHasCancellationTokenOverload(compilation, method, cancellationTokenType, genericTask, genericValueTask, obsoleteAttribute, out overload)) {
				if (ArgumentsImplicitOrNamed(cancellationTokenType, invocation.Arguments)) {
					invocationTokenParameterName = overload.Parameters[method.Parameters.Length - 1].Name;
				}
			}
			else {
				return false;
			}

			// Check if there is an ancestor method that has a ct that we can pass to the invocation
			if (!TryGetClosestAncestorThatTakesAToken(
				invocation, containingSymbol, interfaceType,
				contextType, out shouldFix,
				out IMethodSymbol ancestor, out ancestorTokenParameterName)) {

				return false;
			}

			// Finally, if the ct is in an overload method, but adding the ancestor's ct to the current
			// invocation would cause the new signature to become a recursive call, avoid creating a diagnostic
			if (overload != null && overload == ancestor) {
				ancestorTokenParameterName = null;
				return false;
			}

			return true;
		}

		// Try to find the most immediate IAsyncNewMap. Returns true.
		// If none is found, return the context containing symbol. Returns false.
		private static bool TryGetClosestAncestorThatTakesAToken(
			IInvocationOperation invocation,
			IMethodSymbol containingSymbol,
			INamedTypeSymbol interfaceType,
			INamedTypeSymbol contextType,
			out int shouldFix,
			out IMethodSymbol ancestor,
			out string cancellationTokenParameterName) {

			shouldFix = 1;
			IOperation currentOperation = invocation.Parent;
			while (currentOperation != null) {
				ancestor = null;

				if (currentOperation.Kind == OperationKind.AnonymousFunction) {
					ancestor = ((IAnonymousFunctionOperation)currentOperation).Symbol;
				}
				else if (currentOperation.Kind == OperationKind.LocalFunction) {
					ancestor = ((ILocalFunctionOperation)currentOperation).Symbol;
				}

				// When the current ancestor is not IAsyncNewMap, will continue with the next ancestor
				if (ancestor != null) {
					if (TryGetTokenParamName(ancestor, interfaceType, contextType, out cancellationTokenParameterName)) {
						return true;
					}
					// If no token param was found in the previous check, return false if the current operation is an anonymous function,
					// we don't want to keep checking the superior ancestors because the ct may be unrelated
					if (currentOperation.Kind == OperationKind.AnonymousFunction) {
						return false;
					}

					// If the current operation is a local static function, and is not passing a ct, but the parent is, then the
					// ct cannot be passed to the inner invocations of the static local method, but we want to continue trying
					// to find the ancestor method passing a ct so that we still trigger a diagnostic, we just won't offer a fix
					if (currentOperation.Kind == OperationKind.LocalFunction && ancestor.IsStatic) {
						shouldFix = 0;
					}
				}

				currentOperation = currentOperation.Parent;
			}

			// Last resort: fallback to the containing symbol
			ancestor = containingSymbol;
			return TryGetTokenParamName(ancestor, interfaceType, contextType, out cancellationTokenParameterName);
		}

		// https://stackoverflow.com/a/65068997/2672235
		public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(ISymbol symbol) {
			if (symbol.Kind != SymbolKind.Method && symbol.Kind != SymbolKind.Property && symbol.Kind != SymbolKind.Event)
				return ImmutableArray<ISymbol>.Empty;

			var containingType = symbol.ContainingType;
			var query = from iface in containingType.AllInterfaces
						from interfaceMember in iface.GetMembers()
						let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
						where symbol.Equals(impl)
						select interfaceMember;
			return query.ToImmutableArray();
		}

		private static bool TryGetTokenParamName(
			IMethodSymbol methodDeclaration,
			INamedTypeSymbol interfaceType,
			INamedTypeSymbol contextType,
			out string cancellationTokenParameterName) {

			cancellationTokenParameterName = null;

			// Check if the method is an implementation (explicit or implicit) of IAsyncNewMap
			var implementations = ExplicitOrImplicitInterfaceImplementations(methodDeclaration);
			if(implementations.Length == 0 || !implementations.Any(i => i.ContainingType.IsGenericType && i.ContainingType.ConstructUnboundGenericType().Equals(interfaceType)))
				return false;

			// Check if the method has a AsyncMappingContext parameter (it should, but we'll make it future proof
			if(methodDeclaration.Parameters.Count(x => x.Type.Equals(contextType)) == 1) {
				cancellationTokenParameterName = methodDeclaration.Parameters.First(p => p.Type.Equals(contextType)).Name;
				return true;
			}

			return false;
		}

		// Checks if the invocation has an optional ct argument at the end or a params ct array at the end.
		private static bool InvocationMethodTakesAToken(
			IMethodSymbol method,
			ImmutableArray<IArgumentOperation> arguments,
			INamedTypeSymbol cancellationTokenType) {

			return
				method.Parameters[method.Parameters.Length-1] is IParameterSymbol lastParameter &&
				(InvocationIgnoresOptionalCancellationToken(lastParameter, arguments, cancellationTokenType) ||
				InvocationIsUsingParamsCancellationToken(lastParameter, arguments, cancellationTokenType));
		}

		// Checks if the arguments enumerable has any elements that satisfy the provided condition,
		// starting the lookup with the last element since tokens tend to be added as the last argument.
		private static bool AnyArgument<TArg>(ImmutableArray<IArgumentOperation> arguments, Func<IArgumentOperation, TArg, bool> predicate, TArg arg) {
			for (int i = arguments.Length - 1; i >= 0; i--) {
				if (predicate(arguments[i], arg)) {
					return true;
				}
			}

			return false;
		}

		// Check if the currently used overload is the one that takes the ct, but is utilizing the default value offered in the method signature.
		// We want to offer a diagnostic for this case, so the user explicitly passes the ancestor's ct.
		private static bool InvocationIgnoresOptionalCancellationToken(
			IParameterSymbol lastParameter,
			ImmutableArray<IArgumentOperation> arguments,
			INamedTypeSymbol cancellationTokenType) {


			if (lastParameter.Type.Equals(cancellationTokenType) &&
				lastParameter.IsOptional) // Has a default value being used
			{
				// Find out if the ct argument is using the default value
				// Need to check among all arguments in case the user is passing them named and unordered (despite the ct being defined as the last parameter)
				return AnyArgument(
					arguments,
					(a, cancellationTokenType1) => a.Parameter != null && a.Parameter.Type.Equals(cancellationTokenType1) && a.ArgumentKind == ArgumentKind.DefaultValue,
					cancellationTokenType);
			}

			return false;
		}

		// Checks if the method has a `params CancellationToken[]` argument in the last position and ensure no ct is being passed.
		private static bool InvocationIsUsingParamsCancellationToken(
			IParameterSymbol lastParameter,
			ImmutableArray<IArgumentOperation> arguments,
			INamedTypeSymbol cancellationTokenType) {

			if (lastParameter.IsParams &&
				   lastParameter.Type is IArrayTypeSymbol arrayTypeSymbol &&
				   arrayTypeSymbol.ElementType.Equals(cancellationTokenType)) {
				IArgumentOperation paramsArgument = arguments.FirstOrDefault(a => a.ArgumentKind == ArgumentKind.ParamArray);
				if (paramsArgument?.Value is IArrayCreationOperation arrayOperation) {
					// Do not offer a diagnostic if the user already passed a ct to the params
					return arrayOperation.Initializer.ElementValues.IsEmpty;
				}
			}

			return false;
		}

		// Check if there's a method overload with the same parameters as this one, in the same order, plus a ct at the end.
		private static bool MethodHasCancellationTokenOverload(
			Compilation compilation,
			IMethodSymbol method,
			ITypeSymbol cancellationTokenType,
			INamedTypeSymbol genericTask,
			INamedTypeSymbol genericValueTask,
			INamedTypeSymbol obsoleteAttribute,
			out IMethodSymbol overload) {

			overload = method.ContainingType
				.GetMembers(method.Name)
				.OfType<IMethodSymbol>()
				.FirstOrDefault(methodToCompare => !methodToCompare.GetAttributes().Where(a => a.AttributeClass != null && SymbolEqualityComparer.Default.Equals(a.AttributeClass, obsoleteAttribute)).Any()
												   && HasSameParametersPlusCancellationToken(compilation, cancellationTokenType, genericTask, genericValueTask, method, methodToCompare));

			return overload != null;

			// Checks if the parameters of the two passed methods only differ in a ct.
			bool HasSameParametersPlusCancellationToken(
				Compilation compilation1,
				ITypeSymbol cancellationTokenType1,
				INamedTypeSymbol genericTask1,
				INamedTypeSymbol genericValueTask1,
				IMethodSymbol originalMethod,
				IMethodSymbol methodToCompare) {
				// Avoid comparing to itself, or when there are no parameters, or when the last parameter is not a ct
				if (originalMethod.Equals(methodToCompare) ||
					methodToCompare.Parameters.Count(p => p.Type.Equals(cancellationTokenType1)) != 1 ||
					!methodToCompare.Parameters[methodToCompare.Parameters.Length-1].Type.Equals(cancellationTokenType1)) {
					return false;
				}

				IMethodSymbol originalMethodWithAllParameters = (originalMethod.ReducedFrom ?? originalMethod).OriginalDefinition;
				IMethodSymbol methodToCompareWithAllParameters = (methodToCompare.ReducedFrom ?? methodToCompare).OriginalDefinition;

				// Ensure parameters only differ by one - the ct
				if (originalMethodWithAllParameters.Parameters.Length != methodToCompareWithAllParameters.Parameters.Length - 1) {
					return false;
				}

				// Now compare the types of all parameters before the ct
				// The largest i is the number of parameters in the method that has fewer parameters
				for (int i = 0; i < originalMethodWithAllParameters.Parameters.Length; i++) {
					IParameterSymbol originalParameter = originalMethodWithAllParameters.Parameters[i];
					IParameterSymbol comparedParameter = methodToCompareWithAllParameters.Parameters[i];
					if (!originalParameter.Type.Equals(comparedParameter.Type)) {
						return false;
					}
				}

				// Overload is  valid if its return type is implicitly convertable
				var toCompareReturnType = methodToCompareWithAllParameters.ReturnType;
				var originalReturnType = originalMethodWithAllParameters.ReturnType;
				if (toCompareReturnType == null || originalReturnType == null || !compilation1.ClassifyCommonConversion(toCompareReturnType, originalReturnType).IsImplicit) {
					// Generic Task-like types are special since awaiting them essentially erases the task-like type.
					// If both types are Task-like we will warn if their generic arguments are convertable to each other.
					if (IsTaskLikeType(originalReturnType) && IsTaskLikeType(toCompareReturnType) &&
						originalReturnType is INamedTypeSymbol originalNamedType &&
						toCompareReturnType is INamedTypeSymbol toCompareNamedType &&
						TypeArgumentsAreConvertable(originalNamedType, toCompareNamedType)) {
						return true;
					}

					return false;
				}

				return true;

				bool IsTaskLikeType(ITypeSymbol typeSymbol) {
					if (!(genericTask1 is null) &&
						typeSymbol.OriginalDefinition.Equals(genericTask1)) {
						return true;
					}

					if (!(genericValueTask1 is null) &&
						typeSymbol.OriginalDefinition.Equals(genericValueTask1)) {
						return true;
					}

					return false;
				}

				bool TypeArgumentsAreConvertable(INamedTypeSymbol left, INamedTypeSymbol right) {
					if (left.Arity != 1 ||
						right.Arity != 1 ||
						left.Arity != right.Arity) {
						return false;
					}

					var leftTypeArgument = left.TypeArguments[0];
					var rightTypeArgument = right.TypeArguments[0];
					if (leftTypeArgument == null || rightTypeArgument == null || !compilation1.ClassifyCommonConversion(leftTypeArgument, rightTypeArgument).IsImplicit) {
						return false;
					}

					return true;
				}
			}
		}
	}
}
