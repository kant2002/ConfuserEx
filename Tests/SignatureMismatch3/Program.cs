using System;
using SignatureMismatch2Helper;

namespace SignatureMismatch3 {
	public interface IInterfaceReturnType<T>
	{
		External Method();
	}

	public class ClassReturnType : IInterfaceReturnType<object>
	{
		public External Method() {
			 Console.WriteLine("ReturnExternal");
			return new External();
		}
	}

	public interface IInterfaceArg<T> {
		 void Method(External arg);
	}

	public class ClassArg : IInterfaceArg<object> {
		public void Method(External arg) {
			Console.WriteLine(arg.Name);
		}
	}

	public class Program {
		static int Main(string[] args) {
			Console.WriteLine("START");
			new ClassReturnType().Method();
			new ClassArg().Method(new External());
			Console.WriteLine("END");

			return 42;
		}
	}
}
