﻿using System.Collections.Generic;

namespace NeatMapper.Tests {
	public class TestOptions {}

	public static class MappingOptionsUtils {
		// Lock needed because of parallel async maps
		public static readonly object lockObj = new object();

		public static MappingContext context;
		public static List<MappingContext> contexts = new List<MappingContext>();

		public static AsyncMappingContext? asyncContext;
		public static List<AsyncMappingContext> asyncContexts = new List<AsyncMappingContext>();

		public static TestOptions options;
		public static MergeCollectionsMappingOptions mergeOptions;

		public static MatchingContext matchingContext;
	}
}
