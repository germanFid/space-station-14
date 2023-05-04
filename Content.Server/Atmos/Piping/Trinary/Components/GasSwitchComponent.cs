using Robust.Shared.Audio;

namespace Content.Server.Atmos.Piping.Trinary.Components
{
    [RegisterComponent]
    public sealed class GasSwitchComponent : Component
    {
        [DataField("switch")]
        public bool Switch { get; set; } = true;

        [DataField("inlet1")]
        public string Inlet1Name { get; set; } = "inlet1";

        [DataField("inlet2")]
        public string Inlet2Name { get; set; } = "inlet2";

        [DataField("outlet")]
        public string OutletName { get; set; } = "outlet";

        [DataField("valveSound")]
        public SoundSpecifier ValveSound { get; } = new SoundCollectionSpecifier("valveSqueak");
    }
}
