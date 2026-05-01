using System;
using PrototypeSubMod.DestructionEvent;
using PrototypeSubMod.Facilities.Hull.WyrmActions;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class WyrmSubSequenceManager : MonoBehaviour
{
    [SerializeField] private VoiceNotificationManager voiceNotificationManager;
    [SerializeField] private VoiceNotification wyrmFirstInteractionVoiceline;
    [SerializeField] private EmissionColorController emissionColorController;
    [SerializeField] private Color colorDuringSequence;

    private void Start()
    {
        WyrmFirstEncounterManager.OnFirstEncounterStarted += OnSequenceStarted;
        WyrmFirstEncounterManager.OnFirstEncounterEnded += OnSequenceEnded;
        ProtoDestructionEvent.OnSubDestroyed += OnSequenceEnded;
    }

    private void OnSequenceStarted()
    {
        voiceNotificationManager.PlayVoiceNotification(wyrmFirstInteractionVoiceline);
        emissionColorController.RegisterTempColor(this,
            new EmissionColorController.EmissionRegistrarData(colorDuringSequence, 20));
    }

    private void OnSequenceEnded()
    {
        emissionColorController.RemoveTempColor(this);
    }

    private void OnDestroy()
    {
        WyrmFirstEncounterManager.OnFirstEncounterStarted -= OnSequenceStarted;
        WyrmFirstEncounterManager.OnFirstEncounterEnded -= OnSequenceEnded;
        ProtoDestructionEvent.OnSubDestroyed -= OnSequenceEnded;
    }
}