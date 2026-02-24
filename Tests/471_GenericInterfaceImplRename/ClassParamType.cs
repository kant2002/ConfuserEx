namespace GenericInterfaceImplRename {
	public class ClassParamType : IInterfaceParamType<object> {
		public string Process(External input) {
			return "Processed:" + input.Value;
		}
	}
}
