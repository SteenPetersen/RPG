using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpAnimationControl : MonoBehaviour {

    EnemyAI ai;

    private void Awake()
    {
        ai = transform.root.gameObject.GetComponent<EnemyAI>();
    }

    public void StrikeComplete()
    {
        ai.OnStrikeComplete();
    }
}
