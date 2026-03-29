using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Confuser.MSBuild.Tasks.Tests {
	public class ConfusionTest(ITestOutputHelper testOutputHelper) : ProjectTestBase(testOutputHelper) {

		[Theory]
		[InlineData("HelloWorld")]
		public async Task Confuse_Exe_ShouldSucceed(string projectName) {
			HashSet<string> expectedObjArtifacts =
			[
				$"{projectName}.dll"
			];

			var hostExeFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{projectName}.exe" : projectName;
			HashSet<string> expectedBinArtifacts =
			[
				$"{projectName}.dll",
			hostExeFile,
			$"{projectName}.runtimeconfig.json",
			$"{projectName}.deps.json",
		];

			var result = await ExecuteTargets(projectName, "Restore", "Build");

			Assert.True(result.ExitCode == 0);
			AssertIncludes(expectedObjArtifacts, result.IntermediateArtifacts.Select(a => a.FileName).ToList());
			AssertIncludes(expectedBinArtifacts, result.OutputArtifacts.Select(a => a.FileName).ToList());
		}
	}
}
