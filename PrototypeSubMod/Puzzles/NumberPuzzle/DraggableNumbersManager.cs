using UnityEngine;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class DraggableNumbersManager : MonoBehaviour
{
    [SerializeField] private NumberPuzzleManager puzzleManager;
    [SerializeField] private BehaviourLOD behaviourLOD;
    [SerializeField] private float minXValue;
    [SerializeField] private float maxXValue;
    [SerializeField] private float[] numberXValues;

    public float ConstrainToBounds(float xValue)
    {
        return Mathf.Clamp(xValue, minXValue, maxXValue);
    }

    public NumberData GetClosestNumberValue(float xValue)
    {
        int index = 0;
        float lowestDiff = float.MaxValue;
        for (int i = 0; i < numberXValues.Length; i++)
        {
            float difference = Mathf.Abs(xValue - numberXValues[i]);
            if (difference < lowestDiff)
            {
                lowestDiff = difference;
                index = i;
            }
        }

        return new NumberData(numberXValues[index], index + 1);
    }

    public void OnDragComplete(int number, bool isSecondary)
    {
        puzzleManager.SelectNumber(number, isSecondary);
    }

    public bool LODIsFull() => behaviourLOD.IsFull();

    public struct NumberData
    {
        public float snapXValue;
        public int representativeNumber;

        public NumberData(float snapXValue, int representativeNumber)
        {
            this.snapXValue = snapXValue;
            this.representativeNumber = representativeNumber;
        }
    }
}