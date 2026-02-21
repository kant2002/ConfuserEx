namespace GenericInterfaceImplRename {
	public class External {
		public string Value { get; set; }

		public External(string value) {
			Value = value;
		}

		public override string ToString() => Value;
	}
}
