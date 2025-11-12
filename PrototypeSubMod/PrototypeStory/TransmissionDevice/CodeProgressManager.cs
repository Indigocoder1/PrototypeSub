using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class CodeProgressManager : MonoBehaviour
{
    [SerializeField] private uGUI_TransmissionTab transmissionTab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private CodePathPoint[] pathPoints;
    [SerializeField] private float animationTime;

    private void Start()
    {
        var numbers = transmissionTab.GetNumbers();
        foreach (var number in numbers)
        {
            number.onNumberChanged += OnNumberChanged;
        }

        transmissionTab.onTabOpened += () =>
        {
            lineRenderer.enabled = true;
        };
        transmissionTab.onTabClosed += () =>
        {
            lineRenderer.enabled = false;
        };
        
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
    }

    private void OnNumberChanged()
    {
        var numbers = transmissionTab.GetNumbers();
        numbers = numbers.OrderBy(n => n.GetCurrentNumber()).ToArray();
        int correctInARow = 0;
        int prevNumber = numbers[0].GetCurrentNumber();
        List<TransmissionDeviceUINumber> correctNumbers = new();
        correctNumbers.Add(numbers[0]);
        for (int i = 1; i < numbers.Length; i++)
        {
            if (numbers[i].GetCurrentNumber() != prevNumber + 1) break;
            
            correctInARow++;
            correctNumbers.Add(numbers[i]);
            prevNumber = numbers[i].GetCurrentNumber();
        }

        lineRenderer.positionCount = correctInARow + 1;
        var positions = new Vector3[correctInARow + 1];

        int index = 0;
        foreach (var number in correctNumbers)
        {
            foreach (var point in pathPoints)
            {
                if (point.GetNumberButton() == number)
                {
                    positions[index] = point.transform.localPosition;
                }
            }

            index++;
        }

        lineRenderer.SetPositions(positions);
    }

    public void PlayTransmissionAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(PlayAnimation(animationTime));
    }

    private IEnumerator PlayAnimation(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            lineRenderer.material.SetFloat("_FillAmount", time / duration);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        lineRenderer.material.SetFloat("_FillAmount", 1);
    }
}