using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.EngineLever;
using PrototypeSubMod.MotorHandler;
using PrototypeSubMod.SaveData;
using SubLibrary.SaveData;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class ProtoFinsManager : MonoBehaviour, ISaveDataListener
{
    public const int FinsForDocking = 1;
    
    private static readonly int EngineOn = Animator.StringToHash("FinsActive");
    private static readonly int ResetAnimState = Animator.StringToHash("ResetAnimState");
    private static readonly int TransmissionDeactivated = Animator.StringToHash("TransmissionDeactivated");

    public event Action OnFinCountChanged;

    [SerializeField] private GameObject dockingBay;
    [SerializeField] private GameObject[] leftFins;
    [SerializeField] private GameObject[] rightFins;
    [SerializeField] private ProtoMotorHandler motorHandler;
    [SerializeField] private CyclopsMotorMode motorMode;
    [SerializeField] private ProtoEngineLever engineLever;
    [SerializeField] private CrushDamage crushDamage;
    [SerializeField] private float[] noiseValues;
    [SerializeField] private float multiplierIncreasePerFin = 0.5f;
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float depthIncreasePerFin;

    private Animator[] leftFinAnimators;
    private Animator[] rightFinAnimators;
    private int installedFinCount;
    private bool transmissionDeactivated;

    private void Start()
    {
        Initialize();
        
        UpdateFinStatus();
        UpdateDockingBayStatus();
        engineLever.onEngineStateChanged += _ => UpdateDockingBayStatus();
        engineLever.onEngineStateChanged += _ => UpdateFinStatus();
    }

    private void Initialize()
    {
        if (leftFinAnimators != null) return;
        
        leftFinAnimators = new Animator[leftFins.Length];
        rightFinAnimators = new Animator[rightFins.Length];
        for (int i = 0; i < leftFins.Length; i++)
        {
            leftFinAnimators[i] = leftFins[i].GetComponent<Animator>();
        }
        
        for (int i = 0; i < rightFins.Length; i++)
        {
            rightFinAnimators[i] = rightFins[i].GetComponent<Animator>();
        }
    }
    
    public void OnSaveDataLoaded(BaseSubDataClass saveData)
    {
        Initialize();
        
        installedFinCount = saveData.EnsureAsPrototypeData().installedFinCount;
        UpdateFinStatus();
    }

    public void OnBeforeDataSaved(ref BaseSubDataClass saveData)
    {
        saveData.EnsureAsPrototypeData().installedFinCount = installedFinCount;
    }
    
    public int GetInstalledFinCount() => installedFinCount;

    public void SetInstalledFinCount(int count)
    {
        installedFinCount = count;
        UpdateFinStatus();
        OnFinCountChanged?.Invoke();
    }

    public void SetTransmissionDeactivated(bool deactivated)
    {
        transmissionDeactivated = deactivated;
        UpdateFinStatus();
    }

    private void UpdateFinStatus()
    {
        for (int i = 0; i < leftFins.Length; i++)
        {
            leftFins[i].SetActive(i < installedFinCount);
            rightFins[i].SetActive(i < installedFinCount);
        }

        UpdateMotorBonuses();

        UpdateDockingBayStatus();
        UWE.CoroutineHost.StartCoroutine(UpdateFinAnimations(motorMode.engineOn || motorMode.engineOnOldState));
    }

    public void UpdateMotorBonuses()
    {
        motorHandler.AddSpeedBonus(new ProtoMotorHandler.ValueRegistrar(this, defaultSpeed + installedFinCount * multiplierIncreasePerFin));
        motorMode.motorModeNoiseValues[1] = noiseValues[installedFinCount];
        if (installedFinCount > 0)
        {
            crushDamage.SetExtraCrushDepth(installedFinCount * depthIncreasePerFin);
        }
    }

    public void ResetFinAnimations()
    {
        UWE.CoroutineHost.StartCoroutine(UpdateFinAnimations(motorMode.engineOn || motorMode.engineOnOldState));
    }
    
    private IEnumerator UpdateFinAnimations(bool targetState)
    {
        for (int i = 0; i < leftFinAnimators.Length; i++)
        {
            var animL = leftFinAnimators[i];
            var animR = rightFinAnimators[i];
            animL.SetBool(EngineOn, targetState);
            animR.SetBool(EngineOn, targetState);
            
            animL.SetBool(TransmissionDeactivated, transmissionDeactivated);
            animR.SetBool(TransmissionDeactivated, transmissionDeactivated);

            animL.SetTrigger(ResetAnimState);
            animR.SetTrigger(ResetAnimState);
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpdateDockingBayStatus()
    {
        dockingBay.SetActive(installedFinCount >= FinsForDocking);
    }
}