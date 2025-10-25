using System;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class DraggableNumberSelector : MonoBehaviour
{
    [SerializeField] private DraggableNumbersManager draggableNumberSelector;
    [SerializeField] private float sensitivity = 0.75f;
    [SerializeField] private Collider collider;
    [SerializeField] private bool isSecondary;

    private Vector3 localTargetPoint;
    private bool dragging;

    private void OnValidate()
    {
        if (!collider) TryGetComponent(out collider);
    }

    private void Start()
    {
        var numberData = draggableNumberSelector.GetClosestNumberValue(transform.localPosition.x);
        transform.localPosition =
            new Vector3(numberData.snapXValue, transform.localPosition.y, transform.localPosition.z);
        draggableNumberSelector.OnDragComplete(numberData.representativeNumber, isSecondary);
    }

    private void Update()
    {
        if (!draggableNumberSelector.LODIsFull()) return;
        
        HandleDragStartStop();
    }

    private void LateUpdate()
    {
        if (!dragging) return;
        
        MainCameraControl_Patches.SetOverwriteDelta(
            ProtoCameraUtils.CalculateTargetAngleDelta(transform.TransformPoint(localTargetPoint), 10), true);

        transform.localPosition += new Vector3(MainCameraControl_Patches.GetOverwrittenLookDelta().x * sensitivity * Time.deltaTime, 0, 0);
        float newX = draggableNumberSelector.ConstrainToBounds(transform.localPosition.x);
        transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
    }

    private void HandleDragStartStop()
    {
        if (GameInput.GetButtonDown(GameInput.Button.LeftHand) && LookingAtSelector(out var hitInfo))
        {
            dragging = true;
            localTargetPoint = transform.InverseTransformPoint(hitInfo.point);
            MainCameraControl_Patches.SetOverwriteDelta(Vector2.zero, true);
        }

        if (GameInput.GetButtonUp(GameInput.Button.LeftHand) && LookingAtSelector(out _))
        {
            dragging = false;
            MainCameraControl_Patches.SetOverwriteDelta(Vector2.zero, false);
            var numberData = draggableNumberSelector.GetClosestNumberValue(transform.localPosition.x);
            transform.localPosition =
                new Vector3(numberData.snapXValue, transform.localPosition.y, transform.localPosition.z);
            draggableNumberSelector.OnDragComplete(numberData.representativeNumber, isSecondary);
        }
    }

    private bool LookingAtSelector(out RaycastHit hitInfo)
    {
        bool hitObj = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 50,
            ~(1 << LayerID.Player));

        if (!hitObj) return false;

        if (hitInfo.collider != collider) return false;

        return true;
    }
}