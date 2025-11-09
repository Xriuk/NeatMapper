using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches collections by using another <see cref="IMatcher"/> to match elements.
	/// Different matching options are available, defined in <see cref="CollectionMatchersOptions"/>.
	/// </summary>
	public sealed class CollectionMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// <see cref="IMatcher"/> which is used to match source elements with destination elements.
		/// </summary>
		private readonly IMatcher _elementsMatcher;

		/// <summary>
		/// Options to apply when matching collections.
		/// </summary>
		private readonly CollectionMatchersOptions _collectionMatcherOptions;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CollectionMatcher"/>.
		/// </summary>
		/// <param name="elementsMatcher">
		/// <see cref="IMatcher"/> used to match elements between collections to merge them,
		/// if null the elements won't be matched.<br/>
		/// Can be overridden during matching with <see cref="MatcherOverrideMappingOptions.Matcher"/>.
		/// </param>
		/// <param name="collectionMatcherOptions">
		/// Options to apply when matching collections.<br/>
		/// Can be overridden during mapping with <see cref="CollectionMatchersMappingOptions"/>.
		/// </param>
		public CollectionMatcher(
			IMatcher elementsMatcher,
			CollectionMatchersOptions? collectionMatcherOptions = null) {

			_elementsMatcher = new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = [ elementsMatcher ?? throw new ArgumentNullException(nameof(elementsMatcher)), this ]
			});
			_collectionMatcherOptions = collectionMatcherOptions != null ? new CollectionMatchersOptions(collectionMatcherOptions) : new CollectionMatchersOptions();
			var nestedMatchingContext = new NestedMatchingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MatcherOverrideMappingOptions, NestedMatchingContext>(
				m => m?.Matcher != null ? m : new MatcherOverrideMappingOptions(_elementsMatcher, m?.ServiceProvider),
				n => n != null ? new NestedMatchingContext(nestedMatchingContext.ParentMatcher, n) : nestedMatchingContext, options.Cached));
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMatchInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMatchInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMatcher) || elementsMatcher == null)
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			if (source is IEnumerable sourceEnumerable) {
				if (destination == null) {
					if (mappingOptions.GetOptions<CollectionMatchersMappingOptions>()?.NullEqualsEmpty ?? _collectionMatcherOptions.NullEqualsEmpty) {
						var sourceEnumerator = sourceEnumerable.GetEnumerator();
						try {
							return !sourceEnumerator.MoveNext();
						}
						finally {
							if (sourceEnumerator is IDisposable disposable)
								disposable.Dispose();
						}
					}
					else
						return false;
				}
				else if (destination is IEnumerable destinationEnumerable) {
					// Check if the matching is ordered or not
					var collectionMatchingOrder = mappingOptions.GetOptions<CollectionMatchersMappingOptions>()?.CollectionMatchingOrder
						?? _collectionMatcherOptions.CollectionMatchingOrder;
					if(collectionMatchingOrder == CollectionMatchingOrder.Default) {
						if(sourceType.HasInterface(typeof(IList<>)) ||
							sourceType.HasInterface(typeof(IReadOnlyList<>)) ||
							destinationType.HasInterface(typeof(IList<>)) ||
							destinationType.HasInterface(typeof(IReadOnlyList<>))){

							collectionMatchingOrder = CollectionMatchingOrder.Ordered;
						}
						else
							collectionMatchingOrder = CollectionMatchingOrder.NotOrdered;
					}

					if(collectionMatchingOrder == CollectionMatchingOrder.Ordered) {
						var sourceEnumerator = sourceEnumerable.GetEnumerator();
						try {
							var destinationEnumerator = destinationEnumerable.GetEnumerator();
							try {
								var sourceHasNext = sourceEnumerator.MoveNext();
								var destinationHasNext = destinationEnumerator.MoveNext();
								if(sourceHasNext && destinationHasNext) { 
									using(var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions)) { 
										while(sourceHasNext && destinationHasNext) {
											if(!elementsMatcherFactory.Invoke(sourceEnumerator.Current, destinationEnumerator.Current))
												return false;

											sourceHasNext = sourceEnumerator.MoveNext();
											destinationHasNext = destinationEnumerator.MoveNext();
										}
									}
								}

								// If the collection have a different number of elements the last enumerator values will be different
								return sourceHasNext == destinationHasNext;
							}
							finally {
								if (destinationEnumerator is IDisposable disposable)
									disposable.Dispose();
							}
						}
						finally {
							if (sourceEnumerable is IDisposable disposable)
								disposable.Dispose();
						}
					}
					else {
						var matchedDestinations = ObjectPool.Lists.Get();
						var sourceCount = 0;

						try {
							using (var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions)) {
								foreach (var sourceElement in sourceEnumerable) {
									bool found = false;
									foreach (var destinationElement in destinationEnumerable.Cast<object>()) {
										if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
											matchedDestinations.Add(destinationElement);
											found = true;
											break;
										}
									}
									if (!found)
										return false;

									sourceCount++;
								}

								if (sourceCount != matchedDestinations.Count) {
									foreach (var destinationElement in destinationEnumerable.Cast<object>().Except([..matchedDestinations])) {
										bool found = false;
										foreach (var sourceElement in sourceEnumerable.Cast<object>()) {
											if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
												matchedDestinations.Add(destinationElement);
												found = true;
												break;
											}
										}
										if (!found)
											return false;
									}

									// If all elements were matched we should also have the full length of the collection
									return sourceCount == matchedDestinations.Count;
								}
								else {
									// If all elements were matched we should also have the full length of the collection
									return sourceCount == destinationEnumerable.Cast<object>().Count();
								}
							}
						}
						finally {
							ObjectPool.Lists.Return(matchedDestinations);
						}
					}
				}
				else
					throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
			}
			else if(source == null) {
				if(destination == null)
					return true;
				else if(destination is IEnumerable destinationEnumerable) {
					if (mappingOptions.GetOptions<CollectionMatchersMappingOptions>()?.NullEqualsEmpty ?? _collectionMatcherOptions.NullEqualsEmpty) {
						var destinationEnumerator = destinationEnumerable.GetEnumerator();
						try {
							return !destinationEnumerator.MoveNext();
						}
						finally {
							if(destinationEnumerator is IDisposable disposable)
								disposable.Dispose();
						}
					}
					else
						return false;
				}
				else
					throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
			}
			else
				throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMatchInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMatcher) || elementsMatcher == null)
				throw new MapNotFoundException(types);

			// Check if the matching is ordered or not
			var collectionMatchingOrder = mappingOptions.GetOptions<CollectionMatchersMappingOptions>()?.CollectionMatchingOrder
				?? _collectionMatcherOptions.CollectionMatchingOrder;
			if (collectionMatchingOrder == CollectionMatchingOrder.Default) {
				if (sourceType.HasInterface(typeof(IList<>)) ||
					sourceType.HasInterface(typeof(IReadOnlyList<>)) ||
					destinationType.HasInterface(typeof(IList<>)) ||
					destinationType.HasInterface(typeof(IReadOnlyList<>))) {

					collectionMatchingOrder = CollectionMatchingOrder.Ordered;
				}
				else
					collectionMatchingOrder = CollectionMatchingOrder.NotOrdered;
			}

			var nullEqualsEmpty = mappingOptions.GetOptions<CollectionMatchersMappingOptions>()?.NullEqualsEmpty
				?? _collectionMatcherOptions.NullEqualsEmpty;

			var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions);

			try { 
				if (collectionMatchingOrder == CollectionMatchingOrder.Ordered) {
					return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source is IEnumerable sourceEnumerable) {
							if (destination == null) {
								if (nullEqualsEmpty) {
									var sourceEnumerator = sourceEnumerable.GetEnumerator();
									try {
										return !sourceEnumerator.MoveNext();
									}
									finally {
										if (sourceEnumerator is IDisposable disposable)
											disposable.Dispose();
									}
								}
								else
									return false;
							}
							else if (destination is IEnumerable destinationEnumerable) {
								var sourceEnumerator = sourceEnumerable.GetEnumerator();
								try {
									var destinationEnumerator = destinationEnumerable.GetEnumerator();
									try {
										var sourceHasNext = sourceEnumerator.MoveNext();
										var destinationHasNext = destinationEnumerator.MoveNext();
										while (sourceHasNext && destinationHasNext) {
											if (!elementsMatcherFactory.Invoke(sourceEnumerator.Current, destinationEnumerator.Current))
												return false;

											sourceHasNext = sourceEnumerator.MoveNext();
											destinationHasNext = destinationEnumerator.MoveNext();
										}

										// If the collection have a different number of elements the last enumerator values will be different
										return sourceHasNext == destinationHasNext;
									}
									finally {
										if (destinationEnumerator is IDisposable disposable)
											disposable.Dispose();
									}
								}
								finally {
									if (sourceEnumerable is IDisposable disposable)
										disposable.Dispose();
								}
							}
							else
								throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
						}
						else if (source == null) {
							if (destination == null)
								return true;
							else if (destination is IEnumerable destinationEnumerable) {
								if (nullEqualsEmpty) {
									var destinationEnumerator = destinationEnumerable.GetEnumerator();
									try {
										return !destinationEnumerator.MoveNext();
									}
									finally {
										if (destinationEnumerator is IDisposable disposable)
											disposable.Dispose();
									}
								}
								else
									return false;
							}
							else
								throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
						}
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					});
				}
				else {
					return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source is IEnumerable sourceEnumerable) {
							if (destination == null) {
								if (nullEqualsEmpty) {
									var sourceEnumerator = sourceEnumerable.GetEnumerator();
									try {
										return !sourceEnumerator.MoveNext();
									}
									finally {
										if (sourceEnumerator is IDisposable disposable)
											disposable.Dispose();
									}
								}
								else
									return false;
							}
							else if (destination is IEnumerable destinationEnumerable) {
								var matchedDestinations = ObjectPool.Lists.Get();
								var sourceCount = 0;

								try { 
									foreach (var sourceElement in sourceEnumerable) {
										bool found = false;
										foreach (var destinationElement in destinationEnumerable.Cast<object>()) {
											if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
												matchedDestinations.Add(destinationElement);
												found = true;
												break;
											}
										}
										if (!found)
											return false;

										sourceCount++;
									}

									if(sourceCount != matchedDestinations.Count) {
										foreach(var destinationElement in destinationEnumerable.Cast<object>().Except([.. matchedDestinations])) {
											bool found = false;
											foreach (var sourceElement in sourceEnumerable.Cast<object>()) {
												if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
													matchedDestinations.Add(destinationElement);
													found = true;
													break;
												}
											}
											if (!found)
												return false;
										}

										// If all elements were matched we should also have the full length of the collection
										return sourceCount == matchedDestinations.Count;
									}
									else { 
										// If all elements were matched we should also have the full length of the collection
										return sourceCount == destinationEnumerable.Cast<object>().Count();
									}
								}
								finally {
									ObjectPool.Lists.Return(matchedDestinations);
								}
							}
							else
								throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
						}
						else if (source == null) {
							if (destination == null)
								return true;
							else if (destination is IEnumerable destinationEnumerable) {
								if (nullEqualsEmpty) {
									var destinationEnumerator = destinationEnumerable.GetEnumerator();
									try {
										return !destinationEnumerator.MoveNext();
									}
									finally {
										if (destinationEnumerator is IDisposable disposable)
											disposable.Dispose();
									}
								}
								else
									return false;
							}
							else
								throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
						}
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					});
				}
			}
			catch {
				elementsMatcherFactory.Dispose();
				throw;
			}
		}


		private bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out (Type From, Type To) elementTypes,
			out IMatcher elementsMatcher) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable()) {
				if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
					elementTypes = default;
					elementsMatcher = null!;
					mappingOptions = null!;

					return true;
				}
				else { 
					elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType());
					mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
					elementsMatcher = mappingOptions.GetOptions<MatcherOverrideMappingOptions>()?.Matcher ?? _elementsMatcher;

					return elementsMatcher.CanMatch(elementTypes.From, elementTypes.To, mappingOptions);
				}
			}
			else {
				elementTypes = default;
				elementsMatcher = null!;

				return false;
			}
		}
	}
}
