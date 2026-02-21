namespace GenericInterfaceImplRename {
	public class ClassMultiMethod : IInterfaceMultiMethod<object> {
		public External GetFirst() {
			return new External("First");
		}

		public External GetSecond() {
			return new External("Second");
		}

		public string Format(External input) {
			return "Fmt:" + input.Value;
		}
	}
}
