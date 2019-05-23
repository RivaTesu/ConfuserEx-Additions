using System;
using System.Linq;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    internal class HideCallsProtection : Protection // Maybe you need adapt other protections (Control Flow).
    {
        public const string _Id = "Hide Calls";
        public const string _FullId = "Ki.Hcs";

        public override string Name
        {
            get
            {
                return "Hide Calls Protection";
            }
        }

        public override string Description
        {
            get
            {
                return "This protection crash .cctor.";
            }
        }

        public override string Id
        {
            get
            {
                return _Id;
            }
        }

        public override string FullId
        {
            get
            {
                return _FullId;
            }
        }

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
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new HideCallsPhase(this));
        }

        class HideCallsPhase : ProtectionPhase
        {
            public HideCallsPhase(HideCallsProtection parent) : base(parent)
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
                    return "Hide Calls Injection";
                }
            }

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
                {
                    MethodDef method = module.GlobalType.FindStaticConstructor();

                    Local Sugar = new Local(module.Import(typeof(int)).ToTypeSig());
                    Local Sugar_2 = new Local(module.Import(typeof(bool)).ToTypeSig());

                    method.Body.Variables.Add(Sugar);
                    method.Body.Variables.Add(Sugar_2);

                    Instruction operand = null;
                    Instruction instruction = new Instruction(OpCodes.Ret);
                    Instruction instruction2 = new Instruction(OpCodes.Ldc_I4_1);

                    method.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
                    method.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, Sugar));
                    method.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction2));

                    Instruction instruction3 = new Instruction(OpCodes.Ldloc, Sugar);

                    method.Body.Instructions.Insert(3, instruction3);
                    method.Body.Instructions.Insert(4, new Instruction(OpCodes.Ldc_I4_0));
                    method.Body.Instructions.Insert(5, new Instruction(OpCodes.Ceq));
                    method.Body.Instructions.Insert(6, new Instruction(OpCodes.Ldc_I4_1));
                    method.Body.Instructions.Insert(7, new Instruction(OpCodes.Ceq));
                    method.Body.Instructions.Insert(8, new Instruction(OpCodes.Stloc, Sugar_2));
                    method.Body.Instructions.Insert(9, new Instruction(OpCodes.Ldloc, Sugar_2));
                    method.Body.Instructions.Insert(10, new Instruction(OpCodes.Brtrue, method.Body.Instructions[sizeof(Decimal) - 6]));
                    method.Body.Instructions.Insert(11, new Instruction(OpCodes.Ret));
                    method.Body.Instructions.Insert(12, new Instruction(OpCodes.Calli));
                    method.Body.Instructions.Insert(13, new Instruction(OpCodes.Sizeof, operand));
                    method.Body.Instructions.Insert(method.Body.Instructions.Count, instruction2);
                    method.Body.Instructions.Insert(method.Body.Instructions.Count, new Instruction(OpCodes.Stloc, Sugar_2));
                    method.Body.Instructions.Insert(method.Body.Instructions.Count, new Instruction(OpCodes.Br, instruction3));
                    method.Body.Instructions.Insert(method.Body.Instructions.Count, instruction);

                    ExceptionHandler item2 = new ExceptionHandler(ExceptionHandlerType.Finally)
                    {
                        HandlerStart = method.Body.Instructions[10],
                        HandlerEnd = method.Body.Instructions[11],
                        TryEnd = method.Body.Instructions[14],
                        TryStart = method.Body.Instructions[12]
                    };

                    bool flag3 = !method.Body.HasExceptionHandlers;

                    if (flag3)
                    {
                        method.Body.ExceptionHandlers.Add(item2);
                    }

                    operand = new Instruction(OpCodes.Br, instruction);
                    method.Body.OptimizeBranches();
                    method.Body.OptimizeMacros();
                }
            }
        }
    }
}