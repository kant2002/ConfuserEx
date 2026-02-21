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

			if (md.MethodSig.RetType != null &&
				memberRef.MethodSig.RetType != null &&
				md.MethodSig.RetType.FullName != memberRef.MethodSig.RetType.FullName) {
				memberRef.MethodSig.RetType = md.MethodSig.RetType;
				updated = true;
			}

			for (var i = 0; i < memberRef.MethodSig.Params.Count && i < md.MethodSig.Params.Count; i++) {
				var refParamType = memberRef.MethodSig.Params[i];
				var defParamType = md.MethodSig.Params[i];
				if (refParamType.FullName != defParamType.FullName) {
					memberRef.MethodSig.Params[i] = defParamType;
					updated = true;
				}
			}

			return updated;
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
