using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace NewtonsoftJsonSerialization.Test {
	public class NewtonsoftJsonTest : TestBase {
		public NewtonsoftJsonTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/421")]
		public async Task SignatureMismatch() =>
			await Run(
				new[] {
					"421_NewtonsoftJsonSerialization.exe",
					"external:Newtonsoft.Json.dll"
				},
				new [] {
					"{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}",
					"{\"a\":\"a\",\"b\":\"b\",\"c\":\"c\"}"
				},
				new SettingItem<Protection>("rename")
			);
	}
}
