using Content.Shared.Werewolf;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Werewolf;

[RegisterComponent]
public sealed class PupaComponent : SharedPupaComponent
{
    [DataField("pupaUid")] public EntityUid pupaUid;

    [DataField("werewolf")] public EntityUid werewolf;

    [ViewVariables(VVAccess.ReadWrite), DataField("accumulator")]
    public float Accumulator = 0f;

    [ViewVariables(VVAccess.ReadWrite), DataField("maxAccumuluator")]
    public float MaxAccumulator = 300f;

    public Container PupaContainer = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("PupaPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string PupaFinPrototype = "PupaFinEntity";
}
