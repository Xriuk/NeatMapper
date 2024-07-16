using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NeatMapper.Analyzers.Test.CSharpCodeFixVerifier<
	NeatMapper.Analyzers.ForwardAsyncMappingContextCancellationTokenToInvocationsAnalyzer,
	NeatMapper.Analyzers.ForwardAsyncMappingContextCancellationTokenToInvocationsFixer>;


namespace NeatMapper.Analyzers.Tests {
	[TestClass]
	public class ForwardAsyncMappingContextCancellationTokenToInvocationsTests {
		// DEV: cannot test Static maps (requires .NET 7.0+)

		[TestMethod]
		[DataRow(/*false, */false)]
		//[DataRow(true, false)]
		[DataRow(/*false, */true)]
		//[DataRow(true, true)]
		public async Task ShouldDetectInsideIAsyncNewMap(/*bool stat, */bool expl) {
			bool stat = false;
			await VerifyCS.VerifyCodeFixAsync(@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Maps : IAsyncNewMap" + (stat ? "Static" : "") + @"<string, bool> {
    " + (expl ? "" : "public ") + (stat ? "static " : "") + @"async Task<bool> " + (expl ? "IAsyncNewMap" + (stat ? "Static" : "") + "<string, bool>." : "") + @"MapAsync(string source, AsyncMappingContext context){
        await {|#0:Task.Delay|}(200);
        return false;
    }
}
",			VerifyCS.Diagnostic()
				.WithLocation(0)
				.WithArguments("context", "Delay"),
				@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Maps : IAsyncNewMap" + (stat ? "Static" : "") + @"<string, bool> {
    " + (expl ? "" : "public ") + (stat ? "static " : "") + @"async Task<bool> " + (expl ? "IAsyncNewMap" + (stat ? "Static" : "") + "<string, bool>." : "") + @"MapAsync(string source, AsyncMappingContext context){
        await Task.Delay(200, context.CancellationToken);
        return false;
    }
}
");
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public async Task ShouldDetectInsideIAsyncMergeMap(bool expl) {
			await VerifyCS.VerifyCodeFixAsync(@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Maps : IAsyncMergeMap<string, bool> {
    " + (expl ? "" : "public ") + @"async Task<bool> " + (expl ? "IAsyncMergeMap<string, bool>." : "") + @"MapAsync(string source, bool destination, AsyncMappingContext myContextName){
        await {|#0:Task.Delay|}(200);
        return false;
    }
}
", VerifyCS.Diagnostic()
				.WithLocation(0)
				.WithArguments("myContextName", "Delay"),
				@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Maps : IAsyncMergeMap<string, bool> {
    " + (expl ? "" : "public ") + @"async Task<bool> " + (expl ? "IAsyncMergeMap<string, bool>." : "") + @"MapAsync(string source, bool destination, AsyncMappingContext myContextName){
        await Task.Delay(200, myContextName.CancellationToken);
        return false;
    }
}
");
		}

		[TestMethod]
		public async Task ShouldDetectInsideCustomAsyncNewAdditionalMapsOptions() {
			await VerifyCS.VerifyCodeFixAsync(@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Test {
	public void Configure(){
        var additionalMaps = new CustomAsyncNewAdditionalMapsOptions();
		additionalMaps.AddMap<string, bool>(async (s, c) => {
            await {|#0:Task.Delay|}(200);
            return false;
        });
    }
}
", VerifyCS.Diagnostic()
	.WithLocation(0)
	.WithArguments("c", "Delay"),
	@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Test {
	public void Configure(){
        var additionalMaps = new CustomAsyncNewAdditionalMapsOptions();
		additionalMaps.AddMap<string, bool>(async (s, c) => {
            await Task.Delay(200, c.CancellationToken);
            return false;
        });
    }
}
");
		}

		[TestMethod]
		public async Task ShouldDetectInsideCustomAsyncMergeAdditionalMapsOptions() {
			await VerifyCS.VerifyCodeFixAsync(@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Test {
	public void Configure(){
        var additionalMaps = new CustomAsyncMergeAdditionalMapsOptions();
		additionalMaps.AddMap<string, bool>(async (s, d, c) => {
            await {|#0:Task.Delay|}(200);
            return false;
        });
    }
}
", VerifyCS.Diagnostic()
	.WithLocation(0)
	.WithArguments("c", "Delay"),
	@"
using NeatMapper;
using System;
using System.Threading.Tasks;

class Test {
	public void Configure(){
        var additionalMaps = new CustomAsyncMergeAdditionalMapsOptions();
		additionalMaps.AddMap<string, bool>(async (s, d, c) => {
            await Task.Delay(200, c.CancellationToken);
            return false;
        });
    }
}
");
		}
	}
}
