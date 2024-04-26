// Versions in which ConditionalWeakTable implements IEnumerable: NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER,
// but can only test on .NET 5.0 because of GC implementation which passes the tests
#if NET5_0

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper.EntityFrameworkCore.Tests {
	[TestClass]
	public class DbContextTests {
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference RunScope(IServiceProvider serviceProvider) {
			using(var scope = serviceProvider.CreateScope()) { 
				// Initialize db
				var dbContext = scope.ServiceProvider.GetRequiredService<TestContext>();
				dbContext.Tag = "Test";
				dbContext.Database.EnsureDeleted();
				dbContext.Database.EnsureCreated();

				dbContext.Add(new IntKey {
					Id = 2,
					Entity = new OwnedEntity {
						Id = 4
					}
				});
				dbContext.SaveChanges();
	#if NET5_0_OR_GREATER
				dbContext.ChangeTracker.Clear();
	#else
				foreach (var entry in dbContext.ChangeTracker.Entries<IntKey>().ToArray()) {
					entry.State = EntityState.Detached;
				}
	#endif

				var dbContextRef = new WeakReference(dbContext);
				Assert.IsTrue(dbContextRef.IsAlive);

				// Map to create semaphore for context above (should be the same because of lifetime Scoped)
				var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
				mapper.Map<IntKey>(2);

				var conditionalWeakTable = typeof(EfCoreUtils).GetField("_dbContextSemaphores", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as IEnumerable;
				var contextKey = conditionalWeakTable.Cast<object>().First();
				var context = contextKey.GetType().GetProperty(nameof(KeyValuePair<object, object>.Key)).GetValue(contextKey) as TestContext;
				Assert.AreEqual("Test", context.Tag);
				Assert.AreEqual(1, conditionalWeakTable.Cast<object>().Count());

				return dbContextRef;
			}
		}

		[TestMethod]
		public void ShouldDisposeSemaphoreAftedDbContextIsDisposed() {
			using(var connection = new SqliteConnection("Filename=:memory:")) { 
				connection.Open();
			var serviceCollection = new ServiceCollection();
				serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Scoped, ServiceLifetime.Scoped);
				serviceCollection.AddNeatMapper(ServiceLifetime.Scoped, ServiceLifetime.Scoped, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
				serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

				WeakReference dbContextRef = null;
				using (var serviceProvider = serviceCollection.BuildServiceProvider()) {
					dbContextRef = RunScope(serviceProvider);

					GC.Collect();

					// Context should be destroyed
					Assert.IsFalse(dbContextRef.IsAlive);

					// Semaphores should be destroyed
					var conditionalWeakTable = typeof(EfCoreUtils).GetField("_dbContextSemaphores", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as IEnumerable;
					Assert.AreEqual(0, conditionalWeakTable.Cast<object>().Count());
				}
			}
		}
	}
}

#endif