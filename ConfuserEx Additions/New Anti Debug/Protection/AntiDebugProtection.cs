using System.Collections.Generic;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    [BeforeProtection("Ki.ControlFlow")]

    internal class AntiDebugProtection : Protection
    {
        public const string _Id = "Anti Debug";
        public const string _FullId = "Ki.AntiDebug";

        public override string Name
        {
            get
            {
                return "Anti Debug Protection";
            }
        }

        public override string Description
        {
            get
            {
                return "This protection prevents debugging.";
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
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new AntiDebugPhase(this));
        }

        class AntiDebugPhase : ProtectionPhase
        {
            public AntiDebugPhase(AntiDebugProtection parent) : base(parent)
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
                    return "Anti Debug Injection";
                }
            }

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                var rtType = context.Registry.GetService<IRuntimeService>().GetRuntimeType("Confuser.Runtime.AntiDebug");
                var marker = context.Registry.GetService<IMarkerService>();
                var name = context.Registry.GetService<INameService>();

                foreach (var module in parameters.Targets.OfType<ModuleDef>())
                {
                    var members = InjectHelper.Inject(rtType, module.GlobalType, module);
                    var cctor = module.GlobalType.FindStaticConstructor();
                    var init = (MethodDef)members.Single(method => method.Name == "Initialize");

                    cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));

                    foreach (var member in members)
                        name.MarkHelper(member, marker, (Protection)Parent);
                }
            }
        }
    }
}
