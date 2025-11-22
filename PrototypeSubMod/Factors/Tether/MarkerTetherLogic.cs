using PrototypeSubMod.Registration;

namespace PrototypeSubMod.Factors.Tether;

public class MarkerTetherLogic : Factor
{
    public override GameInput.Button GetUseButton() => InputRegisterer.TetherMarkerButton;

    public override void StartUse()
    {
        base.StartUse();
        ErrorMessage.AddError("Used marker tether");
    }
}