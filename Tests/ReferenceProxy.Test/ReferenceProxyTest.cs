using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.UnitTest;
using Xunit;

namespace ReferenceProxy.Test {
	public class ReferenceProxyTest : TestBase {
		public ReferenceProxyTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Theory]
		[MemberData(nameof(ReferenceProxyData))]
		[Trait("Category", "Protection")]
		[Trait("Protection", "ref proxy")]
		[Trait("Issue", "https://github.com/mkaring/ConfuserEx/issues/229")]
		public async Task ReferenceProxy(string mode, string encoding, bool internalRefs, bool typeErasure) =>
			await Run(
				"ReferenceProxy.exe",
				Array.Empty<string>(),
				new SettingItem<Protection>("ref proxy") {
					["mode"] = mode,
					["encoding"] = encoding,
					["internal"] = internalRefs.ToString(),
					["typeErasure"] = typeErasure.ToString()
				},
				outputDirSuffix: $"_{mode}_{encoding}_{internalRefs}_{typeErasure}"
			);

		public static IEnumerable<object[]> ReferenceProxyData() {
			foreach (var mode in new[] { "mild", "strong" })
				foreach (var encoding in new[] { "normal", "expression", "x86" })
					foreach (var internalRefs in new[] { true, false })
						foreach (var typeErasure in new[] { true, false }) {
							if (mode.Equals("mild") && !encoding.Equals("normal")) continue;
							yield return new object[] { mode, encoding, internalRefs, typeErasure };
						}
		}
	}
}
