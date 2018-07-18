using Pathfinding;
using UnityEngine;

public class CreateGraph : MonoBehaviour {

    public int d;
    public int w;
    public float node;
    public float diameter;
    public bool cutCorners;

    private void Start()
    {
        //AiGridPath(d, w, node, diameter, cutCorners);
    }

    public void AiGridPath(int depth, int width, float nodeSize, float diameter, bool cutCorners)
    {
        // This holds all graph data
        AstarData data = AstarPath.active.data;
        data.cacheStartup = false;

        // This creates a Grid Graph
        GridGraph gridGraph = data.AddGraph(typeof(GridGraph)) as GridGraph;

        // Setting up the default parameters.
        //gridGraph.width = width;
        //gridGraph.depth = depth;
        //gridGraph.nodeSize = nodeSize;

        // Because it's 2d we are rotating to face the camera, which looks down the z - axis
        gridGraph.rotation.x = -90.0f;

        // Calculating the centre based on node size and number of nodes
        gridGraph.center.x = (width * nodeSize);
        gridGraph.center.y = (depth * nodeSize);
        gridGraph.center = new Vector3(gridGraph.center.x, gridGraph.center.y, 0);

        // Enabled corner cutting, disable slop detection and change slop axis to Z
        gridGraph.cutCorners = cutCorners;
        //gridGraph.maxClimb = 0;
        //gridGraph.

        // Setting to use 2d grid collision detection
        gridGraph.collision.use2D = true;
        gridGraph.collision.type = ColliderType.Sphere;
        gridGraph.collision.diameter = diameter;
        gridGraph.collision.mask = LayerMask.GetMask("PlayerObstacles");

        // Updates internal size from the above values
        gridGraph.SetDimensions(width * 2, depth * 2, nodeSize);

        
        // Scans all graphs, do not call gg.Scan(), that is an internal method
        AstarPath.active.Scan();

        // this is used to start the coroutine that only renders stuff close to the player
        // has to be called after the final scan so the grid knows where to draw the entire dungeon
        
        Debug.Log("Scan is now complete");
    }
}