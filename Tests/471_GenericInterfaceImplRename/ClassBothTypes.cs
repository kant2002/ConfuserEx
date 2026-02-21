namespace GenericInterfaceImplRename {
	public class ClassBothTypes : IInterfaceBothTypes<object> {
		public External Transform(External input) {
			return new External("Transformed:" + input.Value);
		}
	}
}
