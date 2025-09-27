using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.StasisPulse;

internal class ProtoStasisFreeze : MonoBehaviour
{
    private const float MAX_MASS_VALUE = 200f;

    private Rigidbody rigidbody;
    private GameObject unfreezeFX;

    private float currentFreezeTime;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, true);
        rigidbody.SendMessage("OnFreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
        var mixin = GetComponent<LiveMixin>();
        if (mixin.maxHealth > 500)
        {
            UWE.CoroutineHost.StartCoroutine(TakeDamageOverTime(mixin, 3, 10));
        }
    }

    private void Update()
    {
        if (currentFreezeTime > 0)
        {
            currentFreezeTime -= Time.deltaTime;
            return;
        }

        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, false);
        rigidbody.SendMessage("OnUnfreezeByStasisSphere", SendMessageOptions.DontRequireReceiver);
        Utils.PlayOneShotPS(unfreezeFX, transform.position, Quaternion.identity);

        Destroy(this);
    }

    private IEnumerator TakeDamageOverTime(LiveMixin mixin, float duration, float damage)
    {
        float startTime = Time.time;
        float dmgPerUpdate = damage / duration;
        while (Time.time < startTime + duration)
        {
            mixin.TakeDamage(dmgPerUpdate * Time.deltaTime);
            yield return null;
        }
    }

    public void SetFreezeTimes(float minFreezeTime, float maxFreezeTime)
    {
        float normalizedMass = Mathf.InverseLerp(0, MAX_MASS_VALUE, rigidbody.mass);
        currentFreezeTime = Mathf.Lerp(maxFreezeTime, minFreezeTime, normalizedMass);
    }

    public void SetUnfreezeVF(GameObject unfreezeFX)
    {
        this.unfreezeFX = unfreezeFX;
    }
}
