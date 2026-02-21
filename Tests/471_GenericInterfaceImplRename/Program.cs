using System;

namespace GenericInterfaceImplRename {
	public class Program {
		static int Main(string[] args) {
			Console.WriteLine("START");

			var returnObj = new ClassReturnType();
			Console.WriteLine(returnObj.Method());

			var paramObj = new ClassParamType();
			Console.WriteLine(paramObj.Process(new External("ParamExternal")));

			Console.WriteLine("END");

			return 42;
		}
	}
}
