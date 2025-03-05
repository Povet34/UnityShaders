using System.Collections.Generic;
using UnityEngine;

public class FallingSnow : MonoBehaviour
{
    private ParticleSystem ps;
    private List<ParticleCollisionEvent> colEventList;

    private GameObject cachedTargetGO;
    private GroundSnowPainter snowPainter;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        colEventList = new List<ParticleCollisionEvent>(100);
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other != cachedTargetGO)
        {
            cachedTargetGO = other;
            snowPainter = other.GetComponent<GroundSnowPainter>();
        }

        if (snowPainter == null || snowPainter.isActiveAndEnabled == false)
            return;

        int numColEvents = ps.GetCollisionEvents(other, colEventList);

        for (int i = 0; i < numColEvents; i++)
        {
            snowPainter.PileSnow(colEventList[i].intersection);
        }
    }
}
