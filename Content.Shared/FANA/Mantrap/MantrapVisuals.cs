using Robust.Shared.Serialization;

namespace Content.Shared.Mantrap;

[Serializable, NetSerializable]
public enum MantrapVisuals : byte
{
    Visual,
    Armed,
    Unarmed
}
