using System.IO;
using System.Xml;
using Confuser.Core.Project;
using Xunit;

namespace Confuser.Core.Test {
	public class ConfuserProjectTest {
		[Fact]
		public void SerializeConfuserProjectSuccessfully() {
			var proj = new ConfuserProject();
			proj.BaseDirectory = @"c:\obfuscation\input";
			proj.OutputDirectory = @"c:\obfuscation\output";
			proj.Debug = true;
			proj.Add(new ProjectModule() {
				Path = @"c:\obfuscation\input\test.dll"
			});
			proj.Add(new ProjectModule() {
				Path = @"c:\obfuscation\input\test2.dll"
			});
			proj.ProbePaths.Add(@"c:\obfuscation\input\bin");

			var xmlDoc = proj.Save();
			var stringWriter = new StringWriter();
			var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Indent = true });
			xmlDoc.WriteTo(xmlWriter);
			xmlWriter.Flush();

			Assert.Equal(
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<project outputDir=""c:\obfuscation\output"" baseDir=""c:\obfuscation\input"" debug=""true"" xmlns=""http://confuser.codeplex.com"">
  <module path=""c:\obfuscation\input\test.dll"" />
  <module path=""c:\obfuscation\input\test2.dll"" />
  <probePath>c:\obfuscation\input\bin</probePath>
</project>".Replace("\r\n", "\n"),
				stringWriter.ToString().Replace("\r\n", "\n"));
		}
	}
}
