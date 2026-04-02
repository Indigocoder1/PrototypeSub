using PrototypeSubMod.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.DestructionEvent;

internal class ProtoSaveStateManager : MonoBehaviour, IProtoEventListener
{
    [SaveStateReference]
    public static List<ProtoSaveStateManager> DestroyedManagers = new();

    [SerializeField] private SubRoot root;

    private void Awake()
    {
        DestroyedManagers ??= new();

        UpdateManagerStatus();
    }

    private void OnEnable()
    {
        if (!Plugin.GlobalSaveData.prototypeDestroyed)
        {
            Plugin.GlobalSaveData.prototypePresent = true;
            DestroyedManagers.Remove(this);
        }
    }

    private void OnDisable()
    {
        Plugin.GlobalSaveData.prototypePresent = false;
        UpdateManagerStatus();
    }

    public void UpdateManagerStatus()
    {
        if (Plugin.GlobalSaveData.prototypeDestroyed && !DestroyedManagers.Contains(this))
        {
            DestroyedManagers.Add(this);
        }
        else if (!Plugin.GlobalSaveData.prototypeDestroyed && DestroyedManagers.Contains(this))
        {
            DestroyedManagers.Remove(this);
        }
    }

    public GameObject GetSubRoot()
    {
        return root.gameObject;
    }

    public bool SubDestroyed() => Plugin.GlobalSaveData.prototypeDestroyed;

    public void OnProtoSerialize(ProtobufSerializer serializer)
    {
        UpdateManagerStatus();
    }

    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        UpdateManagerStatus();
    }

    private void OnDestroy()
    {
        DestroyedManagers.Remove(this);
    }
}
