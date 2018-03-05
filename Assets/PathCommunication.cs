using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

// A Path instance contains two lists. Path.vectorPath is a Vector3 list which holds the path, this list will be modified 
// if any smoothing is used, it is the recommended way to get a path.secondly there is the Path.path list which is a list of GraphNode 
// elements, it holds all the nodes the path visisted which can be useful to get additonal info on the traversed path.

public class PathCommunication : MonoBehaviour {

    public Transform targetPos;          // a given target  Position to move to
    Seeker seeker;                       // seeker attached to this script
    EnemyAIMovement movementScript;      // script that actually movees the object

    public Path path;

    public float speed = 2;

    public float nextWaypointDistance = 1;

    private int currentWaypoint;

    public float repathRate = 0.5f;
    private float lastRepath = float.NegativeInfinity;

    void Start () {

        // reference to the seeker component
        seeker = GetComponent<Seeker>();

        // reference to this objects movement script
        movementScript = GetComponent<EnemyAIMovement>();

        //OnPathComplete will be called every time a path is returned to this seeker
        seeker.pathCallback += OnPathComplete;

    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("Path calculated. Did it have an error? " + p.error);
        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
    }

    void Update()
    {
        FollowPath();
    }

    // Logic for following path and sending information to the controller script
    private void FollowPath()
    {
        if (Time.time > lastRepath + repathRate && seeker.IsDone())
        {
            lastRepath = Time.time;
        }

        // Start a new path to the targetPosition, call the the OnPathComplete function
        // when the path has been calculated (which may take a few frames depending on the complexity)
        if (targetPos != null)
        {
            seeker.StartPath(transform.position, targetPos.position);
        }

        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }

        if (currentWaypoint > path.vectorPath.Count) return;
        if (currentWaypoint == path.vectorPath.Count)
        {
            Debug.Log("End Of Path Reached");
            currentWaypoint++;
            return;
        }

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;

        //Debug.Log(dir);

        // Multiply the direction by our desired speed to get a velocity
        Vector3 velocity = dir * speed;

        movementScript.MoveObj(dir, path.vectorPath[currentWaypoint]);

        if ((transform.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance)
        {
            currentWaypoint++;
            return;
        }
    }

    public void OnDisable()
    {
        // When disabling or destroying the script, callback references are not removed, so it is good practise to 
        // remove the callback during OnDisable in case that should happen
        seeker.pathCallback -= OnPathComplete;
    }
}
