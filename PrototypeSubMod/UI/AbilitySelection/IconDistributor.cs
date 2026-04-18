using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.UI.AbilitySelection;

public class IconDistributor : MonoBehaviour
{
    [SerializeField] private GameObject radialIconPrefab;
    [SerializeField] private float distanceFromCenter;
    [SerializeField] private bool offsetByHalfIncrement;

    private float increment = 1;

    public void RegenerateIcons(List<IAbilityIcon> abilityIcons)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        increment = 360f / abilityIcons.Count;
        var offset = GetInitialRotation();
        for (int i = 0; i < abilityIcons.Count; i++)
        {
            var icon = Instantiate(radialIconPrefab, transform);
            icon.GetComponent<RadialIcon>().SetAbility(abilityIcons[i]);
            float x = Mathf.Cos((increment * i + offset) * Mathf.Deg2Rad) * distanceFromCenter;
            float y = Mathf.Sin((increment * i + offset) * Mathf.Deg2Rad) * distanceFromCenter;
            icon.transform.localPosition = new Vector2(x, y);
        }
    }

    public float GetIncrement()
    {
        return increment;
    }

    public float GetInitialRotation()
    {
        return offsetByHalfIncrement ? increment / 2f : 0;
    }

    public GameObject GetIconClosestToAngle(float angle)
    {
        float incrementIndex = Mathf.RoundToInt(angle / increment);
        return transform.GetChild((int)incrementIndex % transform.childCount).gameObject;
    }

    public GameObject GetIconAtIndex(int index)
    {
        return transform.GetChild(index).gameObject;
    }
}
