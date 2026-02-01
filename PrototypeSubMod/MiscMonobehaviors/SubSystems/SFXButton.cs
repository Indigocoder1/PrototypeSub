using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class SFXButton : Button
{
    [SerializeField]
    public UnityEvent onClickWrapper;
    public FMODAsset onEnterFX;
    public FMODAsset onExitFX;
    public FMODAsset onClickFX;
    public float volume = 1;
    public float minDistForSound = 2;

    private int layerMask;
    private bool wasOutOfRange;
    private bool mouseOnObject;
    private bool onPDA;

    private RaycastHit[] hitInfos = new RaycastHit[4];
    
    private void Awake()
    {
        layerMask = int.MaxValue;
        layerMask |= ~(1 << LayerID.Trigger);
        onClick.AddListener(() =>
        {
            if ((Player.main.transform.position - transform.position).sqrMagnitude >
                minDistForSound * minDistForSound) return;
            
            onClickWrapper?.Invoke();
        });
    }

    private void Start()
    {
        StartCoroutine(UpdateHoverDistance());
        OnPointerExit(new PointerEventData(EventSystem.current));

        onPDA = GetComponentInParent<uGUI_PDA>() != null;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        mouseOnObject = true;
        
        if (ButtonBlocked()) return;
        
        if (!gameObject.activeSelf) return;
        
        if ((Player.main.transform.position - transform.position).sqrMagnitude >
            minDistForSound * minDistForSound) return;
        
        base.OnPointerEnter(eventData);
        
        if (onEnterFX != null)
        {
            FMODUWE.PlayOneShot(onEnterFX, transform.position, volume);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!Player.main) return;
        
        base.OnPointerExit(new PointerEventData(EventSystem.current));
        
        mouseOnObject = false;
        if (!gameObject.activeSelf) return;
        
        if ((Player.main.transform.position - transform.position).sqrMagnitude > minDistForSound * minDistForSound)
        {
            return;
        }
        
        if (onExitFX != null)
        {
            FMODUWE.PlayOneShot(onExitFX, transform.position, volume);
        }
    }
    
    public override void OnPointerClick(PointerEventData eventData) { }

    private void OnClick()
    {
        if (ButtonBlocked()) return;
        
        if (!gameObject.activeSelf) return;
        
        if ((Player.main.transform.position - transform.position).sqrMagnitude >
            minDistForSound * minDistForSound) return;

        base.OnPointerClick(new PointerEventData(EventSystem.current));

        if (onClickFX != null)
        {
            FMODUWE.PlayOneShot(onClickFX, transform.position, volume);
        }
    }
    
    private void OnUpdate()
    {
        if (!wasOutOfRange && mouseOnObject && GameInput.GetButtonDown(GameInput.Button.LeftHand))
        {
            OnClick();
        }
    }

    private bool ButtonBlocked()
    {
        if (!Player.main) return true;
        
        if (onPDA) return false;
        
        var dir = Camera.main.transform.position - transform.position;
        var count = Physics.RaycastNonAlloc(transform.position + dir.normalized * 0.1f, dir, hitInfos, dir.magnitude, layerMask);
        if (count == 0) return false;

        for (int i = 0; i < count; i++)
        {
            if (hitInfos[i].collider.isTrigger) continue;
            
            if (hitInfos[i].collider.gameObject == Player.main.gameObject) continue;

            return true;
        }
        
        return false;
    }
    
    private IEnumerator UpdateHoverDistance()
    {
        while (gameObject.activeInHierarchy)
        {
            if (Player.main == null) yield break;
            
            yield return new WaitForSeconds(1);
            bool outOfRange = (Player.main.transform.position - transform.position).sqrMagnitude >
                              minDistForSound * minDistForSound;
            
            if (wasOutOfRange == outOfRange) continue;
            
            if (outOfRange)
            {
                ((Image)targetGraphic).overrideSprite = null;
            }
            else if (mouseOnObject)
            {
                OnPointerEnter(new PointerEventData(EventSystem.current));
            }
            
            wasOutOfRange = outOfRange;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateHoverDistance());
        ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
    }

    private void OnDisable()
    {
        OnPointerExit(new PointerEventData(EventSystem.current));
        ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
    }
}