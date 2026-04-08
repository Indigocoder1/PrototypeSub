using HarmonyLib;
using Nautilus.Handlers;
using PrototypeSubMod.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UWE;
using static PrecursorKeyTerminal;

namespace PrototypeSubMod.Facilities;

internal class MultipurposeKeyTerminal : InteractableTerminal
{
    [SaveStateReference]
    private static GameObject _keyTerminalPrefab;

    public override event Action onTerminalInteracted
    {
        add => onInteracted.AddListener(new UnityAction(value));
        remove => onInteracted.RemoveListener(new UnityAction(value));
    }
    
    [SerializeField] private string techType;
    [SerializeField] private Texture2D replacementSprite;
    [SerializeField] private UnityEvent onInteracted;

    private PrecursorKeyTerminal terminal;
    private bool queuedForceInteract;
    private bool interacted;
    
    private void Start()
    {
        if (_keyTerminalPrefab)
        {
            SpawnPrefab(_keyTerminalPrefab);
            return;
        }

        CoroutineHost.StartCoroutine(RetrievePrefab());
    }

    private IEnumerator RetrievePrefab()
    {
        var prefabRequest = PrefabDatabase.GetPrefabAsync("c718547d-fe06-4247-86d0-efd1e3747af0");

        yield return prefabRequest;

        GameObject prefab = null;
        if (!prefabRequest.TryGetPrefab(out prefab)) throw new Exception("Error retrieving precursor key terminal prefab!");

        prefab.SetActive(false);
        _keyTerminalPrefab = prefab;
        SpawnPrefab(_keyTerminalPrefab);
    }

    private void SpawnPrefab(GameObject prefab)
    {
        TechType techType = TechType.None;
        try
        {
            techType = (TechType)Enum.Parse(typeof(TechType), this.techType);
        }
        catch (Exception e)
        {
            throw e;
        }

        var keyType = CreateOrRetrieveKeyType(techType);
        var instance = Instantiate(prefab, transform);

        instance.transform.SetParent(transform, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.SetActive(true);

        terminal = instance.GetComponent<PrecursorKeyTerminal>();
        instance.GetComponent<TechTag>().type = TechType.None;
        terminal.acceptKeyType = keyType;
        var glyphRenderer = terminal.transform.Find("Precursor_key_terminal_01/glyph/Face_F").GetComponent<Renderer>();

        glyphRenderer.material.mainTexture = replacementSprite;

        DestroyImmediate(instance.GetComponent<PrefabIdentifier>());
        DestroyImmediate(instance.GetComponent<LargeWorldEntity>());

        var applier = GetComponentInParent<SkyApplier>();
        if (applier)
        {
            applier.renderers.AddRangeToArray(GetComponentsInChildren<Renderer>(true));
            applier.ApplySkybox();
        }

        if (queuedForceInteract)
        {
            terminal.slotted = true;
            terminal.CloseDeck();
            ToggleDoor();
        }
    }

    private PrecursorKeyType CreateOrRetrieveKeyType(TechType type)
    {
        PrecursorKeyType value;
        try
        {
            value = (PrecursorKeyType)Enum.Parse(typeof(PrecursorKeyType), type.ToString());
            return value;
        }
        catch
        {
            var builder = EnumHandler.AddEntry<PrecursorKeyType>(type.ToString());
            return builder.Value;
        }
    }

    // Called via BroadcastMessage on PrecursorKeyTerminal
    public void ToggleDoor()
    {
        if (interacted) return;
        
        onInteracted?.Invoke();
        interacted = true;
    }
    
    public override void ForceInteracted()
    {
        if (terminal)
        {
            terminal.slotted = true;
            terminal.CloseDeck();
            ToggleDoor();
            return;
        }

        queuedForceInteract = true;
    }
}
