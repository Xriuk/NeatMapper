using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class ObjectEqualsMatcherTests {
		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = ObjectEqualsMatcher.Instance;
		}


		[TestMethod]
		public void ShouldMatch() {
			Assert.IsTrue(_matcher.CanMatch<string, string>());
			Assert.IsFalse(_matcher.CanMatch<string, int>());
			Assert.IsFalse(_matcher.CanMatch<int, string>());
			Assert.IsTrue(_matcher.CanMatch<int, int>());
			Assert.IsFalse(_matcher.CanMatch<Guid, string>());
			Assert.IsTrue(_matcher.CanMatch<(int, string), (int, string)>());

			Assert.IsTrue(_matcher.Match("abcd", "abcd"));
			Assert.IsFalse(_matcher.Match<string, string>("abcd", null));
			Assert.IsFalse(_matcher.Match<string, string>(null, "efgh"));
			Assert.IsTrue(_matcher.Match<string, string>(null, null));
			Assert.IsFalse(_matcher.Match("abcd", "efgh"));
			Assert.IsTrue(_matcher.Match(4, 4));
			Assert.IsTrue(_matcher.Match(new Guid("a126b1a9-01f8-4aeb-86b1-fa98797d58ce"), new Guid("a126b1a9-01f8-4aeb-86b1-fa98797d58ce")));
			Assert.IsFalse(_matcher.Match(new Guid("a126b1a9-01f8-4aeb-86b1-fa98797d58ce"), Guid.Empty));
			Assert.IsTrue(_matcher.Match((2, "ciao"), (Id: 2, Name: "ciao")));

			using (var factory = _matcher.MatchFactory<string, string>()) {
				Assert.IsTrue(factory.Invoke("abcd", "abcd"));
				Assert.IsFalse(factory.Invoke("abcd", "efgh"));
			}
		}
	}
}
