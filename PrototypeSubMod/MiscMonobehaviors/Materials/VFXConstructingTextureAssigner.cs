using System;
using SubLibrary.CyclopsReferencers;
using SubLibrary.Handlers;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class VFXConstructingTextureAssigner : MonoBehaviour, ICyclopsReferencer
{
    private bool textureAssigned;
    
    private void Start()
    {
        if (CyclopsReferenceHandler.CyclopsReference)
        {
            ApplyTexture(CyclopsReferenceHandler.CyclopsReference);
        }
    }

    public void OnCyclopsReferenceFinished(GameObject cyclops)
    {
        ApplyTexture(cyclops);
    }

    private void ApplyTexture(GameObject cyclops)
    {
        if (textureAssigned) return;
        
        var vfxConstruct = GetComponent<VFXConstructing>();
        var cyclopsConstruct = cyclops.GetComponent<VFXConstructing>();
        
        vfxConstruct.alphaTexture = cyclopsConstruct.alphaTexture;
        vfxConstruct.alphaDetailTexture = cyclopsConstruct.alphaDetailTexture;

        textureAssigned = true;
    }
}