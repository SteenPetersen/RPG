using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class EnemyAIMovement : MonoBehaviour {

    Rigidbody2D rigid;

    public float speed;

    public bool moved;

	void Start () {

        //get references
        rigid = GetComponent<Rigidbody2D>();

	}
	
	void Update () {
		
	}

    public void MoveObj(Vector3 dir, Vector3 currentTargetWaypoint)
    {
        if (transform.position != currentTargetWaypoint)
        {
            rigid.AddForce(dir * speed);
        }
    }

}
