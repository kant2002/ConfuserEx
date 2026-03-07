using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace SignatureMismatch.Test {
	public class RenameDynamicParameterTest : TestBase {
		public RenameDynamicParameterTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/345")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/349")]
		public async Task RenameDynamicParameter() =>
			await Run(
				"345_RenameDynamicParameter.exe",
				new[] {
					"static message",
					"dynamic message",
					"Overload String: Test",
					"Overload Integer: 1",
					"Override String: Test",
					"Override Integer: 1",
					"Field Value: 1",
					"Ctor String Value",
					"Ctor Integer Value: 1"
				},
				new SettingItem<Protection>("rename")
			);
	}
}
