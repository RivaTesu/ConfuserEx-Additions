using System;
using System.Linq;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    [BeforeProtection(new string[]
    {
        "Ki.ControlFlow"
    })]

    internal class MutateConstantsProtection : Protection
    {
        public override string Name
        {
            get
            {
                return "Mutate Constants";
            }
        }

        public override string Description
        {
            get
            {
                return "This protection mutate Contants with sizeofs.";
            }
        }

        public override string Id
        {
            get
            {
                return "Mutate Constants";
            }
        }

        public override string FullId
        {
            get
            {
                return "Ki.MutateConstants";
            }
        }

        public const string _Id = "Mutate Constants";

        public const string _FullId = "Ki.MutateConstants";

        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Normal;
            }
        }

        protected override void Initialize(ConfuserContext context)
        {
            // Null
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new MutateConstantsPhase(this));
        }

        private class MutateConstantsPhase : ProtectionPhase
        {
            public MutateConstantsPhase(MutateConstantsProtection parent) : base(parent)
            {
                // Null
            }

            public override ProtectionTargets Targets
            {
                get
                {
                    return ProtectionTargets.Modules;
                }
            }

            public override string Name
            {
                get
                {
                    return "Mutating Constants";
                }
            }

            public CilBody body;
            public static Random rnd = new Random();

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                foreach (ModuleDef moduleDef in parameters.Targets.OfType<ModuleDef>())
                {
                    foreach (TypeDef typeDef in moduleDef.Types)
                    {
                        foreach (MethodDef methodDef in typeDef.Methods)
                        {
                            if (methodDef.HasBody && methodDef.Body.HasInstructions)
                            {
                                for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                                {
                                    if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4)
                                    {
                                        body = methodDef.Body;
                                        int ldcI4Value = body.Instructions[i].GetLdcI4Value();
                                        int num = rnd.Next(1, 4);
                                        int num2 = ldcI4Value - num;
                                        body.Instructions[i].Operand = num2;
                                        Mutate(i, num, num2, moduleDef);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void Mutate(int i, int sub, int num2, ModuleDef module)
            {
                switch (sub)
                {
                    case 1:
                        body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
                        body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Add));
                        return;
                    case 2:
                        body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
                        body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
                        body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Add));
                        body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Add));
                        return;
                    case 3:
                        body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(int))));
                        body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(byte))));
                        body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Sub));
                        body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Add));
                        return;
                    case 4:
                        body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(decimal))));
                        body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(GCCollectionMode))));
                        body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Sub));
                        body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Sizeof, module.Import(typeof(int))));
                        body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Sub));
                        body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Add));
                        return;
                    default:
                        return;
                }
            }
        }
    }
}