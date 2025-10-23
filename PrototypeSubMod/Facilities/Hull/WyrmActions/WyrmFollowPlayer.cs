using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmFollowPlayer : CreatureAction
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationAmplitude;
    [SerializeField] private float offsetFromPlayer;

    private Vector3 targetPoint;
    private bool active;

    private void Start()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - rotationAmplitude,
            transform.localEulerAngles.y, transform.localEulerAngles.z);
        RecalculateTargetPoint();
    }

    public override void StartPerform(Creature creature, float time)
    {
        base.StartPerform(creature, time);
        active = true;
    }
    
    public override void StopPerform(Creature creature, float time)
    {
        base.StopPerform(creature, time);
        active = false;
    }

    private void Update()
    {
        if (!active) return;
        
        transform.position += transform.forward * (speed * Time.deltaTime);
        var angle = Mathf.Sin(Time.time * rotationSpeed * Mathf.Deg2Rad) * rotationAmplitude;
        transform.Rotate(transform.right, angle * Time.deltaTime, Space.Self);
    }

    private void RecalculateTargetPoint()
    {
        var dir = Player.main.transform.position - transform.position;
        targetPoint = Player.main.transform.position + dir.normalized * offsetFromPlayer;
    }
}