using System;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace EnumArrayConstantProtection.Test {
	public class ConstantProtectionTest : TestBase {
		public ConstantProtectionTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "constants")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/270")]
		public async Task ConstantsProtection() =>
			await Run(
				"270_EnumArrayConstantProtection.exe",
				new[] {
					"Enum Array OK",
					"String Array OK"
				},
				new SettingItem<Protection>("constants") { { "elements", "SI" } }
			);
	}
}
