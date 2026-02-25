using System;

namespace GenericInterfaceImplRename {
	public class Program {
		static int Main(string[] args) {
			Console.WriteLine("START");

			var returnObj = new ClassReturnType();
			Console.WriteLine(returnObj.Method());

			var paramObj = new ClassParamType();
			Console.WriteLine(paramObj.Process(new External("ParamExternal")));

			var bothObj = new ClassBothTypes();
			Console.WriteLine(bothObj.Transform(new External("BothTest")));

			var multiObj = new ClassMultiMethod();
			Console.WriteLine(multiObj.GetFirst());
			Console.WriteLine(multiObj.GetSecond());
			Console.WriteLine(multiObj.Format(new External("MultiTest")));

			Console.WriteLine("END");

			return 42;
		}
	}
}
