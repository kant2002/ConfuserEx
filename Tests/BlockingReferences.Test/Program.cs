using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace BlockingReferences.Test {
	public class BlockingReferencesTest : TestBase {
		public BlockingReferencesTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/379")]
		public async Task BlockingReferences() =>
			await BlockingReferencesInternal("BlockingReferences.exe", "BlockingReferencesHelper.dll");

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/379")]
		public async Task BlockingReferencesReverse() =>
			await BlockingReferencesInternal("BlockingReferencesHelper.dll", "BlockingReferences.exe");

		private async Task BlockingReferencesInternal(params string[] files) =>
			await Run(
				files,
				new[] {
					"",
					"Implementation2",
				},
				new SettingItem<Protection>("rename") {
					["renPublic"] = "true",
					["mode"] = "decodable"
				},
				outputAction: line => {
					Assert.DoesNotContain("[WARN] Failed to rename all targeted members", line);
				}
			);
	}
}
