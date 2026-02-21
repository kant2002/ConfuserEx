using System.Collections.Generic;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.Renamer;
using Confuser.UnitTest;
using Xunit;
using Xunit.Abstractions;

namespace GenericInterfaceImplRename.Test {
	public class RenameGenericInterfaceImplTest : TestBase {
		public RenameGenericInterfaceImplTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Theory]
		[MemberData(nameof(RenameGenericInterfaceImplData))]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/tshcherban/ConfuserEx/issues/1")]
		public async Task RenameGenericInterfaceImpl(string renameMode, bool flatten) =>
			await Run(
				"471_GenericInterfaceImplRename.exe",
				new[] {
					"ReturnExternal",
					"Processed:ParamExternal"
				},
				new SettingItem<Protection>("rename") {
					{ "mode", renameMode },
					{ "flatten", flatten.ToString() }
				},
				$"_{renameMode}_{flatten}"
			);

		public static IEnumerable<object[]> RenameGenericInterfaceImplData() {
			foreach (var renameMode in new[] { nameof(RenameMode.Unicode), nameof(RenameMode.ASCII), nameof(RenameMode.Letters), nameof(RenameMode.Debug), nameof(RenameMode.Retain) })
				foreach (var flatten in new[] { true, false })
					yield return new object[] { renameMode, flatten };
		}
	}
}
