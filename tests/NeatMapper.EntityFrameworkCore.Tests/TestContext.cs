using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace NeatMapper.EntityFrameworkCore.Tests {
	public class TestContext : DbContext {
		public TestContext(DbContextOptions<TestContext> options) : base(options) {}

		public string Tag { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<IntKey>()
				.OwnsOne(o => o.Entity);

			modelBuilder.Entity<GuidKey>();

			var ownedBuilder = modelBuilder.Entity<StringKey>()
				.OwnsMany(o => o.Entities);
			ownedBuilder.Property(typeof(string), "StringId");
			ownedBuilder.WithOwner()
				.HasForeignKey("StringId");
			ownedBuilder.HasKey("StringId", "Id");

			modelBuilder.Entity<CompositePrimitiveKey>()
				.HasKey(o => new { o.Id1, o.Id2 });

			modelBuilder.Entity<CompositeClassKey>()
				.HasKey(o => new { o.Id1, o.Id2 });

			modelBuilder.Entity<ShadowIntKey>()
				.Property(typeof(int), "Id");
			modelBuilder.Entity<ShadowIntKey>()
				.HasKey("Id");

			modelBuilder.Entity<ShadowStringKey>()
				.Property(typeof(string), "StringId");
			modelBuilder.Entity<ShadowStringKey>()
				.HasOne(s => s.String).WithMany()
				.HasForeignKey("StringId");
			modelBuilder.Entity<ShadowStringKey>()
				.HasKey("StringId");

			modelBuilder.Entity<ShadowCompositeKey>()
				.Property(typeof(string), "Id2");
			modelBuilder.Entity<ShadowCompositeKey>()
				.HasKey("Id1", "Id2");

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
			modelBuilder.Entity<Keyless>()
				.HasNoKey();
#endif
		}
	}
}
