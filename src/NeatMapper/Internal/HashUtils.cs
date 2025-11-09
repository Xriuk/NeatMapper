namespace NeatMapper {
	// DEV: remove when .NET 4.8 support is dropped
	internal static class HashUtils {
		public static int Combine<T1, T2>(T1 value1, T2 value2) {
#if NET48_OR_GREATER
			int hash = 17;
			hash = hash * 31 + (value1?.GetHashCode() ?? 0);
			hash = hash * 31 + (value2?.GetHashCode() ?? 0);
			return hash;
#else
			return System.HashCode.Combine(value1, value2);
#endif
		}

		public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3) {
#if NET48_OR_GREATER
			int hash = 17;
			hash = hash * 31 + (value1?.GetHashCode() ?? 0);
			hash = hash * 31 + (value2?.GetHashCode() ?? 0);
			hash = hash * 31 + (value3?.GetHashCode() ?? 0);
			return hash;
#else
			return System.HashCode.Combine(value1, value2, value3);
#endif
		}
	}
}
