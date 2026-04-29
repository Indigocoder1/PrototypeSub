using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.UI.AbilitySelection;

public class RadialIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Color enabledCol = Color.white;
    [SerializeField] private Color disabledCol = Color.black;
    [SerializeField] private Color notInstalledCol = Color.black;
    [SerializeField] private float colorTransitionSpeed = 1;
    [SerializeField] private float hoverScale;
    [SerializeField] private float scaleSpeed;
    [SerializeField] private float targetTransitionSpeed = 1;

    private IAbilityIcon ability;
    private bool hovered;
    private bool selected;
    private float originalScale;
    private float targetScale;
    private float currentScale;

    private void Start()
    {
        originalScale = transform.localScale.x;
        targetScale = originalScale;
        currentScale = originalScale;
        image.color = !ability.GetIsInstalled() ? notInstalledCol : selected ? enabledCol : disabledCol;
    }

    private void Update()
    {
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * targetTransitionSpeed);
        float scale = Mathf.Lerp(transform.localScale.x, currentScale, Time.deltaTime * scaleSpeed);
        transform.localScale = Vector3.one * scale;

        var targetCol = !ability.GetIsInstalled() ? notInstalledCol : selected ? enabledCol : disabledCol;
        image.color = Color.Lerp(image.color, targetCol, Time.deltaTime * colorTransitionSpeed);
    }

    public void SetAbility(IAbilityIcon ability)
    {
        this.ability = ability;
        image.sprite = ability.GetSprite();

        if (!ability.GetIsInstalled())
        {
            image.color = notInstalledCol;
        }
    }

    public IAbilityIcon GetAbility() => ability;

    public void OnTetherEnter()
    {
        hovered = true;
        targetScale = hoverScale;
    }

    public void OnTetherExit()
    {
        hovered = false;
        targetScale = originalScale;
    }

    public void Select()
    {
        currentScale = 1f;
        selected = true;
        ability.OnSelectedChanged(true);
    }

    public void ForceColorSwap()
    {
        image.color = selected ? enabledCol : disabledCol;
    }

    public void Deselect()
    {
        selected = false;
        ability.OnSelectedChanged(false);
    }

    public bool Activate() => ability.OnActivated();
    public bool GetHovering() => hovered;
}
