using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using PrototypeSubMod.Patches;
using PrototypeSubMod.PrecursorWearables;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class ColorFactor : MonoBehaviour
{
    private static readonly string ConfigFolder = Path.Combine(Path.GetDirectoryName(Paths.BepInExConfigPath), Plugin.Assembly.GetName().Name);
    private static readonly string SuitColorsPath = Path.Combine(ConfigFolder, "SuitColors.json");

    private SuitColors suitColors;
    private PrecursorSuitManager suitManager;
    private Pickupable pickupable;
    private float intensity = 1;
    private bool equipped;
    private bool editingColor = true;
    private int currentColorIndex;
    
    private void Awake()
    {
        if (!Directory.Exists(ConfigFolder) || !File.Exists(SuitColorsPath))
        {
            CreateDefaultColors();
        }

        suitColors = LoadSuitColors();
        pickupable = GetComponent<Pickupable>();
        suitManager = Player.main.GetComponent<PrecursorSuitManager>();
        
        Inventory.main.equipment.onEquip += OnEquip;
        Inventory.main.equipment.onUnequip += OnUnequip;
        TooltipFactory_Patches.onRunItemActions += UpdateFromUI;
    }

    private void OnEquip(string slot, InventoryItem item)
    {
        if (pickupable.inventoryItem != item) return;

        equipped = true;
        UpdateSuitColor();
    }

    private void OnUnequip(string slot, InventoryItem item)
    {
        if (pickupable.inventoryItem != item) return;

        equipped = false;
        suitManager.DeregisterEmissionController(this);
    }

    public Color GetCurrentColor()
    {
        return suitColors.suitColorDatas[currentColorIndex].color;
    }
    
    public string GetCurrentLocalizationKey()
    {
        return suitColors.suitColorDatas[currentColorIndex].localizationKey;
    }

    public bool GetIsEditingColor() => editingColor;
    public float GetIntensity() => intensity;

    public GameInput.Button GetNextButton()
    {
        return GameInput.PrimaryDevice == GameInput.Device.Controller
            ? GameInput.Button.RightHand
            : GameInput.Button.CycleNext;
    }

    public GameInput.Button GetPrevButton()
    {
        return GameInput.PrimaryDevice == GameInput.Device.Controller
            ? GameInput.Button.LeftHand
            : GameInput.Button.CyclePrev;
    }

    private void UpdateFromUI()
    {
        if (IngameMenu.main.selected) return;
        
        if (GameInput.GetButtonDown(GetNextButton()))
        {
            if (editingColor) HandleColorChange(1);
            else HandleIncrementChange(1);
            
        }
        else if (GameInput.GetButtonDown(GetPrevButton()))
        {
            if (editingColor) HandleColorChange(-1);
            else HandleIncrementChange(-1);
        }

        if (GameInput.GetButtonDown(GameInput.Button.AltTool))
        {
            editingColor = !editingColor;
        }
    }

    private void HandleColorChange(int direction)
    {
        currentColorIndex += direction;
        
        int colorDataCount = suitColors.suitColorDatas.Count;
        if (currentColorIndex < 0)
        {
            currentColorIndex = colorDataCount - 1;
        }
        
        currentColorIndex %= colorDataCount;

        if (equipped) UpdateSuitColor();
    }

    private void HandleIncrementChange(int direction)
    {
        intensity += 0.25f * direction;
        intensity = Mathf.Clamp(intensity, 0, 5);
        UpdateSuitColor();
    }

    private void UpdateSuitColor()
    {
        suitManager.RegisterEmissionController(this,
            new PrecursorSuitManager.EmissionController(GetCurrentColor(), intensity, 5));
    }

    private void OnDestroy()
    {
        Inventory.main.equipment.onEquip -= OnEquip;
        Inventory.main.equipment.onUnequip -= OnUnequip;
        TooltipFactory_Patches.onRunItemActions -= UpdateFromUI;
    }

    private static void CreateDefaultColors()
    {
        var green = new SuitColorData("SuitColorGreen", Color.green);
        var white = new SuitColorData("SuitColorWhite", Color.white);
        var black = new SuitColorData("SuitColorBlack", Color.black);
        var blue = new SuitColorData("SuitColorBlue", Color.blue);
        var cyan = new SuitColorData("SuitColorCyan", Color.cyan);
        var magenta = new SuitColorData("SuitColorMagenta", Color.magenta);
        var red = new SuitColorData("SuitColorRed", Color.red);
        var yellow = new SuitColorData("SuitColorYellow", Color.yellow);

        var colors = new SuitColors(green, white, black, blue, cyan, magenta, red, yellow);
        var jsonData = JsonConvert.SerializeObject(colors, Formatting.Indented);
        
        Directory.CreateDirectory(ConfigFolder);
        File.WriteAllText(SuitColorsPath, jsonData);
    }

    private static SuitColors LoadSuitColors()
    {
        var jsonData = File.ReadAllText(SuitColorsPath);
        return JsonConvert.DeserializeObject<SuitColors>(jsonData);
    }
}

public class SuitColors
{
    public List<SuitColorData> suitColorDatas;

    [JsonConstructor]
    public SuitColors(List<SuitColorData> suitColorDatas)
    {
        this.suitColorDatas = suitColorDatas;
    }
    
    public SuitColors(params SuitColorData[] suitColorDatas)
    {
        this.suitColorDatas = suitColorDatas.ToList();
    }
}

public struct SuitColorData
{
    public string localizationKey;
    public SerializableColor color;

    public SuitColorData(string localizationKey, SerializableColor color)
    {
        this.localizationKey = localizationKey;
        this.color = color;
    }
}

public struct SerializableColor
{
    [JsonConstructor]
    public SerializableColor(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
    
    public SerializableColor(Color color)
    {
        r = color.r;
        g = color.g;
        b = color.b;
    }

    public static implicit operator Color(SerializableColor color)
    {
        return new Color(color.r, color.g, color.b);
    }

    public static implicit operator SerializableColor(Color color)
    {
        return new SerializableColor(color);
    }

    public float r;
    public float g;
    public float b;
}