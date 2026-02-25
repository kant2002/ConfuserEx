namespace GenericInterfaceImplRename {
	public class ClassReturnType : IInterfaceReturnType<object> {
		public External Method() {
			return new External("ReturnExternal");
		}
	}
}
