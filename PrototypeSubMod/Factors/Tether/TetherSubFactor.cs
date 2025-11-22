using PrototypeSubMod.Registration;

namespace PrototypeSubMod.Factors.Tether;

public class TetherSubFactor : Factor
{
    public override GameInput.Button GetUseButton() => InputRegisterer.TetherSubButton;

    public override void Use()
    {
        base.Use();
        ErrorMessage.AddError("Used sub tether");
    }
}