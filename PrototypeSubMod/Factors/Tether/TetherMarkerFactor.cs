using PrototypeSubMod.Registration;

namespace PrototypeSubMod.Factors.Tether;

public class TetherMarkerFactor : Factor
{
    public override GameInput.Button GetUseButton() => InputRegisterer.TetherMarkerButton;

    public override void Use()
    {
        base.Use();
        ErrorMessage.AddError("Used marker tether");
    }
}