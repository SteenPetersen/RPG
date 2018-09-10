using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardCreator : MonoBehaviour
{
    // The type of tile that will be laid in a specific position.
    public enum TileType
    {
        BlackArea, Floor, ColliderWall, Goal, SkullDoor, SecretWall, Transparent, WallTrapNorth, WallTrapEast, WallTrapSouth, WallTrapWest
    }

    public static BoardCreator instance;


    public int chanceOfChestPerRoom;
    public int chanceOfBetterChestPerRoom;
    //[Tooltip("How many enemies in corridors")]
    //public int percentageOfEnemiesInCorridors;
    [Tooltip("Maximum length of obstructed corridors")]
    public int obstructedCorridorMaxLength;
    public int playerLevelToSpawnBossRoom = 5;
    [SerializeField] IntRange floorsBeforeBoss = new IntRange(2, 4);

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
    public GameObject[] secretWalls;
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

    bool spaceForBossRoom = true;

    public TileType[][] tiles;                                   // A jagged array of tile types representing the board, like a grid.
    public List<Room> rooms;                                    // All the rooms that are created for this board.
    public List<Corridor> corridors;                            // All the corridors that connect the rooms.
    public List<Room> secretRooms = new List<Room>();
    public List<Corridor> secretCorridors = new List<Corridor>();
    [SerializeField] GameObject secretCover;


    private GameObject boardHolder;                             // GameObject that acts as a container for all other tiles.
    private GameObject enemyHolder;
    private GameObject outterWallHolder;

    private Vector2 goalPos;

    public CreateGraph aStarGridCreator;

    Room highestRoom;
    Room lowestRoom;
    Room eastRoom;
    Room farthestRoom;

    [SerializeField] GameObject trapWall;


    [SerializeField] GameObject[] prefabBossRooms;
    [SerializeField] GameObject[] prefabSecretRooms;
    
    [Tooltip("Total Width of the boss room to check this int must be divisable by 2")]
    [SerializeField] int checkTotalWidthBossRoom = 50; 
    [Tooltip("half of the total Width of the boss room")]
    [SerializeField] int halfWidthBossRoom = 25;
    [Tooltip("Boss Room Height which to check")]
    [SerializeField] int checkHeightBossRoom = 30;

    [Tooltip("Total Width of the secret rooms to check this int must be divisable by 2")]
    [SerializeField] int checkTotalWidthSecretRoom = 26;
    [Tooltip("half of the total Width of the boss room")]
    [SerializeField] int halfWidthSecretRoom = 13;
    [Tooltip("Boss Room Height which to check")]
    [SerializeField] int checkHeightSecretRoom = 20;

    /// <summary>
    /// Secret Room Variables
    /// </summary>
    public IntRange hiddenRoomHeight;
    public IntRange hiddenRoomWidth;

    [HideInInspector]
    public int currentBoardsNumberOfRooms;
    int currentBoardFloorsBeforeBoss;

    List<GameObject> tileTraps = new List<GameObject>();

    TileLocation northMost;
    TileLocation eastMost;
    TileLocation southMost;

    bool northSecret, eastSecret, southSecret;
    TileLocation northMain, northSide, eastMain, eastSide, southMain, southSide;
    SecretRoom northSR, eastSR, southSR;

    private void Awake()
    {
        player = PlayerController.instance.gameObject;
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

    /// <summary>
    /// Creates all rooms and corridors and 
    /// defines sizes and positions
    /// </summary>
    void CreateRoomsAndCorridors()
    {
        ///Create a list of rooms with a random size within the given range
        currentBoardsNumberOfRooms = numRooms.Random;
        rooms = new List<Room>(new Room[currentBoardsNumberOfRooms]);

        /// There is 1 less corridor than rooms
        corridors = new List<Corridor>(new Corridor[rooms.Count - 1]);

        /// Initialize the first room and Corridor
        rooms[0] = new Room();
        corridors[0] = new Corridor();

        /// Setup the first room
        rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

        /// Based on the first room, setup the first corridor
        corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);

        /// For the remaining room in the list, Set them 
        /// up based on the corridor entering them
        for (int i = 1; i < rooms.Count; i++)
        {
            /// Initialize a room
            rooms[i] = new Room();

            /// Setup the room based on the corridor entering it
            rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy, numDestructables);

            /// If it is the middle room, set the middleroom bool to true
            if (i == rooms.Count / 2)
            {
                //rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1], numEnemy, numDestructables, true);
            }

            /// if we still have corridors to make
            if (i < corridors.Count)
            {
                /// Initialize a corridor
                corridors[i] = new Corridor();

                /// Setup a corridor based on the room that was previously created
                corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
            }
        }

    }

    /// <summary>
    /// Spawns an elemtn into the dungeon
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="obj"></param>
    /// <param name="randRot"></param>
    /// <param name="isBoss"></param>
    /// <param name="wall"></param>
    /// <returns></returns>
    public GameObject SpawnElement(Vector3 pos, GameObject obj, bool randRot = false, bool isBoss = false, bool wall = false)
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

        else if (tmp.tag == "SecretRoomCover")
        {
            tmp.transform.parent = boardHolder.transform;
        }

        else if (tmp.tag == "Goal")
        {
            goalPos = pos;
        }

        if (wall)
        {
            tmp.transform.position = new Vector3(tmp.transform.position.x, tmp.transform.position.y, -0.5f);
        }

        return tmp;
    }

    /// <summary>
    /// Overload method which adds items to a list
    /// Initializes list if it hasn't already been initialized
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="obj"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public GameObject SpawnElement(Vector3 pos, GameObject obj, List<GameObject> list)
    {
        GameObject tmp = Instantiate(obj, pos, Quaternion.identity);

        if (list == null)
        {
            list = new List<GameObject>();
        }

        if (tmp.tag == "Torch")
        {
            list.Add(tmp);
        }

        if (tmp.tag == "SecretRoomCover")
        {
            tmp.transform.parent = boardHolder.transform;
            list.Add(tmp);
        }

        return tmp;
    }

    /// <summary>
    /// Overload Method to also define rotation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="obj"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public GameObject SpawnElement(Vector3 pos, GameObject obj, Vector3 rot)
    {
        GameObject tmp = Instantiate(obj, pos, Quaternion.identity);
        tmp.transform.Rotate(rot);

        return tmp;
    }

    /// <summary>
    /// Returns true if a grid was created
    /// </summary>
    /// <returns></returns>
    public bool CreateDungeonGraph()
    {
        return aStarGridCreator.AiGridPath(columns, rows, 0.5f, 1, false);
    }

    [SerializeField] float percentChanceTrapWall;

    void SetTilesValuesForRooms()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Count; i++)
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

            if (!currentRoom.isBossRoom && currentRoom != rooms[0])
            {
                // ... and for each room go through it's width.
                for (int j = 0; j < currentRoom.roomWidth; j++)
                {
                    int xCoord = currentRoom.xPos + j;

                    //// For each horizontal tile, go up vertically through the room's height.
                    for (int k = 0; k < currentRoom.roomHeight; k++)
                    {
                        int yCoord = currentRoom.yPos + k;

                        int rand = UnityEngine.Random.Range(0, 100);

                        /// Until player kinda understand the game dont spawn these traps
                        if (ExperienceManager.MyLevel <= 2)
                        {
                            rand = 100;
                        }

                        // Make the left wall have colliders
                        if (xCoord == currentRoom.xPos)
                        {
                            if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord - 1] == TileType.Transparent)
                            {

                                tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;

                            }

                            if (tiles[xCoord - 1][yCoord] == TileType.BlackArea || tiles[xCoord - 1][yCoord] == TileType.Transparent)
                            {
                                if (rand < percentChanceTrapWall)
                                {
                                    tiles[xCoord - 1][yCoord] = TileType.WallTrapWest;
                                }
                                else
                                {
                                    tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                                }
                            }

                            if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                            {
                                if (tiles[xCoord - 1][yCoord] == TileType.BlackArea || tiles[xCoord - 1][yCoord] == TileType.Transparent)
                                {
                                    if (rand < percentChanceTrapWall)
                                    {
                                        tiles[xCoord - 1][yCoord] = TileType.WallTrapWest;
                                    }
                                    else
                                    {
                                        tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                                    }

                                }

                                if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord + 1] == TileType.Transparent)
                                {
                                    tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                                }
                            }


                        }

                        // Make the Right wall have colliders
                        if (xCoord == currentRoom.xPos + currentRoom.roomWidth - 1)
                        {
                            if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord - 1] == TileType.Transparent)
                            {
                                tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                            }

                            if (tiles[xCoord + 1][yCoord] == TileType.BlackArea || tiles[xCoord + 1][yCoord] == TileType.Transparent)
                            {
                                if (rand < percentChanceTrapWall)
                                {
                                    tiles[xCoord + 1][yCoord] = TileType.WallTrapEast;
                                }
                                else
                                {
                                    tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                                }

                            }

                            if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                            {
                                if (tiles[xCoord + 1][yCoord] == TileType.BlackArea || tiles[xCoord + 1][yCoord] == TileType.Transparent)
                                {
                                    if (rand < percentChanceTrapWall)
                                    {
                                        tiles[xCoord + 1][yCoord] = TileType.WallTrapEast;
                                    }
                                    else
                                    {
                                        tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                                    }

                                }

                                if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord + 1] == TileType.Transparent)
                                {
                                    tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                                }
                            }

                        }

                        // Make the Top wall have colliders
                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                        {
                            if (tiles[xCoord][yCoord + 1] == TileType.BlackArea || tiles[xCoord][yCoord + 1] == TileType.Transparent)
                            {
                                if (rand < percentChanceTrapWall)
                                {
                                    tiles[xCoord][yCoord + 1] = TileType.WallTrapNorth;
                                }
                                else
                                {
                                    tiles[xCoord][yCoord + 1] = TileType.ColliderWall;
                                }
                            }

                            if (xCoord == currentRoom.xPos)
                            {
                                if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord + 1] == TileType.Transparent)
                                {
                                    tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                                }
                            }

                            if (xCoord == currentRoom.xPos + currentRoom.roomWidth - 1)
                            {
                                if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord + 1] == TileType.Transparent)
                                {
                                    tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                                }
                            }

                        }

                        // Make the Bottom wall have colliders
                        if (yCoord == currentRoom.yPos)
                        {
                            if (tiles[xCoord][yCoord - 1] == TileType.BlackArea || tiles[xCoord][yCoord - 1] == TileType.Transparent)
                            {
                                if (rand < percentChanceTrapWall)
                                {
                                    tiles[xCoord][yCoord - 1] = TileType.WallTrapSouth;
                                }
                                else
                                {
                                    tiles[xCoord][yCoord - 1] = TileType.ColliderWall;
                                }
                            }

                            if (xCoord == currentRoom.xPos)
                            {
                                if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord - 1] == TileType.Transparent)
                                {
                                    tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                                }
                            }

                            if (xCoord == currentRoom.xPos + currentRoom.roomWidth - 1)
                            {
                                if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord - 1] == TileType.Transparent)
                                {
                                    tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                                }
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
        }
    }

    void SetTilesValuesForCorridors()
    {
        // Go through every corridor...
        for (int i = 0; i < corridors.Count; i++)
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

    void SetTileValuesForSecretRooms()
    {
        // Go through all the rooms...
        for (int i = 0; i < secretRooms.Count; i++)
        {
            Room currentRoom = secretRooms[i];

            /// ... and for each room go through it's width.
            for (int x = 0; x < currentRoom.roomWidth; x++)
            {
                int xCoord = currentRoom.xPos + x;

                /// For each horizontal tile, go up vertically through the room's height.
                for (int y = 0; y < currentRoom.roomHeight; y++)
                {
                    int yCoord = currentRoom.yPos + y;

                    /// Make the left wall have colliders
                    if (xCoord == currentRoom.xPos)
                    {
                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord - 1] == TileType.Transparent)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }
                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea || tiles[xCoord - 1][yCoord] == TileType.Transparent)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                        }

                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                        {
                            if (tiles[xCoord - 1][yCoord] == TileType.BlackArea || tiles[xCoord - 1][yCoord] == TileType.Transparent)
                            {
                                tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
                            }

                            if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord + 1] == TileType.Transparent)
                            {
                                tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                            }
                        }


                    }

                    /// Make the Right wall have colliders
                    if (xCoord == currentRoom.xPos + currentRoom.roomWidth - 1)
                    {
                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord - 1] == TileType.Transparent)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord] == TileType.BlackArea || tiles[xCoord + 1][yCoord] == TileType.Transparent)
                        {
                            tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                        }

                        if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                        {
                            if (tiles[xCoord + 1][yCoord] == TileType.BlackArea || tiles[xCoord + 1][yCoord] == TileType.Transparent)
                            {
                                tiles[xCoord + 1][yCoord] = TileType.ColliderWall;
                            }

                            if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord + 1] == TileType.Transparent)
                            {
                                tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                            }
                        }

                    }

                    /// Make the Top wall have colliders
                    if (yCoord == currentRoom.yPos + currentRoom.roomHeight - 1)
                    {
                        if (tiles[xCoord][yCoord + 1] == TileType.BlackArea || tiles[xCoord][yCoord + 1] == TileType.Transparent)
                        {
                            tiles[xCoord][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord - 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord + 1] == TileType.Transparent)
                        {
                            tiles[xCoord - 1][yCoord + 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord + 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord + 1] == TileType.Transparent)
                        {
                            tiles[xCoord + 1][yCoord + 1] = TileType.ColliderWall;
                        }
                    }

                    /// Make the Bottom wall have colliders
                    if (yCoord == currentRoom.yPos)
                    {
                        if (tiles[xCoord][yCoord - 1] == TileType.BlackArea || tiles[xCoord][yCoord - 1] == TileType.Transparent)
                        {
                            tiles[xCoord][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord - 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord - 1][yCoord - 1] == TileType.Transparent)
                        {
                            tiles[xCoord - 1][yCoord - 1] = TileType.ColliderWall;
                        }

                        if (tiles[xCoord + 1][yCoord - 1] == TileType.BlackArea || tiles[xCoord + 1][yCoord - 1] == TileType.Transparent)
                        {
                            tiles[xCoord + 1][yCoord - 1] = TileType.ColliderWall;
                        }
                    }

                    /// The coordinates in the jagged array are based on the room's position and it's width and height.
                    tiles[xCoord][yCoord] = TileType.Floor;

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
    }

    void SetTilesValuesForSecretCorridors()
    {
        // Go through every corridor...
        for (int i = 0; i < secretCorridors.Count; i++)
        {
            Corridor currentCorridor = secretCorridors[i];

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

                        if (j == 0)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.secretFloorTiles);
                            StoreTileLocation(xCoord + 1, yCoord, currentCorridor.secretFloorTiles);
                        }


                        //         Checking side tiles
                        //         X [current tile] X X

                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
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
                    #endregion North

                    #region East
                    case Direction.East:
                        xCoord += j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord][yCoord - 1] = TileType.Floor;

                        if (j != currentCorridor.corridorLength - 1)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.secretFloorTiles);
                            StoreTileLocation(xCoord, yCoord - 1, currentCorridor.secretFloorTiles);
                        }

                        //         Checking side tiles
                        //                 X
                        //          [current tile]
                        //                 X
                        //                 X


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
                    #endregion East

                    #region South
                    case Direction.South:
                        yCoord -= j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord + 1][yCoord] = TileType.Floor;

                        if (j == 0)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.secretFloorTiles);
                            StoreTileLocation(xCoord + 1, yCoord, currentCorridor.secretFloorTiles);
                        }


                        //         Checking side tiles
                        //         X [current tile] X X

                        if (tiles[xCoord - 1][yCoord] == TileType.BlackArea)
                        {
                            tiles[xCoord - 1][yCoord] = TileType.ColliderWall;
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
                    #endregion South

                    #region West
                    case Direction.West:
                        xCoord -= j;

                        tiles[xCoord][yCoord] = TileType.Floor;
                        tiles[xCoord][yCoord - 1] = TileType.Floor;


                        if (j != currentCorridor.corridorLength - 1)
                        {
                            StoreTileLocation(xCoord, yCoord, currentCorridor.secretFloorTiles);
                            StoreTileLocation(xCoord, yCoord - 1, currentCorridor.secretFloorTiles);
                        }

                        //         Checking side tiles
                        //                 X
                        //          [current tile]
                        //                 X
                        //                 X


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

                        #endregion West
                }

                ///If you're on the last tile of the length
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

                // If the tile type is a west trap...
                else if (tiles[i][j] == TileType.WallTrapWest)
                {
                    Quaternion rot = Quaternion.Euler(0, 0, 90);
                    InstantiateWall(trapWall, i, j, rot);
                }

                // If the tile type is a west trap...
                else if (tiles[i][j] == TileType.WallTrapEast)
                {
                    Quaternion rot = Quaternion.Euler(0, 0, -90);
                    InstantiateWall(trapWall, i, j, rot);
                }

                // If the tile type is a west trap...
                else if (tiles[i][j] == TileType.WallTrapNorth)
                {
                    Quaternion rot = Quaternion.Euler(0, 0, 0);
                    InstantiateWall(trapWall, i, j, rot);
                }

                // If the tile type is a west trap...
                else if (tiles[i][j] == TileType.WallTrapSouth)
                {
                    Quaternion rot = Quaternion.Euler(0, 0, 180);
                    InstantiateWall(trapWall, i, j, rot);
                }

                // If the tile type is Goal...
                else if (tiles[i][j] == TileType.Goal)
                {
                    Vector3 position = new Vector3(goalPos.x, goalPos.y, 0f);
                    Instantiate(goalFloor, position, Quaternion.identity);
                }

            }
        }
    }

    void InstantiateWall(GameObject wall, float x, float y, Quaternion rot)
    {
        // The position to be instantiated at is based on the coordinates.
        Vector3 position = new Vector3(x, y, -0.5f);

        // Create an instance of the prefab from the random index of the array.
        GameObject wallInstance = Instantiate(wall, position, rot) as GameObject;

        // Set the tile's parent to the board holder.
        wallInstance.transform.parent = outterWallHolder.transform;
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

    void SpawnNormalFloorTile(int x, int y)
    {
        // Create a random index for the array.
        int randomIndex = UnityEngine.Random.Range(0, normalFloorTiles.Length);

        // The position to be instantiated at is based on the coordinates.
        Vector3 position = new Vector3(x, y, 0f);

        // Create an instance of the prefab from the random index of the array.
        GameObject tileInstance = Instantiate(normalFloorTiles[randomIndex], position, Quaternion.identity) as GameObject;

        // Set the tile's parent to the board holder.
        tileInstance.transform.parent = boardHolder.transform;
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

        ///Add all traps to an array of traps so we can 
        ///make sure they dont spawn at the entrace
        if (tileInstance.name.StartsWith("Trap"))
        {
            tileTraps.Add(tileInstance);
        }
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

    /// <summary>
    /// Find the farthest room from the entrance and 
    /// sets the goal location randomly inside that room
    /// </summary>
    private void SetGoal()
    {
        farthestRoom.SetGoalLocation();
    }

    /// <summary>
    /// Returns the average rooms height of all
    /// rooms in this dungeon
    /// </summary>
    /// <returns></returns>
    private float FindAverageRoomHeight()
    {
        float tmp = 0;

        foreach (var room in rooms)
        {
            tmp += room.middleTile.y;
        }

        return tmp / rooms.Count;
    }

    /// <summary>
    /// Find the room farthest away from the start room and returns it
    /// </summary>
    /// <param name="farthestRoom"></param>
    /// <returns></returns>
    private Room FindFarthestRoom(Room farthestRoom)
    {
        float maxDist = 0;

        foreach (var room in rooms)
        {
            float dist = Vector2.Distance(rooms[0].middleTile.MyPosition, room.middleTile.MyPosition);

            if (dist > maxDist)
            {
                maxDist = dist;
                farthestRoom = room;
            }
        }

        return farthestRoom;
    }

    /// <summary>
    /// Finds the highest room by checking against all rooms against the average height
    /// when it has checked all rooms returns the highest one.
    /// </summary>
    /// <param name="avg"></param>
    /// <param name="highestRoom"></param>
    /// <returns></returns>
    private Room FindHighestRoom(float avg, Room highestRoom)
    {
        float y = avg;

        foreach (var room in rooms)
        {
            if (room != rooms[0])
            {
                if (room.middleTile.y > y)
                {
                    highestRoom = room;
                    y = room.middleTile.y;
                }
            }
        }

        return highestRoom;
    }

    /// <summary>
    /// Finds the lowest room by checking against all rooms against the average height
    /// when it has checked all rooms returns the lowest one.
    /// </summary>
    /// <param name="avg"></param>
    /// <param name="highestRoom"></param>
    /// <returns></returns>
    private Room FindLowestRoom(float avg, Room lowestRoom)
    {
        float y = rows;

        foreach (var room in rooms)
        {
            if (room != rooms[0])
            {
                if (room.middleTile.y < y)
                {
                    lowestRoom = room;
                    y = room.middleTile.y;
                }
            }
        }

        return lowestRoom;
    }

    /// <summary>
    /// Finds the Eastern most room by checking the x values
    /// of all rooms and return the one with the highest value
    /// </summary>
    /// <param name="avg"></param>
    /// <param name="eastRoom"></param>
    /// <returns></returns>
    private Room FindEastRoom(float avg, Room eastRoom)
    {
        float x = 0;

        foreach (var room in rooms)
        {
            if (room.middleTile.x > x)
            {
                eastRoom = room;
                x = room.middleTile.x;
            }
        }

        return eastRoom;
    }

    int FindLowestYValue()
    {
        TileLocation tile = new TileLocation(columns / 2, rows / 2);

        foreach (var t in allRoomFloorTiles)
        {
            if (t.y < tile.y)
            {
                tile = t;
            }
        }

        return tile.y;
    }

    int FindLowestXValue()
    {
        TileLocation tile = new TileLocation(columns / 2, rows / 2);

        foreach (var t in allRoomFloorTiles)
        {
            if (t.x < tile.x)
            {
                tile = t;
            }
        }

        return tile.x;
    }

    int FindHighestYValue()
    {
        TileLocation tile = new TileLocation(columns / 2, rows / 2);

        foreach (var t in allRoomFloorTiles)
        {
            if (t.y > tile.y)
            {
                tile = t;
            }
        }

        return tile.y;
    }

    int FindHighestXValue()
    {
        TileLocation tile = new TileLocation(columns / 2, rows / 2);

        foreach (var t in allRoomFloorTiles)
        {
            if (t.x > tile.x)
            {
                tile = t;
            }
        }

        return tile.x;
    }

    /// <summary>
    /// Removes all traps from the entrance then clears the 
    /// tileTrap list.
    /// </summary>
    void RemoveAllTrapsFromEntrance()
    {
        foreach (var trap in tileTraps)
        {

            float dist = Vector2.Distance(player.transform.position, trap.transform.position);

            if (dist < 10)
            {
                SpawnNormalFloorTile((int)trap.transform.position.x, (int)trap.transform.position.y);
                Destroy(trap);
            }
        }

        tileTraps.Clear();
    }

    /// <summary>
    /// Spawns enemies for each room by randomizing its lists of tiles using
    /// Fisher-Yates shuffle algorithm and by this getting a random tile in the room
    /// </summary>
    private void SpawnEnemies()
    {
        foreach (Room room in rooms)
        {
            if (room != rooms[0] && room != rooms[rooms.Count - 1])
            {
                room.middleFloorTilesInThisRoom.Shuffle();

                for (int i = 0; i < room.enemyAmount; i++)
                {
                    TileLocation spawnTile = room.middleFloorTilesInThisRoom[i];
                    Vector2 vec = new Vector2(spawnTile.x, spawnTile.y);

                    float distToPlayer = Vector2.Distance(player.transform.position, vec);

                    if (distToPlayer > 10)
                    {
                        int rand = UnityEngine.Random.Range(0, enemy.Length);
                        room.middleFloorTilesInThisRoom.Remove(spawnTile);
                        SpawnElement(vec, enemy[rand]);
                    }

                }
            }
        }
    }

    private void SpawnDestructables()
    {
        foreach (Room room in rooms)
        {
            if (room != rooms[0] && room != rooms[rooms.Count - 1])
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

                if (room.chestSpawned)
                {
                    int rand = UnityEngine.Random.Range(0, 100);

                    TileLocation spawnTile = new TileLocation();
                    Vector2 vec = new Vector2();

                    if (rand <= 25)
                    {
                        spawnTile = room.edgeFloorTilesOfThisRoom[0];
                        vec = new Vector2(spawnTile.x, spawnTile.y);
                        room.edgeFloorTilesOfThisRoom.Remove(spawnTile);

                    }
                    else
                    {
                        spawnTile = room.middleFloorTilesInThisRoom[0];
                        vec = new Vector2(spawnTile.x, spawnTile.y);
                        room.middleFloorTilesInThisRoom.Remove(spawnTile);
                    }

                    bool canSpawn = CheckArea(vec);

                    if (canSpawn)
                    {
                        if (room.chestQuality == 0)
                        {
                            SpawnElement(vec, chestLowerTier, true);
                            return;
                        }

                        SpawnElement(vec, chestHigherTier, true);
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

    //public void SpawnSecretWalls()
    //{
    //    foreach (Corridor cor in secretCorridors)
    //    {
    //        foreach (TileLocation tile in cor.secretFloorTiles)
    //        {
    //            GameObject secretWall = SpawnElement(tile.MyPosition, secretWalls[0], false, false, true);
    //            SecretWall sw = secretWall.GetComponent<SecretWall>();
    //            sw.myRoom = cor.mySecretRoom;
    //        }
    //    }
    //}



    /// <summary>
    /// After setting the location of the goal, this method sets the transparent floor above it.
    /// </summary>
    public void SpawnGoalFloor(TileLocation goalFloorLocation)
    {
        tiles[goalFloorLocation.x][goalFloorLocation.y] = TileType.Goal;
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

        return isRelevant;
    }

    void FunctionHolderForTestingPurposes()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Count; i++)
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

    public IEnumerator InitializeMap()
    {
        bool workDone = false;
        DungeonManager.instance.bossRoomAvailable = false;

        while (!workDone)
        {
            /// Let the engine run for a frame
            yield return null;

            /// Find the board and enemy holder.
            boardHolder = GameObject.Find("BoardHolder");
            enemyHolder = new GameObject("EnemyHolder");
            outterWallHolder = GameObject.Find("OutterWallHolder");
            currentBoardFloorsBeforeBoss = floorsBeforeBoss.Random;

            percentChanceTrapWall = Mathf.Clamp(ExperienceManager.MyLevel - 3.5f, 0, 5);


            SetupTilesArray();

            CreateRoomsAndCorridors();

            try
            {
                if (DebugControl.debugDungeon)
                {
                    Debug.LogWarning("Running SetTileValuesForcorridors");
                }

                SetTilesValuesForCorridors();

                if (DebugControl.debugDungeon)
                {
                    Debug.LogWarning("Running SetTileValuesForRooms");
                }

                SetTilesValuesForRooms();

                if (DebugControl.debugDungeon)
                {
                    Debug.LogWarning("Running SetTargetRooms");
                }

                SetTargetRoomsAndTiles();

                if (DebugControl.debugDungeon)
                {
                    Debug.LogWarning("Running CheckSpaceForBossRoom");
                }

                if (ExperienceManager.MyLevel >= 5 && DungeonManager.dungeonLevel > currentBoardFloorsBeforeBoss)
                {
                    CheckForBossRoom();
                }

                if (DebugControl.debugDungeon)
                {
                    Debug.LogWarning("Running Checkforsecretrooms");
                }

                ///Reliant on setTileValuesForRooms so it
                ///knows wether the rooms it checks are boss rooms or not
                CheckForSecretRooms();

                if (!DungeonManager.instance.bossRoomAvailable)
                {
                    if (DebugControl.debugDungeon)
                    {
                        Debug.LogWarning("Running SetGoal");
                    }

                    SetGoal();
                }

            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError("Caught an index Out of Range Exception redoing the dungeon");
                Scene s = SceneManager.GetActiveScene();
                StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene(s.name, "Generating Rooms"));
            }

            InstantiateOuterWalls();

            BuildEntranceRoom(); // TODO rotate camera randomly and rotate the skull to fit

            /// Clear the enemies from the previous floor from the list
            DungeonManager.instance.enemiesInDungeon.Clear();

            SpawnEnemies();
            SpawnDestructables();

            SpawnObstructedCorridor();

            InstantiateTiles();

            RemoveAllTrapsFromEntrance();

            workDone = true;
        }
    }

    /// <summary>
    /// Finds all the rooms that will be used for creating secret
    /// and boos rooms if there is space going off them.
    /// </summary>
    private void SetTargetRoomsAndTiles()
    {
        float avg = FindAverageRoomHeight();

        highestRoom = rooms[1];
        highestRoom = FindHighestRoom(avg, highestRoom);

        northMost = new TileLocation(highestRoom.middleTile.x, highestRoom.yPos + highestRoom.roomHeight + 1);

        lowestRoom = rooms[1];
        lowestRoom = FindLowestRoom(avg, lowestRoom);

        southMost = new TileLocation(lowestRoom.middleTile.x, lowestRoom.yPos - 2);

        eastRoom = rooms[rooms.Count / 2];
        eastRoom = FindEastRoom(avg, eastRoom);

        eastMost = new TileLocation(eastRoom.xPos + eastRoom.roomWidth + 1, eastRoom.middleTile.y);

        farthestRoom = rooms[rooms.Count - 1];
        farthestRoom = FindFarthestRoom(farthestRoom);
    }

    /// <summary>
    /// Checks to see if a boss room can be spawn and if it
    /// can then it spawns a randomly determined prefab boss room
    /// </summary>
    private void CheckForBossRoom()
    {
        // Check all three directions for space
        bool northBossRoom = CheckSpaceForBossRoom(northMost, Direction.North);

        int rand = UnityEngine.Random.Range(0, prefabBossRooms.Length);

        if (northBossRoom)
        {
            Vector2 pos = new Vector2(northMost.x, northMost.y);
            SpawnElement(pos, prefabBossRooms[rand], Vector3.zero);
            highestRoom.isBossRoom = true;
            ClearBossArea(northMost, Direction.North);
            DungeonManager.instance.bossRoomAvailable = true;
            return;
        }

        bool southBossRoom = CheckSpaceForBossRoom(southMost, Direction.South);

        if (southBossRoom)
        {
            Vector2 pos = new Vector2(southMost.x, southMost.y);
            Vector3 rot = new Vector3(0, 0, -180);
            SpawnElement(pos, prefabBossRooms[rand], rot);
            lowestRoom.isBossRoom = true;
            ClearBossArea(southMost, Direction.South);
            DungeonManager.instance.bossRoomAvailable = true;
            return;
        }

        bool EastBossRoom = CheckSpaceForBossRoom(eastMost, Direction.East);

        if (EastBossRoom)
        {
            Vector2 pos = new Vector2(eastMost.x, eastMost.y);
            Vector3 rot = new Vector3(0, 0, -90);
            SpawnElement(pos, prefabBossRooms[rand], rot);
            eastRoom.isBossRoom = true;
            ClearBossArea(eastMost, Direction.East);
            DungeonManager.instance.bossRoomAvailable = true;
            return;
        }

    }

    private void ClearBossArea(TileLocation startTile, Direction dir)
    {
        switch (dir)
        {
            case Direction.North:

                TileLocation N = new TileLocation(startTile.x - halfWidthBossRoom, startTile.y);

                for (int x = N.x; x <= N.x + checkTotalWidthBossRoom; x++)
                {
                    for (int y = N.y; y <= N.y + checkHeightBossRoom; y++)
                    {
                        if (tiles[x][y] != TileType.ColliderWall)
                        {
                            tiles[x][y] = TileType.Transparent;
                        }
                        
                    }
                }

                tiles[startTile.x][startTile.y - 1] = TileType.Floor;
                tiles[startTile.x + 1][startTile.y - 1] = TileType.Floor;

                break;


            case Direction.East:

                TileLocation E = new TileLocation(startTile.x, startTile.y + halfWidthBossRoom);

                for (int x = E.x; x <= E.x + checkHeightBossRoom; x++)
                {
                    for (int y = E.y; y >= E.y - checkTotalWidthBossRoom; y--)
                    {
                        if (tiles[x][y] != TileType.ColliderWall)
                        {
                            tiles[x][y] = TileType.Transparent;
                        }
                    }
                }

                tiles[startTile.x - 1][startTile.y] = TileType.Floor;
                tiles[startTile.x - 1][startTile.y - 1] = TileType.Floor;

                break;
            case Direction.South:

                TileLocation s = new TileLocation(startTile.x - halfWidthBossRoom, startTile.y - checkHeightBossRoom);

                for (int x = s.x; x <= s.x + checkTotalWidthBossRoom; x++)
                {
                    for (int y = s.y; y <= s.y + checkHeightBossRoom; y++)
                    {
                        if (tiles[x][y] != TileType.ColliderWall)
                        {
                            tiles[x][y] = TileType.Transparent;
                        }
                    }
                }

                tiles[startTile.x][startTile.y + 1] = TileType.Floor;
                tiles[startTile.x - 1][startTile.y + 1] = TileType.Floor;

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Runs through and check a large area for space for
    /// a Boss room, in three different directions corresponding 
    /// to three different rooms, highest, lowest and eastMost
    /// </summary>
    /// <param name="startTile"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool CheckSpaceForBossRoom(TileLocation startTile, Direction dir)
    {
        if (DebugControl.debugDungeon)
        {
            Debug.Log("Scanning area for BossRoom");
        }

        switch (dir)
        {
            case Direction.North:

                /// Sanity Check
                if (startTile.x - halfWidthBossRoom > 5 && startTile.x + halfWidthBossRoom < (columns - 5))
                {
                    if (startTile.y + checkHeightBossRoom < (rows - 5))
                    {
                        TileLocation startCheckPosition = new TileLocation(startTile.x - halfWidthBossRoom, startTile.y);

                        for (int x = startCheckPosition.x; x <= startCheckPosition.x + checkTotalWidthBossRoom; x++)
                        {
                            for (int y = startCheckPosition.y; y <= startCheckPosition.y + checkHeightBossRoom; y++)
                            {
                                if (tiles[x][y] != TileType.BlackArea)
                                {
                                    return false;
                                }
                            }
                        }

                        if (DebugControl.debugDungeon)
                        {
                            Debug.Log("There is Space for a boss room in the North");
                        }

                        return true;

                    }
                }

                if (DebugControl.debugDungeon)
                {
                    Debug.LogError("Failed the sanity Check " + dir);
                }

                return false;

            case Direction.East:

                /// Sanity Check
                if (startTile.x + checkHeightBossRoom < (columns - 5))
                {
                    if (startTile.y + halfWidthBossRoom < (rows - 5) && startTile.y - halfWidthBossRoom > 5)
                    {
                        TileLocation startCheckPosition = new TileLocation(startTile.x, startTile.y + halfWidthBossRoom);

                        for (int x = startCheckPosition.x; x <= startCheckPosition.x + checkHeightBossRoom; x++)
                        {
                            for (int y = startCheckPosition.y; y >= startCheckPosition.y - checkTotalWidthBossRoom; y--)
                            {
                                if (tiles[x][y] != TileType.BlackArea)
                                {
                                    return false;
                                }
                            }
                        }

                        if (DebugControl.debugDungeon)
                        {
                            Debug.Log("There is Space for a boss room in the East");
                        }

                        return true;

                    }
                }


                if (DebugControl.debugDungeon)
                {
                    Debug.LogError("Failed the sanity Check" + dir);
                }

                return false;

            case Direction.South:

                /// Sanity Check
                if (startTile.x - halfWidthBossRoom > 5 && startTile.x + halfWidthBossRoom < (columns - 5))
                {
                    if (startTile.y - checkHeightBossRoom > 5)
                    {
                        TileLocation startCheckPosition = new TileLocation(startTile.x - halfWidthBossRoom, startTile.y - checkHeightBossRoom);

                        for (int x = startCheckPosition.x; x <= startCheckPosition.x + checkTotalWidthBossRoom; x++)
                        {
                            for (int y = startCheckPosition.y; y <= startCheckPosition.y + checkHeightBossRoom; y++)
                            {
                                if (tiles[x][y] != TileType.BlackArea)
                                {
                                    return false;
                                }
                            }

                        }

                        if (DebugControl.debugDungeon)
                        {
                            Debug.Log("There is Space for a boss room in the South");
                        }

                        return true;

                    }
                }


                if (DebugControl.debugDungeon)
                {
                    Debug.LogError("Failed the sanity Check" + dir);
                }
                return false;

            default:
                break;
        }

        return true;

    }

    private void CheckForSecretRooms()
    {
        if (!highestRoom.isBossRoom)
        {
            bool northSecretRoom = CheckSpaceForSecretRoom(northMost, Direction.North);

            int rand = UnityEngine.Random.Range(0, prefabSecretRooms.Length);

            if (northSecretRoom)
            {
                Vector2 pos = new Vector2(northMost.x, northMost.y);
                GameObject NSR = SpawnElement(pos, prefabSecretRooms[rand], Vector3.zero);
                ClearSecretArea(northMost, Direction.North);
                northSecret = true;
                northSR = NSR.GetComponent<SecretRoom>();
            }
        }

        if (!eastRoom.isBossRoom)
        {
            bool eastSecretRoom = CheckSpaceForSecretRoom(eastMost, Direction.East);

            int rand = UnityEngine.Random.Range(0, prefabSecretRooms.Length);

            if (eastSecretRoom)
            {
                Vector2 pos = new Vector2(eastMost.x, eastMost.y);
                Vector3 rot = new Vector3(0, 0, -90);
                GameObject ESR = SpawnElement(pos, prefabSecretRooms[rand], rot);
                ClearSecretArea(eastMost, Direction.East);
                eastSecret = true;
                eastSR = ESR.GetComponent<SecretRoom>();
            }
        }

        if (!lowestRoom.isBossRoom)
        {
            bool southSecretRoom = CheckSpaceForSecretRoom(southMost, Direction.South);

            int rand = UnityEngine.Random.Range(0, prefabSecretRooms.Length);

            if (southSecretRoom)
            {
                Vector2 pos = new Vector2(southMost.x, southMost.y);
                Vector3 rot = new Vector3(0, 0, -180);
                GameObject SSR = SpawnElement(pos, prefabSecretRooms[rand], rot);
                ClearSecretArea(southMost, Direction.South);
                southSecret = true;
                southSR = SSR.GetComponent<SecretRoom>();
            }
        }
    }

    private void ClearSecretArea(TileLocation startTile, Direction dir)
    {
        switch (dir)
        {
            case Direction.North:

                TileLocation N = new TileLocation(startTile.x - halfWidthSecretRoom, startTile.y);

                for (int x = N.x; x <= N.x + checkTotalWidthSecretRoom; x++)
                {
                    for (int y = N.y; y <= N.y + checkHeightSecretRoom; y++)
                    {
                        if (tiles[x][y] != TileType.ColliderWall)
                        {
                            tiles[x][y] = TileType.Transparent;
                        }

                    }
                }

                tiles[startTile.x][startTile.y - 1] = TileType.Floor;
                tiles[startTile.x + 1][startTile.y - 1] = TileType.Floor;

                northMain = new TileLocation(startTile.x, startTile.y-1);
                northSide = new TileLocation(startTile.x + 1, startTile.y - 1);

                break;


            case Direction.East:

                TileLocation E = new TileLocation(startTile.x, startTile.y + halfWidthSecretRoom);

                for (int x = E.x; x <= E.x + checkHeightSecretRoom; x++)
                {
                    for (int y = E.y; y >= E.y - checkTotalWidthSecretRoom; y--)
                    {
                        if (tiles[x][y] != TileType.ColliderWall)
                        {
                            tiles[x][y] = TileType.Transparent;
                        }
                    }
                }

                tiles[startTile.x - 1][startTile.y] = TileType.Floor;
                tiles[startTile.x - 1][startTile.y - 1] = TileType.Floor;

                eastMain = new TileLocation(startTile.x - 1, startTile.y);
                eastSide = new TileLocation(startTile.x - 1, startTile.y - 1);

                break;
            case Direction.South:

                TileLocation s = new TileLocation(startTile.x - halfWidthSecretRoom, startTile.y - checkHeightSecretRoom);

                for (int x = s.x; x <= s.x + checkTotalWidthSecretRoom; x++)
                {
                    for (int y = s.y; y <= s.y + checkHeightSecretRoom; y++)
                    {
                        if (tiles[x][y] != TileType.ColliderWall)
                        {
                            tiles[x][y] = TileType.Transparent;
                        }
                    }
                }

                tiles[startTile.x][startTile.y + 1] = TileType.Floor;
                tiles[startTile.x - 1][startTile.y + 1] = TileType.Floor;

                southMain = new TileLocation(startTile.x, startTile.y + 1);
                southSide = new TileLocation(startTile.x - 1, startTile.y + 1);

                break;

            default:
                break;
        }
    }

    private bool CheckSpaceForSecretRoom(TileLocation startTile, Direction dir)
    {
        if (DebugControl.debugDungeon)
        {
            Debug.Log("Scanning area for Secret Room");
        }

        switch (dir)
        {
            case Direction.North:

                Debug.Log(startTile.x + " |||| " + startTile.y);

                /// Sanity Check
                if (startTile.x - halfWidthSecretRoom > 5 && startTile.x + halfWidthSecretRoom < (columns - 5))
                {
                    if (startTile.y + checkHeightSecretRoom < (rows - 5))
                    {
                        TileLocation startCheckPosition = new TileLocation(startTile.x - halfWidthSecretRoom, startTile.y);

                        for (int x = startCheckPosition.x; x <= startCheckPosition.x + checkTotalWidthSecretRoom; x++)
                        {
                            for (int y = startCheckPosition.y; y <= startCheckPosition.y + checkHeightSecretRoom; y++)
                            {
                                if (tiles[x][y] != TileType.BlackArea)
                                {
                                    return false;
                                }
                            }
                        }

                        if (DebugControl.debugDungeon)
                        {
                            Debug.Log("There is Space for a Secret room in the North");
                        }

                        return true;

                    }
                }

                if (DebugControl.debugDungeon)
                {
                    Debug.LogError("Failed the sanity Check for secret room " + dir);
                }

                return false;

            case Direction.East:

                /// Sanity Check
                if (startTile.x + checkHeightSecretRoom < (columns - 5))
                {
                    if (startTile.y + halfWidthSecretRoom < (rows - 5) && startTile.y - halfWidthSecretRoom > 5)
                    {
                        TileLocation startCheckPosition = new TileLocation(startTile.x, startTile.y + halfWidthSecretRoom);

                        for (int x = startCheckPosition.x; x <= startCheckPosition.x + checkHeightSecretRoom; x++)
                        {
                            for (int y = startCheckPosition.y; y >= startCheckPosition.y - checkTotalWidthSecretRoom; y--)
                            {
                                if (tiles[x][y] != TileType.BlackArea)
                                {
                                    return false;
                                }
                            }
                        }

                        if (DebugControl.debugDungeon)
                        {
                            Debug.Log("There is Space for a secret room in the East");
                        }

                        return true;

                    }
                }


                if (DebugControl.debugDungeon)
                {
                    Debug.LogError("Failed the sanity Check for secret room" + dir);
                }

                return false;

            case Direction.South:

                /// Sanity Check
                if (startTile.x - halfWidthSecretRoom > 5 && startTile.x + halfWidthSecretRoom < (columns - 5))
                {
                    if (startTile.y - checkHeightSecretRoom > 5)
                    {
                        TileLocation startCheckPosition = new TileLocation(startTile.x - halfWidthSecretRoom, startTile.y - checkHeightSecretRoom);

                        for (int x = startCheckPosition.x; x <= startCheckPosition.x + checkTotalWidthSecretRoom; x++)
                        {
                            for (int y = startCheckPosition.y; y <= startCheckPosition.y + checkHeightSecretRoom; y++)
                            {
                                if (tiles[x][y] != TileType.BlackArea)
                                {
                                    return false;
                                }
                            }

                        }

                        if (DebugControl.debugDungeon)
                        {
                            Debug.Log("There is Space for a secret room in the South");
                        }

                        return true;

                    }
                }


                if (DebugControl.debugDungeon)
                {
                    Debug.LogError("Failed the sanity Check for secret room " + dir);
                }
                return false;

            default:
                break;
        }

        return true;

    }

    public void SpawnSecretWalls()
    {
        if (northSecret)
        {
            GameObject secretWallMain = SpawnElement(northMain.MyPosition, secretWalls[0], false, false, true);
            GameObject secretWallSide = SpawnElement(northSide.MyPosition, secretWalls[0], false, false, true);

            SecretWall sw = secretWallMain.GetComponent<SecretWall>();
            SecretWall sws = secretWallSide.GetComponent<SecretWall>();

            sw.myRoom = northSR;
            sws.myRoom = northSR;
        }

        if (eastSecret)
        {
            GameObject secretWallMain = SpawnElement(eastMain.MyPosition, secretWalls[0], false, false, true);
            GameObject secretWallSide = SpawnElement(eastSide.MyPosition, secretWalls[0], false, false, true);

            SecretWall sw = secretWallMain.GetComponent<SecretWall>();
            SecretWall sws = secretWallSide.GetComponent<SecretWall>();

            sw.myRoom = eastSR;
            sws.myRoom = eastSR;
        }

        if (southSecret)
        {
            GameObject secretWallMain = SpawnElement(southMain.MyPosition, secretWalls[0], false, false, true);
            GameObject secretWallSide = SpawnElement(southSide.MyPosition, secretWalls[0], false, false, true);

            SecretWall sw = secretWallMain.GetComponent<SecretWall>();
            SecretWall sws = secretWallSide.GetComponent<SecretWall>();

            sw.myRoom = southSR;
            sws.myRoom = southSR;
        }
    }


    #region unusedMethods


    //private void RemoveAllEnemiesNearStartPoint()
    //{
    //    /// make a layerMask
    //    int layerId = 11;
    //    int enemyMask = 1 << layerId;

    //    Collider2D[] killzone = Physics2D.OverlapCircleAll(player.transform.position, 10, enemyMask);

    //    if (DebugControl.debugEnemies)
    //    {
    //        foreach (var enemy in killzone)
    //        {
    //            if (DebugControl.debugEnemies)
    //            {
    //                Debug.Log("Im a: " + enemy.transform.parent.gameObject.name);
    //            }

    //            DungeonManager.instance.enemiesInDungeon.Remove(enemy.gameObject);
    //            Destroy(enemy.transform.parent.gameObject);
    //        }
    //    }
    //}

    #endregion unusedMethods

}

[Serializable]
public class TileLocation
{
    public int x;
    public int y;

    /// <summary>
    /// Default constructor
    /// </summary>
    public TileLocation()
    {

    }

    /// <summary>
    /// Paramaterized constructor where each part of the 
    /// vector is defined individually
    /// </summary>
    /// <param name="xStart"></param>
    /// <param name="yStart"></param>
    public TileLocation(int xStart, int yStart)
    {
        x = xStart;
        y = yStart;
    }

    /// <summary>
    /// Parameterized constructor which takes a vector
    /// </summary>
    /// <param name="pos"></param>
    public TileLocation(Vector2 pos)
    {
        x = (int)pos.x;
        y = (int)pos.y;
    }

    public Vector2 MyPosition
    {
        get
        {
            return new Vector2(x, y);
        }
    }
}