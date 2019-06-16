using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace Confuser.Protections.ControlFlow
{
    internal class ControlFlowPhase : ProtectionPhase
    {
        public ControlFlowPhase(ControlFlowProtection parent) : base(parent) { }

        public override ProtectionTargets Targets => ProtectionTargets.Methods;

        public override string Name => "Control Flow";

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            foreach (MethodDef method in parameters.Targets.OfType<MethodDef>().WithProgress(context.Logger))
            {
                if (!method.HasBody) continue;
                if (!method.Body.HasInstructions) continue;

                for (int i = 0; i < method.Body.Instructions.Count; i++)
                {
                    if (method.Body.Instructions[i].IsLdcI4())
                    {
                        int numorig = new Random(Guid.NewGuid().GetHashCode()).Next();
                        int div = new Random(Guid.NewGuid().GetHashCode()).Next();
                        int num = numorig ^ div;

                        Instruction nop = OpCodes.Nop.ToInstruction();

                        Local local = new Local(method.Module.ImportAsTypeSig(typeof(int)));
                        method.Body.Variables.Add(local);

                        method.Body.Instructions.Insert(i + 1, OpCodes.Stloc.ToInstruction(local));
                        method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldc_I4, method.Body.Instructions[i].GetLdcI4Value() - sizeof(float)));
                        method.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldc_I4, num));
                        method.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Ldc_I4, div));
                        method.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Xor));
                        method.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Ldc_I4, numorig));
                        method.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Bne_Un, nop));
                        method.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Ldc_I4, 2));
                        method.Body.Instructions.Insert(i + 9, OpCodes.Stloc.ToInstruction(local));
                        method.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Sizeof, method.Module.Import(typeof(float))));
                        method.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Add));
                        method.Body.Instructions.Insert(i + 12, nop);
                        i += 12;
                    }
                }
            }
        }
    }
}
