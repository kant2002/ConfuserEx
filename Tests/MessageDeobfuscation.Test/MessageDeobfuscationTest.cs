using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.Renamer;
using Confuser.UnitTest;
using Xunit;

namespace MessageDeobfuscation.Test {
	public class MessageDeobfuscationTest : TestBase {
		readonly string _expectedDeobfuscatedOutput = String.Join(Environment.NewLine,
			"Exception",
			"   at MessageDeobfuscation.Class.NestedClass.Method(String )",
			"   at MessageDeobfuscation.Program.Main()");

		const string Password = "password";
		const string Seed = "seed";

		public MessageDeobfuscationTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Theory]
		[MemberData(nameof(RenameModeAndExpectedObfuscatedOutput))]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		public async Task MessageDeobfuscationWithSymbolsMap(string renameMode, string[] expectedObfuscatedOutput) =>
			await Run(
				"MessageDeobfuscation.exe",
				expectedObfuscatedOutput,
				new SettingItem<Protection>("rename") { ["mode"] = renameMode },
				$"SymbolsMap_{renameMode}",
				seed: "1234",
				postProcessAction: outputPath => {
					var deobfuscator = MessageDeobfuscator.Load(Path.Combine(outputPath, "symbols.map"));
					var deobfuscatedMessage =
						deobfuscator.DeobfuscateMessage(string.Join(Environment.NewLine, expectedObfuscatedOutput));

					string classId, nestedClassId, methodId, fieldId, propertyId, eventId;
					if (renameMode == nameof(RenameMode.Decodable)) {
						classId = "_OokpKOmal5JNZMPvSAFgHLHjBke";
						nestedClassId = "_tc5CFDIJ2J9Fx3ehd3sgjTMAxCaA";
						methodId = "_zbgDV4jbK6Oi9WBq66uG2ct7IoRA";
						fieldId = "_QHxqC1xaBFmUQawCZSOQpattICo";
						propertyId = "_FJthtfOBOiQFgVDIymbi3wwJoeN";
						eventId = "_cbPBZqkDuaNXOkmJtacrG2uYfZs";
					}
					else {
						classId = "_F";
						nestedClassId = "_D";
						methodId = "_c";
						fieldId = "_C";
						propertyId = "_e";
						eventId = "_A";
					}

					void CheckName(string expectedFullName, string expectedShortName, string obfuscatedName) {
						var fullName = deobfuscator.DeobfuscateSymbol(obfuscatedName, false);
						Assert.Equal(expectedFullName, fullName);
						Assert.Equal(expectedShortName, MessageDeobfuscator.ExtractShortName(fullName));
					}

					CheckName("MessageDeobfuscation.Class", "MessageDeobfuscation.Class",
						classId);
					CheckName("MessageDeobfuscation.Class/NestedClass", "NestedClass",
						nestedClassId);
					CheckName("MessageDeobfuscation.Class::Method(System.String,System.Int32)", "Method",
						methodId);
					CheckName("MessageDeobfuscation.Class::Field", "Field",
						fieldId);
					CheckName("MessageDeobfuscation.Class::Property", "Property",
						propertyId);
					CheckName("MessageDeobfuscation.Class::Event", "Event",
						eventId);

					Assert.Equal(_expectedDeobfuscatedOutput, deobfuscatedMessage);
					return Task.Delay(0);
				}
			);

		public static IEnumerable<object[]> RenameModeAndExpectedObfuscatedOutput() =>
			new[] {
				new object[] {
					nameof(RenameMode.Decodable),
					new[] {
						"Exception",
						"   at _OokpKOmal5JNZMPvSAFgHLHjBke._tc5CFDIJ2J9Fx3ehd3sgjTMAxCaA._8Tq88jpv7mEXkEMavg6AaMFsXJt(String )",
						"   at _ykdLsBmsKGrd6fxeEseqJs8XlpP._tfvbqapfg44suL8taZVvOKM4AoG()"
					}
				},
				new object[] {
					nameof(RenameMode.Sequential), new[] {
						"Exception",
						"   at _F._D._B(String )",
						"   at _b._E()"
					}
				}
			};

		[Fact]
		[Trait("Category", "Protection")]
		public async Task CheckGeneratedPassword() {
			string actualPassword1 = null, actualPassword2 = null;
			await RunDeobfuscationWithPassword(true, null, "_0", Array.Empty<string>(),
				outputPath => {
				actualPassword1 = File.ReadAllText(Path.Combine(outputPath, CoreComponent.PasswordFileName));
				Assert.True(Guid.TryParse(actualPassword1, out _));
				return Task.Delay(0);
			});
			await RunDeobfuscationWithPassword(true, null, "_1", Array.Empty<string>(),
				outputPath => {
				actualPassword2 = File.ReadAllText(Path.Combine(outputPath, CoreComponent.PasswordFileName));
				Assert.True(Guid.TryParse(actualPassword2, out _));
				return Task.Delay(0);
			});
			Assert.NotEqual(actualPassword1, actualPassword2);
		}

		[Fact]
		[Trait("Category", "Protection")]
		public async Task CheckPasswordDependsOnSeed() {
			var expectedObfuscatedOutput = new[] {
				"Exception",
				"   at oZuuchQgRo99FxO43G5kj2LB6aE3b$hsLiIOVL3cn0lg.98C7L64wnMJK6DFKHzyWSw8.at9I2jHJrbSIlewmDrNXdMI(String )",
				"   at EcGxTPKtKIEeZuP3ekjPVhrVKQsiovm5zMkq5xfZbt1V.AiskF07vqbD8ZFG03Jyiiu8()"
			};
			await RunDeobfuscationWithPassword(true, Seed, "_0", expectedObfuscatedOutput,
				outputPath => {
					Assert.Equal(Seed, File.ReadAllText(Path.Combine(outputPath, CoreComponent.PasswordFileName)));
				return Task.Delay(0);
			});
			await RunDeobfuscationWithPassword(true, Seed, "_1", expectedObfuscatedOutput,
				outputPath => {
					Assert.Equal(Seed, File.ReadAllText(Path.Combine(outputPath, CoreComponent.PasswordFileName)));
				return Task.Delay(0);
			});
		}

		[Fact]
		[Trait("Category", "Protection")]
		[Trait("Protection", "rename")]
		public async Task MessageDeobfuscationWithPassword() {
			var expectedObfuscatedOutput = new[] {
				"Exception",
				"   at oQmpV$y2k2b9P3d6GP1cxGPuRtKaNIZvZcKpZXSfKFG8.99_z9Rxdp_fWfuD3fr45FSA.at9DaPNMANuLaMV_3scPWDU(String )",
				"   at EbUjRcrC76NnA7RJlhQffrfp$vMGHdDfqtVFtWrAOPyD.AkpOh$3Zo3M8ga5lTY9etcM()"
			};
			await RunDeobfuscationWithPassword(false, null, "", expectedObfuscatedOutput, outputPath => {
				var deobfuscator = new MessageDeobfuscator(Password);
				var deobfuscatedMessage =
					deobfuscator.DeobfuscateMessage(string.Join(Environment.NewLine, expectedObfuscatedOutput));

				void CheckName(string expectedName, string obfuscatedName) {
					var name = deobfuscator.DeobfuscateSymbol(obfuscatedName, true);
					Assert.Equal(expectedName, name);
				}

				CheckName("MessageDeobfuscation.Class", "oQmpV$y2k2b9P3d6GP1cxGPuRtKaNIZvZcKpZXSfKFG8");
				CheckName("NestedClass", "CE8t0VDPQk9$jgv1XuRwt1k");
				CheckName("Method", "jevJU4p4yNrAYGqN7GkRWaI");
				CheckName("Field", "3IS4xsnUsvDQZop6e4WmNVw");
				CheckName("Property", "917VMBMNYHd0kfnnNkgeJ10");
				CheckName("Event", "AIyINk7kgFLFc73Md8Nu8Z0");

				Assert.Equal(_expectedDeobfuscatedOutput, deobfuscatedMessage);
				return Task.Delay(0);
			});
		}

		async Task RunDeobfuscationWithPassword(bool generatePassword, string seed, string suffix,
			string[] expectedObfuscatedOutput, Func<string, Task> postProcessAction) => await Run(
			"MessageDeobfuscation.exe",
			expectedObfuscatedOutput,
			new SettingItem<Protection>("rename") {
				["mode"] = "reversible",
				["password"] = Password,
				["generatePassword"] = generatePassword.ToString()
			},
			$"Password_{(generatePassword ? $"Random{(seed != null ? "_Seed" : "")}{suffix}" : $"Hardcoded{suffix}")}",
			checkOutput: !generatePassword || seed != null,
			seed: seed,
			postProcessAction: postProcessAction
		);
	}
}
