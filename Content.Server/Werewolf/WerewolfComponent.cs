using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Audio;
using Content.Shared.Damage;

namespace Content.Server.Werewolf
{
    [RegisterComponent]
    public sealed class WerewolfComponent : Component
    {
        [DataField("pupationActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
        public string PupationActionId = "WerewolfPupation";

        [DataField("pupationAction")]
        public EntityTargetAction? PupationAction;

        [ViewVariables(VVAccess.ReadWrite), DataField("soundPupation")]
        public SoundSpecifier? SoundPupation = new SoundPathSpecifier("/Audio/Werewolf/pupateactsound.ogg");

        [DataField("eatPupaActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
        public string EatPupaActionId = "WerewolfEatPupa";

        [DataField("eatPupaAction")]
        public EntityTargetAction? EatPupaAction;

        [ViewVariables(VVAccess.ReadWrite), DataField("SoundPupaEat")]
        public SoundSpecifier? SoundPupaEat = new SoundPathSpecifier("/Audio/Werewolf/pupaeatsound.ogg");

        [DataField("eatPupaTime")]
        public float eatPupaTime = 15f;

        [DataField("PupationTime")]
        public float PupationTime = 20f;

        [DataField("transformationActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
        public string TransformationActionId = "WerewolfTransformation";

        [DataField("transformationAction")]
        public InstantAction? TransformationAction;

        [DataField("werewolfEvolveActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
        public string EvolveActionId = "WerewolfEvolveAction";

        [DataField("werewolfEvolveAction")]
        public InstantAction? WerewolfEvolveAction;

        [ViewVariables(VVAccess.ReadWrite), DataField("PupaPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PupaPrototype = "PupaEntity";

        [DataField("PupasEaten")]
        public int PupasEaten = 0;

        [DataField("humanUid")]
        public EntityUid HumanUid;

        [ViewVariables(VVAccess.ReadWrite), DataField("PupationWhitelist")]
        public EntityWhitelist? PupationWhitelist = new()
        {
            Components = new[]
            {
                "MobState",
            },
        };
        [ViewVariables(VVAccess.ReadWrite), DataField("PupaEatWhitelist")]
        public EntityWhitelist? PupaEatWhitelist = new()
        {
            Components = new[]
            {
                "Pupa",
            },

        };

    }

    public sealed class WerewolfEatPupaActionEvent : EntityTargetActionEvent {}
    public sealed class WerewolfPupationActionEvent : EntityTargetActionEvent {}
    public sealed class WerewolfTransformationInHumanEvent : InstantActionEvent {}
    public sealed class WerewolfEvolveActionEvent : InstantActionEvent{}
    public sealed class WerewolfDestroyWallsEvent : EntityTargetActionEvent{}
}
