using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class TransmissionDeviceUINumber : MonoBehaviour
{
    public event Action onNumberChanged;
    
    [SerializeField] private Image numberImage;
    [SerializeField] private Sprite[] numberSprites;
    [SerializeField] private int correctNumber;

    private Dictionary<Sprite, int> mixedNumberSprites;
    
    private int index;
    
    private void Start()
    {
        var indices = new int[numberSprites.Length];
        var availableNumbers = new List<int>();
        for (int i = 0; i < numberSprites.Length; i++)
        {
            availableNumbers.Add(i);
        }
        
        for (int i = 0; i < numberSprites.Length; i++)
        {
            var j = Random.Range(0, availableNumbers.Count - 1);
            if (i == 0 && j + 1 == correctNumber)
            {
                j = GetJ(j);
            }
            
            indices[i] = availableNumbers[j];
            availableNumbers.RemoveAt(j);
        }

        mixedNumberSprites = new();
        foreach (var index in indices)
        {
            mixedNumberSprites.Add(numberSprites[index], index);
        }
        
        UpdateImage();
    }

    private int GetJ(int j)
    {
        j = Random.Range(0, numberSprites.Length - 1);
        if (j + 1 == correctNumber)
        {
            j = GetJ(j);
        }

        return j;
    }

    public void IncreaseNumber()
    {
        index++;
        index %= numberSprites.Length;
        UpdateImage();
    }
    
    public void DecreaseNumber()
    {
        index--;
        if (index < 0)
        {
            index = numberSprites.Length - 1;
        }

        UpdateImage();
    }

    public bool OnCorrectNumber()
    {
        if (mixedNumberSprites == null) return false;
        
        var currentNumberIndex = mixedNumberSprites.ElementAt(index).Value;
        return currentNumberIndex + 1 == correctNumber;
    }

    public int GetCorrectNumber() => correctNumber;

    public int GetCurrentNumber()
    {
        if (mixedNumberSprites == null) return -1;
        
        var currentNumberIndex = mixedNumberSprites.ElementAt(index).Value;
        return currentNumberIndex + 1;
    }
    
    private void UpdateImage()
    {
        numberImage.sprite = mixedNumberSprites.ElementAt(index).Key;
        onNumberChanged?.Invoke();
    }
}