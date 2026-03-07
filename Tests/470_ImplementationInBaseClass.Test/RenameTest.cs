using System.Collections.Generic;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.Renamer;
using Confuser.UnitTest;
using Xunit;

namespace ImplementationInBaseClass.Test
{
	public class RenameTest : TestBase
	{
		public RenameTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Theory]
		[MemberData(nameof(ResolveNameData))]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/470")]
		public async Task ResolveNameLoop(RenameMode mode, bool flatten) =>
			await Run(
				new[] {
					"470_ImplementationInBaseClass.exe"
				},
				new[] {
					"Called MyMethod",
					"Called MyMethod",
					"Called MyMethod",
					"Called MyMethod"
				},
				new SettingItem<Protection>("rename") {
					{ "mode", mode.ToString() },
					{ "renPublic", "true" },
					{ "flatten", flatten.ToString() }
				},
				$"_{mode}_{flatten}"
			);

		public static IEnumerable<object[]> ResolveNameData() {
			foreach (var renameMode in new[] { RenameMode.Unicode, RenameMode.Sequential })
				foreach (var flatten in new[] { true, false })
					yield return new object[] { renameMode, flatten };
		}
	}
}
