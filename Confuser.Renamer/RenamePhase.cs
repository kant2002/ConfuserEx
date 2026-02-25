using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Pdb;

namespace Confuser.Renamer {
	class RenamePhase : ProtectionPhase {
		public RenamePhase(NameProtection parent)
			: base(parent) { }

		public override ProtectionTargets Targets => ProtectionTargets.AllDefinitions;

		public override string Name => "Renaming";

		protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {
			var service = (NameService)context.Registry.GetService<INameService>();

			context.Logger.Debug("Renaming...");
			foreach (var renamer in service.Renamers) {
				foreach (var def in parameters.Targets)
					renamer.PreRename(context, service, parameters, def);
				context.CheckCancellation();
			}

			var targets = parameters.Targets.ToList();
			service.GetRandom().Shuffle(targets);
			var pdbDocs = new HashSet<string>();
			foreach (var def in GetTargetsWithDelay(targets, context, service).WithProgress(targets.Count, context.Logger)) {
				if (def is ModuleDef moduleDef && parameters.GetParameter(context, moduleDef, "rickroll", false))
					RickRoller.CommenceRickroll(context, moduleDef);

				bool canRename = service.CanRename(def);
				var mode = service.GetRenameMode(def);

				if (def is MethodDef method) {
					if ((canRename || method.IsConstructor) && parameters.GetParameter(context, method, "renameArgs", true)) {
						foreach (var param in method.ParamDefs)
							param.Name = null;
					}

					if (method.HasBody) {
						if (parameters.GetParameter(context, method, "renPdb", false)) {
							foreach (var instr in method.Body.Instructions) {
								if (instr.SequencePoint != null && !pdbDocs.Contains(instr.SequencePoint.Document.Url)) {
									instr.SequencePoint.Document.Url = service.ObfuscateName(instr.SequencePoint.Document.Url, mode);
									pdbDocs.Add(instr.SequencePoint.Document.Url);
								}
							}
							foreach (var local in method.Body.Variables) {
								if (!string.IsNullOrEmpty(local.Name))
									local.Name = service.ObfuscateName(local.Name, mode);
							}

							if (method.Body.HasPdbMethod)
								method.Body.PdbMethod.Scope = new PdbScope();
						} else {
							// Earlier ConfuserEx remove location of the instructions, probably due ot bug
							// in the dnlib. We leave this behaviour since it make sence to hide location of the code.
							// If we want renamingof paths, we should explicitly opt-in using renPdb
							foreach (var instr in method.Body.Instructions) {
								if (instr.SequencePoint != null && !pdbDocs.Contains(instr.SequencePoint.Document.Url)) {
									instr.SequencePoint.Document = null;
								}
							}
						}
					}
				}

				if (!canRename)
					continue;

				service.SetIsRenamed(def);

				var references = service.GetReferences(def);
				bool cancel = references.Any(r => r.ShouldCancelRename);
				if (cancel)
					continue;

				if (def is TypeDef typeDef) {
					if (parameters.GetParameter(context, typeDef, "flatten", true)) {
						typeDef.Namespace = "";
					}
					else {
						var nsFormat = parameters.GetParameter(context, typeDef, "nsFormat", "{0}");
						typeDef.Namespace = service.ObfuscateName(nsFormat, typeDef.Namespace, mode);
					}
					typeDef.Name = service.ObfuscateName(typeDef, mode);
					RenameGenericParameters(typeDef.GenericParameters);
				}
				else if (def is MethodDef methodDef) {
					methodDef.Name = service.ObfuscateName(methodDef, mode);
					RenameGenericParameters(methodDef.GenericParameters);
				}
				else
					def.Name = service.ObfuscateName(def, mode);

				int updatedReferences = -1;
				do {
					var oldUpdatedCount = updatedReferences;
					// This resolves the changed name references and counts how many were changed.
					var updatedReferenceList = references.Where(refer => refer.UpdateNameReference(context, service)).ToArray();
					updatedReferences = updatedReferenceList.Length;
					if (updatedReferences == oldUpdatedCount) {
						var errorBuilder = new StringBuilder();
						errorBuilder.AppendLine("Infinite loop detected while resolving name references.");
						errorBuilder.Append("Processed definition: ").AppendDescription(def, service).AppendLine();
						errorBuilder.Append("Assembly: ").AppendLine(context.CurrentModule.FullName);
						errorBuilder.AppendLine("Faulty References:");
						foreach (var reference in updatedReferenceList) {
							errorBuilder.Append(" - ").AppendLine(reference.ToString(service));
						}
						context.Logger.Error(errorBuilder.ToString().Trim());
						throw new ConfuserException();
					}
					context.CheckCancellation();
				} while (updatedReferences > 0);
			}
		}

		static void RenameGenericParameters(IList<GenericParam> genericParams)
		{
			foreach (var param in genericParams)
				param.Name = ((char) (param.Number + 1)).ToString();
		}

		static IEnumerable<IDnlibDef> GetTargetsWithDelay(IList<IDnlibDef> definitions, ConfuserContext context, INameService service) {
			var delayedItems = new List<IDnlibDef>();
			var currentList = definitions;
			var lastCount = -1;
			while (currentList.Any()) {
				foreach (var def in currentList) {
					if (service.GetReferences(def).Any(r => r.DelayRenaming(service, def)))
						delayedItems.Add(def);
					else
						yield return def;
				}

				if (delayedItems.Count == lastCount) {
					var errorBuilder = new StringBuilder();
					errorBuilder.AppendLine("Failed to rename all targeted members, because the references are blocking each other.");
					errorBuilder.AppendLine("Remaining definitions: ");
					foreach (var def in delayedItems) {
						errorBuilder.Append("• ").AppendDescription(def, service).AppendLine();
					}
					context.Logger.Warn(errorBuilder.ToString().Trim());
					yield break;
				}
				lastCount = delayedItems.Count;
				currentList = delayedItems;
				delayedItems = new List<IDnlibDef>();
			}
		}
	}
}
