namespace Content.Server.Mantrap;

[RegisterComponent]
public sealed class MantrapComponent : Component
{
    [ViewVariables]
    public bool IsActive;

    [DataField("chargingTime")]
     public float ChargingTime = 15f;
}
