using Nautilus.Utility;
using System.Collections;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.MiscMonobehaviors;

public class RandomSirenManager : MonoBehaviour
{
    private static readonly Vector3 GunPos = new (430, 30, 1185);

    private const float MaxWaitTime = 18000f;
    private const float MinWaitTime = 10800f;
    private const float MinDistanceToPlay = 1000f;

    private void Start()
    {
        CoroutineHost.StartCoroutine(RandomSiren());
    }

    private IEnumerator RandomSiren()
    {
        var randomWaitTime = Random.Range(MaxWaitTime, MinWaitTime);
        yield return new WaitForSeconds(randomWaitTime);

        if (Player.main == null) yield break;
        
        var distance = Vector3.Distance(GunPos, Player.main.transform.position);

        if (distance >= MinDistanceToPlay)
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("GunSiren"), GunPos);
        }
        
        CoroutineHost.StartCoroutine(RandomSiren());
    }
}