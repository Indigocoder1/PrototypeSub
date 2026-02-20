using Nautilus.Utility;
using PrototypeSubMod.Prefabs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.MiscMonobehaviors;

public class RandomSirenManager : MonoBehaviour
{
    private Vector3 gunPos = new Vector3(430, 30, 1185);

    private float maxWaitTime = 18000f;
    private float minWaitTime = 10800f;
    private float minDistanceToPlay = 1000f;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(RandomSiren());
    }

    private IEnumerator RandomSiren()
    {
        float randomWaitTime = UnityEngine.Random.Range(maxWaitTime, minWaitTime);
        yield return new WaitForSeconds(randomWaitTime);
        var distance = Vector3.Distance(gunPos, Player.main.transform.position);
        if (distance < minDistanceToPlay) UWE.CoroutineHost.StartCoroutine(RandomSiren());
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("GunSiren"), gunPos);
        UWE.CoroutineHost.StartCoroutine(RandomSiren());
    }
}