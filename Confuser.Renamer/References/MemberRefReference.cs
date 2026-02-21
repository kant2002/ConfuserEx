using System.Text;
using Confuser.Core;
using dnlib.DotNet;

namespace Confuser.Renamer.References {
	public sealed class MemberRefReference : INameReference<IMemberDef> {
		readonly IMemberDef memberDef;
		readonly MemberRef memberRef;

		public bool ShouldCancelRename => false;

		public MemberRefReference(MemberRef memberRef, IMemberDef memberDef) {
			this.memberRef = memberRef;
			this.memberDef = memberDef;
		}

		/// <inheritdoc />
		public bool DelayRenaming(INameService service, IDnlibDef currentDef) => false;

		public bool UpdateNameReference(ConfuserContext context, INameService service) {
			var updated = false;

			if (!UTF8String.Equals(memberRef.Name, memberDef.Name)) {
				memberRef.Name = memberDef.Name;
				updated = true;
			}

			if (memberDef is MethodDef md && md.DeclaringType.HasGenericParameters)
				updated |= FixGenericInterfaceImplSignature(md);

			return updated;
		}

		private bool FixGenericInterfaceImplSignature(MethodDef md) {
			if (memberRef.MethodSig == null || md.MethodSig == null)
				return false;

			var updated = false;

			if (ShouldSyncType(md.MethodSig.RetType, memberRef.MethodSig.RetType)) {
				memberRef.MethodSig.RetType = md.MethodSig.RetType;
				updated = true;
			}

			for (var i = 0; i < memberRef.MethodSig.Params.Count && i < md.MethodSig.Params.Count; i++) {
				if (ShouldSyncType(md.MethodSig.Params[i], memberRef.MethodSig.Params[i])) {
					memberRef.MethodSig.Params[i] = md.MethodSig.Params[i];
					updated = true;
				}
			}

			return updated;
		}

		private static bool ShouldSyncType(TypeSig defType, TypeSig refType) {
			if (defType == null || refType == null)
				return false;
			if (!(defType is TypeDefOrRefSig) || !(refType is TypeDefOrRefSig))
				return false;
			return defType.FullName != refType.FullName;
		}

		public override string ToString() => ToString(null); 

		public string ToString(INameService nameService) {
			var builder = new StringBuilder();
			builder.Append("MemberRef Reference").Append("(");
			builder.Append("MemberRef").Append("(").AppendHashedIdentifier("Name", memberRef.Name).Append(")");
			builder.Append("; ");
			builder.AppendReferencedDef(memberDef, nameService);
			builder.Append(")");
			return builder.ToString();
		}
	}
}
