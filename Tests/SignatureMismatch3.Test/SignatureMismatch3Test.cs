using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;
using Xunit.Abstractions;

namespace SignatureMismatch3.Test {
	public class SignatureMismatch3Test : TestBase {
		public SignatureMismatch3Test(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/187")]
		public async Task SignatureMismatch3() =>
			await Run(
				new [] { "SignatureMismatch3.exe", "SignatureMismatch2Helper.dll" },
				new [] { "ReturnExternal", "External" },
				new SettingItem<Protection>("rename") { ["renPublic"] = "true", ["mode"] = "debug" }
			);
	}
}
