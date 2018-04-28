using System;
using System.Collections;
using UnityEngine;

public class BoardCreator : MonoBehaviour
{
    // The type of tile that will be laid in a specific position.
    public enum TileType
    {
        BlackArea, Floor, ColliderWall, Goal
    }

    public static BoardCreator instance;

    public int chanceOfChestPerRoom;
    public int chanceOfBetterChestPerRoom;


    public float chanceOfVariantFloorTiles;
    public int columns = 100;                                 // The number of columns on the board (how wide it will be).
    public int rows = 100;                                    // The number of rows on the board (how tall it will be).
    public IntRange numRooms = new IntRange(15, 20);         // The range of the number of rooms there can be.
    public IntRange roomWidth = new IntRange(3, 10);         // The range of widths rooms can have.
    public IntRange roomHeight = new IntRange(3, 10);        // The range of heights rooms can have.
    public IntRange corridorLength = new IntRange(6, 10);    // The range of lengths corridors between rooms can have.

    public IntRange numEnemy = new IntRange(0, 4);           // The range of enemies rooms can have.

    public GameObject normalFloorTile;                        // the normal floor tile that should be present in a majority of situations
    public GameObject[] variantFloorTiles;                    // An array of floor tile prefabs that vary in graphic from the normal one
    public GameObject[] BlackArea;                            // An array of sprites that mask the non relevant areas.
    public GameObject[] colliderWalls;                        // An Array of walls that have colliders
    public GameObject[] outerWallTiles;                       // An array of outer wall tile prefabs.
    public GameObject goalFloor;
    public GameObject player;
    public GameObject[] enemy;
    public GameObject boss;
    public GameObject goal;
    public GameObject map;
    public GameObject entranceTile;
    public GameObject entranceTorch;
    public GameObject chestLowerTier;
    public GameObject chestHigherTier;
    public GameObject floorGrid;

    public Camera cam;

    private TileType[][] tiles;                               // A jagged array of tile types representing the board, like a grid.
    private Room[] rooms;                                     // All the rooms that are created for this board.
    private Corridor[] corridors;                             // All the corridors that connect the rooms.
    private GameObject boardHolder;                           // GameObject that acts as a container for all other tiles.
    private GameObject enemyHolder;
    private GameObject outterWallHolder;

    private Vector2 goalPos;

    public CreateGraph aStarGridCreator;

    [HideInInspector]
    public int currentBoardsNumberOfRooms;

    private void Awake()
    {
        player = PlayerController.instance.gameObject;
        cam = CameraController.instance.GetComponentInChildren<Camera>();
        instance = this;

        StartCoroutine(InitializeMap());
        aStarGridCreator.d = rows;
        aStarGridCreator.w = columns;
    }

    void SetupTilesArray()
    {
        // Set the tiles jagged array to the correct width.
        tiles = new TileType[columns][];

        // Go through all the tile arrays...
        for (int i = 0; i < tiles.Length; i++)
        {
            // ... and set each tile array is the correct height.
            tiles[i] = new TileType[rows];
        }
    }

    void CreateRoomsAndCorridors()
    {
        // Create the rooms array with a random size.
        currentBoardsNumberOfRooms = numRooms.Random;
        rooms = new Room[currentBoardsNumberOfRooms];

        // There should be one less corridor than there is rooms.
        corridors = new Corridor[rooms.Length - 1];

        // Create the first room and corridor.
        rooms[0] = new Room();
        corridors[0] = new Corridor();
        // Setup the first room, there is no previous corridor so we do not use one.
        rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

        // Setup the first corridor using the first room.
        corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);

        for (int i = 1; i < rooms.Length; i++)
        {
            // Create a room.
            rooms[i] = new Room();

            // Setup the room based on the previous corridor.
            rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy);

            if (i == rooms.Length / 2)
            {
                rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy, true);
            }

            if (i == corridors.Length)
            {
                // Setup the last room
                rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy, 1);
            }

            // If we haven't reached the end of the corridors array...
            if (i < corridors.Length)
            {
                // ... create a corridor.
                corridors[i] = new Corridor();

                // Setup the corridor based on the room that was just created.
                corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
            }

        }

    }

    public void SpawnElement(Vector2 pos, GameObject obj)
    {
        GameObject tmp = Instantiate(obj, pos, Quaternion.identity);

        if (tmp.tag == "Enemy")
        {
            tmp.transform.parent = enemyHolder.transform;
        }

        else if (tmp.tag == "Goal")
        {
            goalPos = pos;
        }
    }

    void SetTilesValuesForRooms()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Length; i++)
        {
            Room currentRoom = rooms[i];

            // ... and for each room go through it's width.
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //// For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;
                    Debug.Log("  xCoord is  " + xCoord + "  yCoord is  " + yCoord + "  room number is:   " + i + "  k is  " + k);


                    // Make the left wall have colliders
                    if (xCoord == currentRoom.xPos)
                    {
                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                        }

                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                        {
                            if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                            {
                                tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                            }

                            if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                            {
                                tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                            }
                        }
                        

                    }

                    // Make the Right wall have colliders
                    if (xCoord == currentRoom.xPos + currentRoom.roomWidth - 1)
                    {
                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                        }

                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                        {
                            if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                            {
                                tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                            }

                            if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                            {
                                tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                            }
                        }

                    }

                    // Make the Top wall have colliders
                    if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                    {
                        if (tiles[xCoord][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                        }
                    }

                    // Make the Bottom wall have colliders
                    if (yCoord == currentRoom.yPos)
                    {
                        if (tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                        }
                    }

                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                     tiles[xCoord][yCoord] = TileType.Floor;
                }
            }

            if (i == rooms.Length - 1)
            {
                int x = (int)goalPos.x;
                int y = (int)goalPos.y;
                tiles[x][y] = TileType.Goal;
            }

        }
    }

    void SetTilesValuesForCorridors()
    {
        // Go through every corridor...
        for (int i = 0; i < corridors.Length; i++)
        {
            Corridor currentCorridor = corridors[i];

            // and go through it's length.
            for (int j = 0; j < currentCorridor.corridorLength; j++)
            {
                // Start the coordinates at the start of the corridor.
                int xCoord = currentCorridor.startXPos;
                int yCoord = currentCorridor.startYPos;

                // Depending on the direction, add or subtract from the appropriate
                // coordinate based on how far through the length the loop is.
                switch (currentCorridor.direction)
                {
                    case Direction.North:
                        yCoord += j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord + 1][yCoord] = TileType.Floor;
                        //         Checking side tiles
                        //         X [current tile] X X

                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord] = TileType.Floor;
                        }
                        if (tiles[xCoord + 2][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord] = TileType.ColliderWall;
                        }

                        //       Checking LOWER side tiles
                        //            [current tile] 
                        //         X                 X X

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord + 2][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord - 1] = TileType.ColliderWall;
                        }


                        //       Checking UPPER side tiles
                        //          X                X
                        //            [current tile]           

                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.Floor;
                        }
                        if (tiles[xCoord + 2][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord + 1] = TileType.ColliderWall;
                        }


                        break;

                    case Direction.East:
                        xCoord += j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord][yCoord - 1] = TileType.Floor;

                        //         Checking side tiles
                        //                 X
                        //          [current tile]
                        //                 X
                        //                 X


                        if (tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord - 2] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord + 1] = TileType.ColliderWall;
                        }

                        //        Checking Behind tiles
                        //           X
                        //             [current tile]       
                        //           X
                        //           X


                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord - 1][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 2] = TileType.ColliderWall;
                        }


                        //          Checking Infront tiles
                        //                            X
                        //             [current tile]       
                        //                            X       

                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord + 1][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 2] = TileType.ColliderWall;
                        }
                        break;

                    case Direction.South:
                        yCoord -= j;


                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord + 1][yCoord] = TileType.Floor;

                        //         Checking side tiles
                        //         X [current tile] X X

                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord] = TileType.Floor;
                        }
                        if (tiles[xCoord + 2][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord] = TileType.ColliderWall;
                        }

                        //       Checking LOWER side tiles
                        //            [current tile] 
                        //         X                 X X

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord + 2][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord - 1] = TileType.ColliderWall;
                        }


                        //       Checking UPPER side tiles
                        //          X                X
                        //            [current tile]           

                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.Floor;
                        }
                        if (tiles[xCoord + 2][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord + 1] = TileType.ColliderWall;
                        }

                        break;

                    case Direction.West:
                        xCoord -= j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord][yCoord - 1] = TileType.Floor;


                        //         Checking side tiles
                        //                 X
                        //          [current tile]
                        //                 X
                        //                 X


                        if (tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord - 2] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord][yCoord + 1] = TileType.ColliderWall;
                        }

                        //        Checking Behind tiles
                        //           X
                        //             [current tile]       
                        //           X
                        //           X


                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord - 1][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 2] = TileType.ColliderWall;
                        }


                        //          Checking Infront tiles
                        //                            X
                        //             [current tile]       
                        //                            X       

                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.Floor;
                        }
                        if (tiles[xCoord + 1][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 2] = TileType.ColliderWall;
                        }
                        break;
                }

                if (j == currentCorridor.corridorLength - 1)
                {
                    if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                    {
                        tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                    }
                    if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                    {
                        tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                    }

                    if (tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                    {
                        tiles[xCoord][yCoord - 1] = TileType.ColliderWall;
                    }
                    if (tiles[xCoord][yCoord + 1] == TileType.BlackArea)
                    {
                        tiles[xCoord][yCoord + 1] = TileType.ColliderWall;
                    }
                }
            }
        }
    }

    void InstantiateTiles()
    {
        // Go through all the tiles in the jagged array...
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {

                // If the tile type is Floor...
                if (tiles[i][j] == TileType.Floor)
                {
                    // ... instantiate a floor tile for it.

                    float rand = UnityEngine.Random.Range(0, 100);

                    if (rand <= chanceOfVariantFloorTiles)
                    {
                        InstantiateFromArrayFloor(variantFloorTiles, i, j);
                    }
                    else
                    {
                        // The position to be instantiated at is based on the coordinates.
                        Vector3 position = new Vector3(i, j, 0f);

                        // Create an instance of the prefab from the random index of the array.
                        GameObject tileInstance = Instantiate(normalFloorTile, position, Quaternion.identity) as GameObject;

                        // Set the tile's parent to the board holder.
                        tileInstance.transform.parent = boardHolder.transform;
                    }
                }

                // If the tile type is Black area...
                else if (tiles[i][j] == TileType.BlackArea)
                {
                    // ... instantiate a wall over the top.
                    InstantiateFromArrayDarkness(BlackArea, i, j);
                }

                // If the tile type is Wall...
                else if (tiles[i][j] == TileType.ColliderWall)
                {
                    // ... instantiate a Collider wall over the top.
                    InstantiateFromArrayWall(colliderWalls, i, j);
                }

                // If the tile type is Goal...
                else if (tiles[i][j] == TileType.Goal)
                {
                    Debug.Log("calling goal tile");
                    // ... instantiate a goal see through floor over the top.
                    //Vector3 position = new Vector3(float)i, j, 0f);
                    Vector3 position = new Vector3(goalPos.x, goalPos.y, 0f);
                    Instantiate(goalFloor, position, Quaternion.identity);
                }
            }
        }
    }

    void InstantiateOuterWalls()
    {
        // The outer walls are one unit left, right, up and down from the board.
        float leftEdgeX = -1f;
        float rightEdgeX = columns + 0f;
        float bottomEdgeY = -1f;
        float topEdgeY = rows + 0f;

        // Instantiate both vertical walls (one on each side).
        InstantiateVerticalOuterWall(leftEdgeX, bottomEdgeY, topEdgeY);
        InstantiateVerticalOuterWall(rightEdgeX, bottomEdgeY, topEdgeY);

        // Instantiate both horizontal walls, these are one in left and right from the outer walls.
        InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
        InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
    }

    void InstantiateVerticalOuterWall(float xCoord, float startingY, float endingY)
    {
        // Start the loop at the starting value for Y.
        float currentY = startingY;

        // While the value for Y is less than the end value...
        while (currentY <= endingY)
        {
            // ... instantiate an outer wall tile at the x coordinate and the current y coordinate.
            InstantiateFromArrayWall(outerWallTiles, xCoord, currentY);

            currentY++;
        }
    }

    void InstantiateHorizontalOuterWall(float startingX, float endingX, float yCoord)
    {
        // Start the loop at the starting value for X.
        float currentX = startingX;

        // While the value for X is less than the end value...
        while (currentX <= endingX)
        {
            // ... instantiate an outer wall tile at the y coordinate and the current x coordinate.
            InstantiateFromArrayWall(outerWallTiles, currentX, yCoord);

            currentX++;
        }
    }

    void InstantiateFromArrayFloor(GameObject[] prefabs, float xCoord, float yCoord)
    {
        // Create a random index for the array.
        int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);

        // The position to be instantiated at is based on the coordinates.
        Vector3 position = new Vector3(xCoord, yCoord, 0f);

        // Create an instance of the prefab from the random index of the array.
        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        // Set the tile's parent to the board holder.
        tileInstance.transform.parent = boardHolder.transform;
    }

    void InstantiateFromArrayWall(GameObject[] prefabs, float xCoord, float yCoord)
    {
        // Create a random index for the array.
        int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);

        // The position to be instantiated at is based on the coordinates.
        Vector3 position = new Vector3(xCoord, yCoord, -0.5f);

        // Create an instance of the prefab from the random index of the array.
        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        //tileInstance.GetComponent<Renderer>().material.color = new Color(0.5f, 1, 1); //C#

        // Set the tile's parent to the board holder.
        tileInstance.transform.parent = outterWallHolder.transform;
    }

    void InstantiateFromArrayDarkness(GameObject[] prefabs, float xCoord, float yCoord)
    {
        // Create a random index for the array.
        int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);

        // The position to be instantiated at is based on the coordinates.
        Vector3 position = new Vector3(xCoord, yCoord, -1f);

        // Create an instance of the prefab from the random index of the array.
        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        //tileInstance.GetComponent<Renderer>().material.color = new Color(0.5f, 1, 1); //C#

        // Set the tile's parent to the board holder.
        tileInstance.transform.parent = boardHolder.transform;
    }

    void BuildEntranceRoom()
    {
        Vector3 position = new Vector3(player.transform.position.x, player.transform.position.y, 0f);
        Instantiate(entranceTile, position, Quaternion.identity);
    }

    public IEnumerator InitializeMap()
    {
        bool workDone = false;

        while (!workDone)
        {
            // Let the engine run for a frame
            yield return null;

            // Find the board and enemy holder.
            boardHolder = GameObject.Find("BoardHolder");
            enemyHolder = new GameObject("EnemyHolder");
            outterWallHolder = GameObject.Find("OutterWallHolder");

            cam.fieldOfView = 43;
            cam.backgroundColor = Color.black;

            SetupTilesArray();

            CreateRoomsAndCorridors();

            SetTilesValuesForRooms();
            SetTilesValuesForCorridors();

            InstantiateTiles();
            InstantiateOuterWalls();

            BuildEntranceRoom();

            workDone = true;
        }
    }

    void FunctionHolderForTestingPurposes()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Length; i++)
        {
            Room currentRoom = rooms[i];

            // ... and for each room go through it's width.
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //// For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;
                    Debug.Log("  xCoord is  " + xCoord + "  yCoord is  " + yCoord + "  i is   " + i + "  k is  " + k);
                    // Make the left wall have colliders
                    if (xCoord == currentRoom.xPos && tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                    {
                        tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;

                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1 && tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;

                            if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                            {
                                tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                            }
                        }
                    }

                    // Make the Right wall have colliders
                    if (xCoord == currentRoom.xPos + currentRoom.roomWidth - 1 && tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                    {
                        tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;

                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1 && tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord] = TileType.ColliderWall;

                            if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                            {
                                tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                            }
                        }
                    }

                    // Make the Top wall have colliders
                    if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1 && tiles[xCoord][yCoord + 1] == TileType.BlackArea)
                    {
                        tiles[xCoord][yCoord + 1] = TileType.ColliderWall;

                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                        }
                    }

                    // Make the Bottom wall have colliders
                    if (yCoord == currentRoom.yPos && tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                    {
                        tiles[xCoord][yCoord - 1] = TileType.ColliderWall;

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                        }
                    }

                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                    tiles[xCoord][yCoord] = TileType.Floor;
                }
            }


            int x = (int)goalPos.x;
            int y = (int)goalPos.y;
            tiles[x][y] = TileType.Goal;
        }
        
    }
}