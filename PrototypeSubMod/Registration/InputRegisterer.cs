using Nautilus.Handlers;

namespace PrototypeSubMod.Registration;

public static class InputRegisterer
{
    public static GameInput.Button TetherSubButton;
    public static GameInput.Button TetherMarkerButton;
    
    public static void Register()
    {
        TetherSubButton = EnumHandler.AddEntry<GameInput.Button>("ProtoTetherSubButton")
            .CreateInput()
            .WithBinding(GameInput.Device.Keyboard, GameInputHandler.Paths.Keyboard.V)
            .WithBinding(GameInput.Device.Controller, GameInputHandler.Paths.Gamepad.DpadDown)
            .WithCategory("PrototypeInputCategory");
        
        TetherMarkerButton = EnumHandler.AddEntry<GameInput.Button>("ProtoTetherMarkerButton")
            .CreateInput()
            .WithBinding(GameInput.Device.Keyboard, GameInputHandler.Paths.Keyboard.X)
            .WithBinding(GameInput.Device.Controller, GameInputHandler.Paths.Gamepad.DpadUp)
            .WithCategory("PrototypeInputCategory");
    }
}