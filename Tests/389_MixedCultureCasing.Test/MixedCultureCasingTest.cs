using System;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace MixedCultureCasing.Test
{
    public class MixedCultureCasingTest : TestBase {
		public MixedCultureCasingTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/389")]
		public async Task MixedCultureCasing() =>
			await Run(
				new [] {
					"389_MixedCultureCasing.exe",
					@"de-DE\389_MixedCultureCasing.resources.dll"
				},
				new [] {
					"Test 1 (neutral)",
					"Test 1 (deutsch)",
					"Test 2 (neutral)",
					"Test 2 (deutsch)"
				},
				new SettingItem<Protection>("rename")
			);
	}
}
