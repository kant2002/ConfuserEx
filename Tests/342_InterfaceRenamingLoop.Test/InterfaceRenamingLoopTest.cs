using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.Renamer;
using Confuser.UnitTest;
using Xunit;

namespace InterfaceRenamingLoop.Test {
	public class InterfaceRenamingLoopTest : TestBase {
		public InterfaceRenamingLoopTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Theory]
		[MemberData(nameof(RenameInterfaceLoopTestData))]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/342")]
		public async Task RenameInterfaceLoop(string renameMode) =>
			await Run(
				"342_InterfaceRenamingLoop.exe",
				Array.Empty<string>(),
				new SettingItem<Protection>("rename") {
					{ "mode", renameMode }
				},
				$"_{renameMode}"
			);

		public static IEnumerable<object[]> RenameInterfaceLoopTestData() {
			foreach (var renameMode in new[] { nameof(RenameMode.Unicode), nameof(RenameMode.Debug), nameof(RenameMode.Sequential) })
				yield return new object[] { renameMode };
		}
	}
}
