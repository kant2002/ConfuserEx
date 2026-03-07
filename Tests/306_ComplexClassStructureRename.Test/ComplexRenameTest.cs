using System;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace ComplexClassStructureRename.Test {
	public class ComplexRenameTest : TestBase {
		public ComplexRenameTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/306")]
		public async Task ComplexClassStructureRename() =>
			await Run(
				new[] {
					"306_ComplexClassStructureRename.exe",
					"306_ComplexClassStructureRename.Lib.dll"
				},
				new[] {
					"InternalClass1: test1 Hello"
				},
				new SettingItem<Protection>("rename") { 
					{ "mode", "sequential" },
					{ "renPublic", "true" },
					{ "flatten", "false" }
				}
			);
	}
}
