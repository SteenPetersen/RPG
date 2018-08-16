using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCreator : MonoBehaviour
{
    // The type of tile that will be laid in a specific position.
    public enum TileType
    {
        BlackArea, Floor, ColliderWall, Goal, SkullDoor
    }

    public static BoardCreator instance;

    public int chanceOfChestPerRoom;
    public int chanceOfBetterChestPerRoom;
    public int percentageOfEnemiesInCorridors;
    public int obstructedCorridorMaxLength;
    public int playerLevelToSpawnBossRoom = 5;

    public float chanceOfVariantFloorTiles;
    public int columns = 100;                                 // The number of columns on the board (how wide it will be).
    public int rows = 100;                                    // The number of rows on the board (how tall it will be).
    public IntRange numRooms = new IntRange(15, 20);         // The range of the number of rooms there can be.
    public IntRange roomWidth = new IntRange(3, 10);         // The range of widths rooms can have.
    public IntRange roomHeight = new IntRange(3, 10);        // The range of heights rooms can have.
    public IntRange corridorLength = new IntRange(6, 10);    // The range of lengths corridors between rooms can have.

    public IntRange numEnemy = new IntRange(0, 4);           // The range of enemies rooms can have.
    public IntRange numDestructables = new IntRange(0, 4);   // The range of destructables rooms can have.

    public GameObject[] normalFloorTiles;                        // the normal floor tile that should be present in a majority of situations
    public GameObject[] variantFloorTiles;                    // An array of floor tile prefabs that vary in graphic from the normal one
    public GameObject[] BlackArea;                            // An array of sprites that mask the non relevant areas.
    public GameObject[] colliderWalls;                        // An Array of walls that have colliders
    public GameObject[] outerWallTiles;                       // An array of outer wall tile prefabs.
    public GameObject goalFloor;
    public GameObject skullDoor;
    public GameObject player;
    public GameObject[] enemy;
    public GameObject[] destructables;
    public GameObject destructableWall;
    public GameObject boss;
    public GameObject goal;
    public GameObject map;
    public GameObject entranceTile;
    public GameObject entranceTorch;
    public GameObject chestLowerTier;
    public GameObject chestHigherTier;
    public GameObject floorGrid;
    public GameObject followLight;

    public List<TileLocation> skullDoorLocations = new List<TileLocation>();


    public List<TileLocation> allRoomFloorTiles = new List<TileLocation>();
    public List<TileLocation> allCorridorFloorTiles = new List<TileLocation>();

    public Camera cam;

    bool spaceForBossRoom = true;

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
            rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy, numDestructables);

            // check if its the middle room
            if (i == rooms.Length / 2)
            {
                rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy, numDestructables, true);
            }

            // check if its the last room
            if (i == corridors.Length)
            {
                // Setup the last room
                //Debug.Log("Skipping creation of last room");
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

    public void SpawnElement(Vector3 pos, GameObject obj, bool randRot = false, bool isBoss = false, bool wall = false)
    {
        GameObject tmp = Instantiate(obj, pos, Quaternion.identity);

        if (randRot)
        {
            int r = UnityEngine.Random.Range(0, 360);
            tmp.transform.rotation = Quaternion.Euler(0, 0, r);
        }


        if (tmp.tag == "Enemy" && !isBoss)
        {
            tmp.transform.parent = enemyHolder.transform;
            DungeonManager.instance.enemiesInDungeon.Add(tmp);
        }

        else if (tmp.tag == "Goal")
        {
            goalPos = pos;
        }

        if (wall)
        {
            tmp.transform.position = new Vector3(tmp.transform.position.x, tmp.transform.position.y, -0.5f);
        }
    }

    public void CreateDungeonGraph()
    {
        //Debug.Log("calling for the creation of a grid");
        aStarGridCreator.AiGridPath(columns, rows, 0.5f, 1, false);
    }

    void SetTilesValuesForRooms()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Length; i++)
        {
            Room currentRoom = rooms[i];

            if (currentRoom == rooms[0])
            {
                // ... and for each room go through it's width.
                for (int j = 0; j < currentRoom.roomWidth; j++)
                {
                    int xCoord = currentRoom.xPos + j;

                    //// For each horizontal tile, go up vertically through the room's height.
                    for (int k = 0; k < currentRoom.roomHeight; k++)
                    {
                        int yCoord = currentRoom.yPos + k;
                        //Debug.Log("  xCoord is  " + xCoord + "  yCoord is  " + yCoord + "  room number is:   " + i + "  k is  " + k);


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
            }

            if (!currentRoom.round && currentRoom != rooms[0])
            {
                // ... and for each room go through it's width.
                for (int j = 0; j < currentRoom.roomWidth; j++)
                {
                    int xCoord = currentRoom.xPos + j;

                    //// For each horizontal tile, go up vertically through the room's height.
                    for (int k = 0; k < currentRoom.roomHeight; k++)
                    {
                        int yCoord = currentRoom.yPos + k;
                        //Debug.Log("  xCoord is  " + xCoord + "  yCoord is  " + yCoord + "  room number is:   " + i + "  k is  " + k);


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

                        StoreTileLocation(xCoord, yCoord, allRoomFloorTiles);

                        // if the tile is an edge tile store it as an edge tile
                        if (xCoord == currentRoom.xPos || 
                            xCoord == currentRoom.xPos + currentRoom.roomWidth - 1 || 
                            yCoord == currentRoom.yPos + currentRoom.roomHeight - 1 || 
                            yCoord == currentRoom.yPos)
                        {
                            StoreTileLocation(xCoord, yCoord, currentRoom.edgeFloorTilesOfThisRoom);
                        }
                        else
                        {
                            StoreTileLocation(xCoord, yCoord, currentRoom.middleFloorTilesInThisRoom);
                        }
                    }
                }
            }

            if (currentRoom.round)
            {
                CreateBossRoom(currentRoom);
            }

            if (i == rooms.Length - 2)
            {
                // check if the area is clear

                // grab the last corridor
                Corridor c = corridors[corridors.Length - 1];

                // Find out where you should start your search from depending on the directionality of the corridor...
                if (c.direction == Direction.North)
                {
                    int xPos = c.EndPositionX - 12;
                    int yPos = c.EndPositionY;

                    int xSmall = 0;
                    int xLarge = 25;
                    int ySmall = 5;
                    int yLarge = 25;

                    // if there is space on the board for a boss room
                    if (xPos > 0 && yPos > 0 && (xPos + 26) < columns && (yPos + 26) < rows)
                    {
                        //Debug.LogWarning("I have passed sanity check going for the check method call: first test ( > 200 ): " + (c.EndPositionY - 26) + " Second test: ( > 0 ) " + xPos + " third test: ( < " + columns + " ) " + (xPos + 26));

                        CheckIfAreaIsClear(xPos, yPos, xSmall, xLarge, ySmall, yLarge);
                    }
                    else
                    {
                        spaceForBossRoom = false;
                    }
                }

                else if (c.direction == Direction.East)
                {
                    int xPos = c.EndPositionX;
                    int yPos = c.EndPositionY - 12;

                    int xSmall = 5;
                    int xLarge = 25;
                    int ySmall = 0;
                    int yLarge = 25;

                    if (xPos > 0 && yPos > 0 && (xPos + 26) < columns && (yPos + 26) < rows)
                    {

                        //Debug.LogWarning("I have passed sanity check going for the check method call: first test ( > 200 ): " + (c.EndPositionX - 26) + " Second test: ( > 0 ) " + yPos + " third test: ( < " + rows + " ) " + yPos);

                        CheckIfAreaIsClear(xPos, yPos, xSmall, xLarge, ySmall, yLarge);
                    }
                    else
                    {
                        spaceForBossRoom = false;
                    }
                }

                else if (c.direction == Direction.South)
                {
                    int xPos = c.EndPositionX - 12;
                    int yPos = c.EndPositionY - 25;

                    int xSmall = 0;
                    int xLarge = 25;
                    int ySmall = 0;
                    int yLarge = 20;

                    if (xPos > 0 && yPos > 0 && (xPos + 26) < columns && (yPos + 26) < rows)
                    {

                        //Debug.LogWarning("I have passed sanity check going for the check method call: first test ( > 0 ): " + (c.EndPositionY - 26) + " Second test: ( > 0 ) " + xPos + " third test: ( < " + columns + " ) " + xPos);

                        CheckIfAreaIsClear(xPos, yPos, xSmall, xLarge, ySmall, yLarge);

                    }
                    else
                    {
                        spaceForBossRoom = false;
                    }
                }

                else if (c.direction == Direction.West)
                {
                    int xPos = c.EndPositionX - 25;
                    int yPos = c.EndPositionY - 12;

                    int xSmall = 0;
                    int xLarge = 20;
                    int ySmall = 0;
                    int yLarge = 25;

                    if (xPos > 0 && yPos > 0 && (xPos + 26) < columns && (yPos + 26) < rows)
                    {
                        
                       // Debug.LogWarning("I have passed sanity check going for the check method call: first test ( > 0 ): " + (c.EndPositionX - 26) + " Second test: ( > 0 ) " + yPos + " third test: ( < " + rows + " ) " + xPos);

                        CheckIfAreaIsClear(xPos, yPos, xSmall, xLarge, ySmall, yLarge);

                    }
                    else
                    {
                        spaceForBossRoom = false;
                    }
                }

                if (spaceForBossRoom)
                {
                    
                }

                rooms[rooms.Length - 1].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[corridors.Length - 1], numEnemy, 1, spaceForBossRoom);

            }

            if (i == rooms.Length - 1)
            {
                int x = (int)goalPos.x;
                int y = (int)goalPos.y;
                tiles[x][y] = TileType.Goal;
            }
        }
    }

    /// <summary>
    /// Runs through an area the size of the bossroom and checks to see if all tiletypes
    /// are blackarea, in which case it allows for a boss room to be built
    /// </summary>
    /// <param name="xPos">Starting Position of the check on the X Axis</param>
    /// <param name="yPos">Starting Position of the check on the Y Axis</param>
    private void CheckIfAreaIsClear(int xPos, int yPos , int xSmall , int xLarge, int ySmall, int yLarge)
    {
        if (ExperienceManager.MyLevel < playerLevelToSpawnBossRoom)
        {
            spaceForBossRoom = false;
        }

        for (int x = xSmall; x <= xLarge; x += 2)
        {
            int xcoord = xPos + x;

            for (int y = ySmall; y <= yLarge; y += 2)
            {
                int ycoord = yPos + y;

                //Debug.Log("tile type at this spot is " + tiles[xcoord][ycoord]);

                if (tiles[xcoord][ycoord] != TileType.BlackArea)
                {
                    spaceForBossRoom = false;
                    Debug.Log("Found a non blackarea tile, stopping loop and I will not spawn a boos room");
                    return;
                }
            }
        }

        if (spaceForBossRoom)
        {
            //Debug.LogWarning("Area Clear");
            DungeonManager.instance.bossRoomAvailable = true;
        }
    }

    void CreateBossRoom(Room currentRoom)
    {
        int currentTile = 0;

        TileLocation bossPos = new TileLocation();

        if (corridors[corridors.Length - 1].direction == Direction.North)
        {
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //// For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;

                    if (currentTile >= 6 && currentTile <= 18)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    //------- section 2

                    else if (currentTile >= 29 && currentTile <= 30)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 31 && currentTile <= 43)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 44 && currentTile <= 45)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 3

                    else if (currentTile >= 52 && currentTile <= 53)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 54 && currentTile <= 70)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 71 && currentTile <= 72)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 4

                    else if (currentTile == 76)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 77 && currentTile <= 97)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 98)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 5
                    else if (currentTile == 101)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 102 && currentTile <= 122)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 123)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 6
                    else if (currentTile == 125)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 126 && currentTile <= 148)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 149)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 7
                    else if (currentTile == 150)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 151 && currentTile <= 173)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 174)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 8
                    else if (currentTile == 175)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 176 && currentTile <= 198)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 199)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 9
                    else if (currentTile == 200)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 201 && currentTile <= 223)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 224)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 10
                    else if (currentTile == 225)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 226 && currentTile <= 248)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 249)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 11
                    else if (currentTile == 250)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 251 && currentTile <= 273)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 274)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 12
                    else if (currentTile == 275)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 276 && currentTile <= 298)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 299)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 13
                    else if (currentTile == 300)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        StoreTileLocation(xCoord, yCoord, skullDoorLocations);
                    }

                    else if (currentTile >= 301 && currentTile <= 323)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        if (currentTile == 310)
                        {
                            bossPos.x = xCoord;
                            bossPos.y = yCoord;
                        }
                    }

                    else if (currentTile == 324)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 14
                    else if (currentTile == 325)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        StoreTileLocation(xCoord, yCoord, skullDoorLocations);
                    }

                    else if (currentTile >= 326 && currentTile <= 348)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 349)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 15
                    else if (currentTile == 350)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 351 && currentTile <= 373)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 374)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 16
                    else if (currentTile == 375)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 376 && currentTile <= 398)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 399)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 17
                    else if (currentTile == 400)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 401 && currentTile <= 423)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 424)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 18
                    else if (currentTile == 425)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 426 && currentTile <= 448)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 449)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 19
                    else if (currentTile == 450)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 451 && currentTile <= 473)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 474)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 20
                    else if (currentTile == 475)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 476 && currentTile <= 498)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 499)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 21
                    else if (currentTile == 501)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 502 && currentTile <= 522)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 523)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 22
                    else if (currentTile == 526)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 527 && currentTile <= 547)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 548)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 23
                    else if (currentTile >= 552 && currentTile <= 553)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 554 && currentTile <= 570)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 571 && currentTile <= 572)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 24
                    else if (currentTile >= 579 && currentTile <= 580)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 581 && currentTile <= 593)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 594 && currentTile <= 595)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 25
                    else if (currentTile >= 606 && currentTile <= 618)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                    //tiles[xCoord][yCoord] = TileType.Floor;
                    currentTile++;
                }
            }
        }

        else if (corridors[corridors.Length - 1].direction == Direction.East)
        {
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //// For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;

                    if (currentTile >= 6 && currentTile <= 10)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 11 && currentTile <= 12)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        StoreTileLocation(xCoord, yCoord, skullDoorLocations);
                    }

                    else if (currentTile >= 13 && currentTile <= 18)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    //------- section 2

                    else if (currentTile >= 29 && currentTile <= 30)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 31 && currentTile <= 43)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 44 && currentTile <= 45)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 3

                    else if (currentTile >= 52 && currentTile <= 53)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 54 && currentTile <= 70)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 71 && currentTile <= 72)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 4

                    else if (currentTile == 76)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 77 && currentTile <= 97)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 98)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 5
                    else if (currentTile == 101)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 102 && currentTile <= 122)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 123)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 6
                    else if (currentTile == 125)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 126 && currentTile <= 148)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 149)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 7
                    else if (currentTile == 150)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 151 && currentTile <= 173)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 174)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 8
                    else if (currentTile == 175)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 176 && currentTile <= 198)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 199)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 9
                    else if (currentTile == 200)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 201 && currentTile <= 223)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 224)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 10
                    else if (currentTile == 225)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 226 && currentTile <= 248)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 249)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 11
                    else if (currentTile == 250)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 251 && currentTile <= 273)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 274)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 12
                    else if (currentTile == 275)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 276 && currentTile <= 298)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 299)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 13
                    else if (currentTile == 300)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 301 && currentTile <= 323)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 324)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 14
                    else if (currentTile == 325)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 326 && currentTile <= 348)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        if (currentTile == 336)
                        {
                            bossPos.x = xCoord;
                            bossPos.y = yCoord;
                        }
                    }

                    else if (currentTile == 349)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 15
                    else if (currentTile == 350)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 351 && currentTile <= 373)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 374)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 16
                    else if (currentTile == 375)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 376 && currentTile <= 398)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 399)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 17
                    else if (currentTile == 400)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 401 && currentTile <= 423)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 424)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 18
                    else if (currentTile == 425)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 426 && currentTile <= 448)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 449)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 19
                    else if (currentTile == 450)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 451 && currentTile <= 473)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 474)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 20
                    else if (currentTile == 475)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 476 && currentTile <= 498)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 499)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 21
                    else if (currentTile == 501)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 502 && currentTile <= 522)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 523)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 22
                    else if (currentTile == 526)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 527 && currentTile <= 547)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 548)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 23
                    else if (currentTile >= 552 && currentTile <= 553)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 554 && currentTile <= 570)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 571 && currentTile <= 572)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 24
                    else if (currentTile >= 579 && currentTile <= 580)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 581 && currentTile <= 593)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 594 && currentTile <= 595)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 25
                    else if (currentTile >= 606 && currentTile <= 618)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                    //tiles[xCoord][yCoord] = TileType.Floor;
                    currentTile++;
                }
            }
        }

        else if (corridors[corridors.Length - 1].direction == Direction.South)
        {
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //// For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;

                    if (currentTile >= 6 && currentTile <= 18)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    //------- section 2

                    else if (currentTile >= 29 && currentTile <= 30)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 31 && currentTile <= 43)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 44 && currentTile <= 45)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 3

                    else if (currentTile >= 52 && currentTile <= 53)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 54 && currentTile <= 70)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 71 && currentTile <= 72)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 4

                    else if (currentTile == 76)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 77 && currentTile <= 97)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 98)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 5
                    else if (currentTile == 101)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 102 && currentTile <= 122)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 123)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 6
                    else if (currentTile == 125)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 126 && currentTile <= 148)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 149)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 7
                    else if (currentTile == 150)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 151 && currentTile <= 173)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 174)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 8
                    else if (currentTile == 175)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 176 && currentTile <= 198)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 199)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 9
                    else if (currentTile == 200)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 201 && currentTile <= 223)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 224)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 10
                    else if (currentTile == 225)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 226 && currentTile <= 248)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 249)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 11
                    else if (currentTile == 250)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 251 && currentTile <= 273)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 274)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 12
                    else if (currentTile == 275)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 276 && currentTile <= 298)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 299)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 13
                    else if (currentTile == 300)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 301 && currentTile <= 323)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 324)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        StoreTileLocation(xCoord, yCoord, skullDoorLocations);
                    }

                    // -------- end section

                    //------- section 14
                    else if (currentTile == 325)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 326 && currentTile <= 348)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        if (currentTile == 336)
                        {
                            bossPos.x = xCoord;
                            bossPos.y = yCoord;
                        }
                    }

                    else if (currentTile == 349)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        StoreTileLocation(xCoord, yCoord, skullDoorLocations);
                    }

                    // -------- end section

                    //------- section 15
                    else if (currentTile == 350)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 351 && currentTile <= 373)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 374)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 16
                    else if (currentTile == 375)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 376 && currentTile <= 398)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 399)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 17
                    else if (currentTile == 400)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 401 && currentTile <= 423)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 424)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 18
                    else if (currentTile == 425)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 426 && currentTile <= 448)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 449)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 19
                    else if (currentTile == 450)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 451 && currentTile <= 473)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 474)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 20
                    else if (currentTile == 475)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 476 && currentTile <= 498)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 499)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 21
                    else if (currentTile == 501)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 502 && currentTile <= 522)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 523)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 22
                    else if (currentTile == 526)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 527 && currentTile <= 547)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 548)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 23
                    else if (currentTile >= 552 && currentTile <= 553)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 554 && currentTile <= 570)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 571 && currentTile <= 572)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 24
                    else if (currentTile >= 579 && currentTile <= 580)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 581 && currentTile <= 593)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 594 && currentTile <= 595)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 25
                    else if (currentTile >= 606 && currentTile <= 618)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                    //tiles[xCoord][yCoord] = TileType.Floor;
                    currentTile++;
                }
            }
        }

        else if (corridors[corridors.Length - 1].direction == Direction.West)
        {
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //// For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;

                    if (currentTile >= 6 && currentTile <= 18)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    //------- section 2

                    else if (currentTile >= 29 && currentTile <= 30)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 31 && currentTile <= 43)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 44 && currentTile <= 45)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 3

                    else if (currentTile >= 52 && currentTile <= 53)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 54 && currentTile <= 70)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 71 && currentTile <= 72)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 4

                    else if (currentTile == 76)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 77 && currentTile <= 97)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 98)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 5
                    else if (currentTile == 101)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 102 && currentTile <= 122)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 123)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 6
                    else if (currentTile == 125)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 126 && currentTile <= 148)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 149)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 7
                    else if (currentTile == 150)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 151 && currentTile <= 173)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 174)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 8
                    else if (currentTile == 175)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 176 && currentTile <= 198)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 199)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 9
                    else if (currentTile == 200)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 201 && currentTile <= 223)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 224)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 10
                    else if (currentTile == 225)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 226 && currentTile <= 248)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 249)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 11
                    else if (currentTile == 250)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 251 && currentTile <= 273)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 274)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 12
                    else if (currentTile == 275)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 276 && currentTile <= 298)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 299)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 13
                    else if (currentTile == 300)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 301 && currentTile <= 323)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 324)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 14
                    else if (currentTile == 325)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 326 && currentTile <= 348)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        if (currentTile == 336)
                        {
                            bossPos.x = xCoord;
                            bossPos.y = yCoord;
                        }
                    }

                    else if (currentTile == 349)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 15
                    else if (currentTile == 350)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 351 && currentTile <= 373)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 374)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 16
                    else if (currentTile == 375)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 376 && currentTile <= 398)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 399)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 17
                    else if (currentTile == 400)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 401 && currentTile <= 423)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 424)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 18
                    else if (currentTile == 425)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 426 && currentTile <= 448)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 449)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 19
                    else if (currentTile == 450)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 451 && currentTile <= 473)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 474)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 20
                    else if (currentTile == 475)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 476 && currentTile <= 498)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 499)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 21
                    else if (currentTile == 501)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 502 && currentTile <= 522)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 523)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 22
                    else if (currentTile == 526)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 527 && currentTile <= 547)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile == 548)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 23
                    else if (currentTile >= 552 && currentTile <= 553)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 554 && currentTile <= 570)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 571 && currentTile <= 572)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 24
                    else if (currentTile >= 579 && currentTile <= 580)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 581 && currentTile <= 593)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                    }

                    else if (currentTile >= 594 && currentTile <= 595)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    //------- section 25
                    else if (currentTile >= 606 && currentTile <= 610)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    else if (currentTile >= 611 && currentTile <= 612)
                    {
                        tiles[xCoord][yCoord] = TileType.Floor;
                        StoreTileLocation(xCoord, yCoord, skullDoorLocations);
                    }

                    else if (currentTile >= 613 && currentTile <= 618)
                    {
                        tiles[xCoord][yCoord] = TileType.ColliderWall;
                    }

                    // -------- end section

                    // The coordinates in the jagged array are based on the room's position and it's width and height.
                    //tiles[xCoord][yCoord] = TileType.Floor;
                    currentTile++;
                }
            }
        }

        Vector2 vec = new Vector2(bossPos.x, bossPos.y);
        SpawnElement(vec, boss, false, true);
    }

    /// <summary>
    /// Store a tile location in a given list so that things may be instatiated 
    /// at a later time
    /// </summary>
    /// <param name="xCoord">X coordinate of the tile</param>
    /// <param name="yCoord">Y coordinate of the tile</param>
    /// <param name="list">List in which this value should be stored</param>
    private void StoreTileLocation(int xCoord, int yCoord, List<TileLocation> list)
    {
        TileLocation tmp = new TileLocation();
        tmp.x = xCoord;
        tmp.y = yCoord;
        list.Add(tmp);
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
                    #region North
                    case Direction.North:
                        yCoord += j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord + 1][yCoord] = TileType.Floor;

                        if (j != currentCorridor.corridorLength - 1)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.tilesInCorridor);
                            StoreTileLocation(xCoord + 1, yCoord, currentCorridor.tilesInCorridor);
                        }

                        //         Checking side tiles
                        //         X [current tile] X X

                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                        }
                        //if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                        //{
                        //    tiles[xCoord + 1][yCoord] = TileType.Floor;
                        //    //StoreTileLocation(xCoord + 1, yCoord, currentCorridor.tilesInCorridor);
                        //}
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
                            //StoreTileLocation(xCoord + 1, yCoord - 1, currentCorridor.tilesInCorridor);
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
                            //StoreTileLocation(xCoord + 1, yCoord + 1, currentCorridor.tilesInCorridor);
                        }
                        if (tiles[xCoord + 2][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord + 1] = TileType.ColliderWall;
                        }


                        break;
                    #endregion North

                    #region East
                    case Direction.East:
                        xCoord += j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord][yCoord - 1] = TileType.Floor;

                        if (j != currentCorridor.corridorLength - 1)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.tilesInCorridor);
                            StoreTileLocation(xCoord, yCoord - 1, currentCorridor.tilesInCorridor);
                        }

                        //         Checking side tiles
                        //                 X
                        //          [current tile]
                        //                 X
                        //                 X


                        //if (tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                        //{
                        //    tiles[xCoord][yCoord - 1] = TileType.Floor;
                        //    //StoreTileLocation(xCoord, yCoord-1, currentCorridor.tilesInCorridor);
                        //}
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
                            //StoreTileLocation(xCoord -1, yCoord -1, currentCorridor.tilesInCorridor);
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
                           // StoreTileLocation(xCoord + 1, yCoord -1, currentCorridor.tilesInCorridor);
                        }
                        if (tiles[xCoord + 1][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 2] = TileType.ColliderWall;
                        }
                        break;
                    #endregion East

                    #region South
                    case Direction.South:
                        yCoord -= j;


                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord + 1][yCoord] = TileType.Floor;

                        if (j != currentCorridor.corridorLength - 1)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.tilesInCorridor);
                            StoreTileLocation(xCoord + 1, yCoord, currentCorridor.tilesInCorridor);
                        }

                        //         Checking side tiles
                        //         X [current tile] X X

                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                        }
                        //if (tiles[xCoord + 1][yCoord] == TileType.BlackArea)
                        //{
                        //    tiles[xCoord + 1][yCoord] = TileType.Floor;
                        //    StoreTileLocation(xCoord + 1, yCoord, currentCorridor.tilesInCorridor);
                        //}
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
                           // StoreTileLocation(xCoord + 1, yCoord - 1, currentCorridor.tilesInCorridor);
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
                            //StoreTileLocation(xCoord + 1, yCoord + 1, currentCorridor.tilesInCorridor);
                        }
                        if (tiles[xCoord + 2][yCoord + 1] == TileType.BlackArea)
                        {
                            tiles[xCoord + 2][yCoord + 1] = TileType.ColliderWall;
                        }

                        break;
                    #endregion South

                    #region West
                    case Direction.West:
                        xCoord -= j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord][yCoord - 1] = TileType.Floor;

                        if (j != currentCorridor.corridorLength - 1)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.tilesInCorridor);
                            StoreTileLocation(xCoord, yCoord - 1, currentCorridor.tilesInCorridor);

                        }



                        //         Checking side tiles
                        //                 X
                        //          [current tile]
                        //                 X
                        //                 X


                        //if (tiles[xCoord][yCoord - 1] == TileType.BlackArea)
                        //{
                        //    tiles[xCoord][yCoord - 1] = TileType.Floor;
                        //    StoreTileLocation(xCoord, yCoord - 1, currentCorridor.tilesInCorridor);
                        //}
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
                            //StoreTileLocation(xCoord - 1, yCoord - 1, currentCorridor.tilesInCorridor);
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
                            //StoreTileLocation(xCoord + 1, yCoord - 1, currentCorridor.tilesInCorridor);
                        }
                        if (tiles[xCoord + 1][yCoord - 2] == TileType.BlackArea)
                        {
                            tiles[xCoord + 1][yCoord - 2] = TileType.ColliderWall;
                        }
                        break;

                        #endregion West
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
                    SpawnARandomFloorTile(i, j);
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
                    if (spaceForBossRoom)
                    {
                        SpawnARandomFloorTile(i,j);
                        continue;
                    }
                    //Debug.Log("calling goal tile");
                    // ... instantiate a goal see through floor over the top.
                    //Vector3 position = new Vector3(float)i, j, 0f);
                    Vector3 position = new Vector3(goalPos.x, goalPos.y, 0f);
                    Instantiate(goalFloor, position, Quaternion.identity);
                }

            }
        }
    }

    private void SpawnARandomFloorTile(int i, int j)
    {
        float rand = UnityEngine.Random.Range(0, 100);

        if (rand <= chanceOfVariantFloorTiles)
        {
            InstantiateFromArrayFloor(variantFloorTiles, i, j);
        }
        else
        {
            // Create a random index for the array.
            int randomIndex = UnityEngine.Random.Range(0, normalFloorTiles.Length);

            // The position to be instantiated at is based on the coordinates.
            Vector3 position = new Vector3(i, j, 0f);

            // Create an instance of the prefab from the random index of the array.
            GameObject tileInstance = Instantiate(normalFloorTiles[randomIndex], position, Quaternion.identity) as GameObject;

            // Set the tile's parent to the board holder.
            tileInstance.transform.parent = boardHolder.transform;
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
        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, RandomizeFacingDirectionOfObject()) as GameObject;

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
        GameObject wallInstance = Instantiate(prefabs[randomIndex], position, RandomizeFacingDirectionOfObject()) as GameObject;

        // Set the tile's parent to the board holder.
        wallInstance.transform.parent = outterWallHolder.transform;
    }

    private static Quaternion RandomizeFacingDirectionOfObject()
    {
        // randomize between 4 possible faces
        int randomFace = UnityEngine.Random.Range(0, 4);

        Quaternion spawnRotation;

        if (randomFace == 1)
        {
            spawnRotation = Quaternion.identity;
        }
        else if (randomFace == 2)
        {
            spawnRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (randomFace == 3)
        {
            spawnRotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            spawnRotation = Quaternion.Euler(0, 0, 270);
        }

        return spawnRotation;
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

    /// <summary>
    /// Sets the doors after the grid has been made so as to not affect the pathfinding
    /// This method is called from the Gamedetails as the Dungeon fades in.
    /// </summary>
    public void SetDoors()
    {
        foreach (TileLocation location in skullDoorLocations)
        {
            Vector3 position = new Vector3(location.x, location.y, -0.5f);
            Instantiate(skullDoor, position, Quaternion.identity);
        }
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

            SetTilesValuesForCorridors();
            SetTilesValuesForRooms();

            InstantiateTiles();
            InstantiateOuterWalls();

            BuildEntranceRoom();

            Debug.Log("There are " + DungeonManager.instance.enemiesInDungeon.Count + " enemies in the list");

            DungeonManager.instance.enemiesInDungeon.Clear();

            SpawnEnemies();
            SpawnDestructables();
            RemoveAllEnemiesNearStartPoint();
            SpawnObstructedCorridor();

            Debug.Log("There are " + DungeonManager.instance.enemiesInDungeon.Count + " enemies in the list");

            workDone = true;
        }
    }

    private void RemoveAllEnemiesNearStartPoint()
    {
        //// make a layerMask
        int layerId = 11;
        int enemyMask = 1 << layerId;

        Collider2D[] killzone = Physics2D.OverlapCircleAll(player.transform.position, 10, enemyMask);

        foreach (var enemy in killzone)
        {
            Debug.Log("Im a: " + enemy.transform.parent.gameObject.name);
            DungeonManager.instance.enemiesInDungeon.Remove(enemy.gameObject);
            Destroy(enemy.transform.parent.gameObject);
        }
    }

    /// <summary>
    /// Spawns enemies for each room by randomizing its lists of tiles using
    /// Fisher-Yates shuffle algorithm and by this getting a random tile in the room
    /// </summary>
    private void SpawnEnemies()
    {


        foreach (Room room in rooms)
        {
            if (room != rooms[0] && room != rooms[rooms.Length - 1])
            {
                room.middleFloorTilesInThisRoom.Shuffle();

                for (int i = 0; i < room.enemyAmount; i++)
                {
                    TileLocation spawnTile = room.middleFloorTilesInThisRoom[i];
                    Vector2 vec = new Vector2(spawnTile.x, spawnTile.y);

                    int rand = UnityEngine.Random.Range(0, enemy.Length);
                    room.middleFloorTilesInThisRoom.Remove(spawnTile);
                    SpawnElement(vec, enemy[rand]);
                }
            }
        }
    }

    private void SpawnDestructables()
    {
        foreach (Room room in rooms)
        {
            if (room != rooms[0] && room != rooms[rooms.Length - 1])
            {
                room.edgeFloorTilesOfThisRoom.Shuffle();

                for (int i = 0; i < room.destructablesAmount; i++)
                {
                    TileLocation spawnTile = room.edgeFloorTilesOfThisRoom[i];
                    Vector3 vec = new Vector3(spawnTile.x, spawnTile.y, -0.25f);

                    int rand = UnityEngine.Random.Range(0, destructables.Length);
                    room.edgeFloorTilesOfThisRoom.Remove(spawnTile);

                    /// check to see if the area is clear of mobs.
                    bool canSpawn = CheckArea(vec);

                    if (canSpawn)
                    {
                        SpawnElement(vec, destructables[rand], true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks the area around to see if there are any enemies
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool CheckArea(Vector3 pos)
    {
        int enemyLayer = 11;
        var enemyLayerMask = 1 << enemyLayer;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(pos, 1f, enemyLayerMask);

        if (enemies.Length != 0)
        {
            return false;
        }

        return true;
    }

    void SpawnObstructedCorridor()
    {
        foreach (Corridor cor in corridors)
        {
            if (cor.corridorLength <= obstructedCorridorMaxLength)
            {
                if (CheckIfCorridorIsRelevent(cor))
                {
                    foreach (TileLocation tile in cor.tilesInCorridor)
                    {
                        SpawnElement(tile.MyPosition, destructableWall, false, false, true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// make sure that this corridorr has not been drawn over by a room 
    /// or closely related to another corridor
    /// </summary>
    /// <returns></returns>
    bool CheckIfCorridorIsRelevent(Corridor currentCorridor)
    {
        bool isRelevant = true;

        switch (currentCorridor.direction)
        {
            case Direction.North:

                foreach (TileLocation tile in currentCorridor.tilesInCorridor)
                {
                    if (tile.x == currentCorridor.startXPos)
                    {
                        if (tiles[tile.x - 1][tile.y] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                    else if (tile.x == currentCorridor.startXPos + 1)
                    {
                        if (tiles[tile.x + 1][tile.y] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                }

                break;

            case Direction.East:

                foreach (TileLocation tile in currentCorridor.tilesInCorridor)
                {
                    if (tile.y == currentCorridor.startYPos)
                    {
                        if (tiles[tile.x][tile.y + 1] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                    else if (tile.y == currentCorridor.startYPos - 1)
                    {
                        if (tiles[tile.x][tile.y - 1] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                }

                break;

            case Direction.South:

                foreach (TileLocation tile in currentCorridor.tilesInCorridor)
                {
                    if (tile.x == currentCorridor.startXPos)
                    {
                        if (tiles[tile.x - 1][tile.y] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                    else if (tile.x == currentCorridor.startXPos + 1)
                    {
                        if (tiles[tile.x + 1][tile.y] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                }

                break;

            case Direction.West:

                foreach (TileLocation tile in currentCorridor.tilesInCorridor)
                {
                    if (tile.y == currentCorridor.startYPos)
                    {
                        if (tiles[tile.x][tile.y + 1] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                    else if (tile.y == currentCorridor.startYPos - 1)
                    {
                        if (tiles[tile.x][tile.y - 1] != TileType.ColliderWall)
                        {
                            return false;
                        }
                    }
                }

                break;
        }

        Debug.Log("Managed to make an obstructed corridor");
        return isRelevant;
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

[Serializable]
public class TileLocation
{
    public int x;
    public int y;

    public Vector2 MyPosition
    {
        get
        {
            return new Vector2(x, y);
        }
    }
}