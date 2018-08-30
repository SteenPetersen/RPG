using System;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int xPos;                      // The x coordinate of the lower left tile of the room.
    public int yPos;                      // The y coordinate of the lower left tile of the room.
    public int roomWidth;                     // How many tiles wide the room is.
    public int roomHeight;                    // How many tiles high the room is.
    public Direction enteringCorridor;    // The direction of the corridor that is entering this room.
    public int enemyAmount;
    public int destructablesAmount;
    public bool chestSpawned;
    public int chestQuality;

    /// <summary>
    /// Marks the middle of this room for distance measureing
    /// </summary>
    public TileLocation middleTile;

    public List<TileLocation> middleFloorTilesInThisRoom = new List<TileLocation>();

    public List<TileLocation> edgeFloorTilesOfThisRoom = new List<TileLocation>();

    public bool roundedBossRoom; // testing round rooms

    /// To decreases likelihood of going over the board boudaries 
    /// and thus resulting in an indexOutOfRangeException
    public int buffer = 3;

    /// <summary>
    ///  Setup the first Room of the Dungeon
    /// </summary>
    /// <param name="widthRange">The range between which the width of the room is determined</param>
    /// <param name="heightRange">The range between which the height of the room is determined</param>
    /// <param name="columns">The amount of Columns in this Dungeon board</param>
    /// <param name="rows">The amount of Rows in this Dungeon board</param>
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows)
    {
        BoardCreator board = BoardCreator.instance;

        // Set a random width and height.
        int width = widthRange.Random;
        int height = heightRange.Random;
        roomWidth = 9;
        roomHeight = 9;

        // Set the x and y coordinates so the room is roughly in the middle of the board.
        xPos = 10;
        yPos = Mathf.RoundToInt((rows / 2f) - (roomHeight / 2f));

        Vector3 playerPos = new Vector3(xPos + (roomWidth / 2), yPos + (roomHeight / 2), 0);

        /// Place the Player inside the first room
        board.player.transform.position = playerPos;

        Vector3[] corners = new[] { new Vector3(xPos, yPos, 0f), new Vector3(xPos + roomWidth - 1, yPos, 0f), new Vector3(xPos, yPos + roomHeight - 1, 0f), new Vector3(xPos + roomWidth - 1, yPos + roomHeight - 1, 0f) };

        middleTile = new TileLocation((roomWidth / 2) + xPos, (roomHeight / 2) + yPos);

        foreach (var corner in corners)
        {
            board.SpawnElement(corner, board.entranceTorch);
        }

        middleTile = new TileLocation((roomWidth / 2) + xPos, (roomHeight / 2) + yPos);
    }

    /// <summary>
    /// This is an overload of the SetupRoom function and has a corridor parameter that represents the corridor entering the room.
    /// This is Used for all rooms except the first and last room
    /// </summary>
    /// <param name="widthRange">The range between which the width of the room is determined</param>
    /// <param name="heightRange">The range between which the height of the room is determined</param>
    /// <param name="columns">The amount of Columns in this Dungeon board</param>
    /// <param name="rows">The amount of Rows in this Dungeon board</param>
    /// <param name="corridor">Corridor leading to this room</param>
    /// <param name="enemies">The range between which the amount of enemies in this room is determined</param>
    /// <param name="destructables">The range between which the amount of destructables in this room is determined</param>
    /// <param name="middle">Optional boolean in case we wish to do something to the middle room</param>
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor, IntRange enemies, IntRange destructables, bool middle = false)
    {
        BoardCreator board = BoardCreator.instance;

        // Set the entering corridor direction.
        enteringCorridor = corridor.direction;

        // Set random values for width and height.
        roomWidth = widthRange.Random;
        roomHeight = heightRange.Random;


        //TODO add monster chance to drop
        enemyAmount = enemies.Random;
        destructablesAmount = destructables.Random;

        switch (corridor.direction)
        {
            // If the corridor entering this room is going north...
            case Direction.North:
                // ... the height of the room mustn't go beyond the board so it must be clamped based
                // on the height of the board (rows) and the end of corridor that leads to the room.
                roomHeight = Mathf.Clamp(roomHeight, 0, (rows - corridor.EndPositionY) - buffer);

                // The y coordinate of the room must be at the end of the corridor (since the corridor leads to the bottom of the room).
                yPos = corridor.EndPositionY;

                // The x coordinate can be random but the left-most possibility is no further than the width
                // and the right-most possibility is that the end of the corridor is at the position of the room.
                xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);

                // This must be clamped to ensure that the room doesn't go off the board.
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);

                break;
            case Direction.East:
                roomWidth = Mathf.Clamp(roomWidth, 0, (columns - corridor.EndPositionX) - buffer);
                xPos = corridor.EndPositionX;

                yPos = UnityEngine.Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, buffer, (rows - board.roomHeight.m_Max) - buffer);

                break;
            case Direction.South:
                // clamp the height of the room between 1 and the (int value given from EndPosition y (e.g 8)) and then subtract the buffer
                roomHeight = Mathf.Clamp(roomHeight, 0, corridor.EndPositionY - buffer);
                yPos = corridor.EndPositionY - roomHeight + 1;

                xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);

                break;
            case Direction.West:
                roomWidth = Mathf.Clamp(roomWidth, 0, corridor.EndPositionX - buffer);
                xPos = corridor.EndPositionX - roomWidth + 1;

                yPos = UnityEngine.Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, buffer, (rows - BoardCreator.instance.roomHeight.m_Max) - buffer);

                break;
        }

        middleTile = new TileLocation((roomWidth / 2) + xPos, (roomHeight / 2) + yPos);

        if (middle)
        {
            SetMapLocation();
        }

        ChestDropCheck();
    }

    /// <summary>
    /// Setup the Last room of the Dungeon
    /// </summary>
    /// <param name="widthRange">The range between which the width of the room is determined</param>
    /// <param name="heightRange">The range between which the height of the room is determined</param>
    /// <param name="columns">The amount of Columns in this Dungeon board</param>
    /// <param name="rows">The amount of Rows in this Dungeon board</param>
    /// <param name="corridor">Corridor leading to this room</param>
    /// <param name="enemies">The range between which the amount of enemies in this room is determined</param>
    /// <param name="bossRoom">Boolean that determines if this is a boss room or not</param>
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor, IntRange enemies, bool bossRoom)
    {
        // Set the entering corridor direction.
        enteringCorridor = corridor.direction;

        // Set random values for width and height.
        int width = widthRange.Random;
        int height = heightRange.Random;
        roomWidth = Mathf.Clamp(width, 2, width * 2);
        roomHeight = Mathf.Clamp(height, 2, height * 2);

        switch (corridor.direction)
        {
            // If the corridor entering this room is going north...
            case Direction.North:

                if (bossRoom)
                {
                    roundedBossRoom = true;

                    xPos = corridor.EndPositionX - 12;
                    yPos = corridor.EndPositionY;

                    roomWidth = 25;
                    roomHeight = roomWidth;
                    break;
                }

                roomHeight = Mathf.Clamp(roomHeight, 0, (rows - corridor.EndPositionY) - buffer);
                yPos = corridor.EndPositionY;
                xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);
                xPos = Mathf.Clamp(xPos, 0, (columns - roomWidth) - buffer);
                break;


            case Direction.East:

                if (bossRoom)
                {
                    roundedBossRoom = true;

                    xPos = corridor.EndPositionX;
                    yPos = corridor.EndPositionY - 12;

                    roomWidth = 25;
                    roomHeight = roomWidth;
                    break;
                }

                roomWidth = Mathf.Clamp(roomWidth, 1, (columns - corridor.EndPositionX) - buffer);
                xPos = corridor.EndPositionX;

                yPos = UnityEngine.Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, 0, (rows - roomHeight) - buffer);
                break;



            case Direction.South:

                if (bossRoom)
                {
                    roundedBossRoom = true;

                    xPos = corridor.EndPositionX - 12;
                    yPos = corridor.EndPositionY - 25;

                    roomWidth = 25;
                    roomHeight = roomWidth;
                    break;
                }


                roomHeight = Mathf.Clamp(roomHeight, 1, corridor.EndPositionY - buffer);
                yPos = corridor.EndPositionY - roomHeight + 1;

                xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);
                break;


            case Direction.West:


                if (bossRoom)
                {
                    roundedBossRoom = true;

                    xPos = corridor.EndPositionX - 25;
                    yPos = corridor.EndPositionY - 12;

                    roomWidth = 25;
                    roomHeight = roomWidth;
                    break;
                }

                roomWidth = Mathf.Clamp(roomWidth, 1, corridor.EndPositionX - buffer);
                xPos = corridor.EndPositionX - roomWidth + 1;

                yPos = UnityEngine.Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, buffer, (rows - roomHeight) - buffer);
                break;

        }

        middleTile = new TileLocation((roomWidth / 2) + xPos, (roomHeight / 2) + yPos);

        //SetGoalLocation();
    }

    /// <summary>
    /// Sets up the secret room according to the corridor that was made succesfully
    /// </summary>
    /// <param name="widthRange">The range between which the width of the room is determined</param>
    /// <param name="heightRange">The range between which the height of the room is determined</param>
    /// <param name="columns">The amount of Columns in this Dungeon board</param>
    /// <param name="rows">The amount of Rows in this Dungeon board</param>
    /// <param name="corridor">The Secret Corridor that Leads to this room</param>
    public void SetupSecretRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor)
    {
        BoardCreator board = BoardCreator.instance;

        enteringCorridor = corridor.direction;

        roomWidth = widthRange.Random;
        roomHeight = heightRange.Random;

        switch (corridor.direction)
        {
            case Direction.North:
                // ... the height of the room mustn't go beyond the board so it must be clamped based
                // on the height of the board (rows) and the end of corridor that leads to the room.
                roomHeight = Mathf.Clamp(roomHeight, 0, (rows - corridor.EndPositionY) - buffer);

                // The y coordinate of the room must be at the end of the corridor (since the corridor leads to the bottom of the room).
                yPos = corridor.EndPositionY;

                // The x coordinate can be random but the left-most possibility is no further than the width
                // and the right-most possibility is that the end of the corridor is at the position of the room.
                xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 2, corridor.EndPositionX);

                // This must be clamped to ensure that the room doesn't go off the board.
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);
                break;

            case Direction.East:
                break;

            case Direction.South:

                roomHeight = Mathf.Clamp(roomHeight, 0, corridor.EndPositionY - buffer);
                yPos = corridor.EndPositionY - roomHeight + 1;

                xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 2, corridor.EndPositionX);
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);

                break;

            case Direction.West:

                break;

            default:
                break;
        }

        Vector3[] corners = new[] { new Vector3(xPos, yPos, 0f),
            new Vector3(xPos + roomWidth - 1, yPos, 0f),
            new Vector3(xPos, yPos + roomHeight - 1, 0f),
            new Vector3(xPos + roomWidth - 1, yPos + roomHeight - 1, 0f) };

        foreach (var corner in corners)
        {
            BoardCreator.instance.SpawnElement(corner, BoardCreator.instance.entranceTorch);
        }

        middleTile = new TileLocation((roomWidth / 2) + xPos, (roomHeight / 2) + yPos);
    }

    /// <summary>
    /// Once the Farthest room has been determined call this Method in order to place
    /// The goal randomly inside that room
    /// </summary>
    public void SetGoalLocation()
    {
        BoardCreator board = BoardCreator.instance;

        int goalX = UnityEngine.Random.Range(xPos, xPos + roomWidth - 1);
        int goalY = UnityEngine.Random.Range(yPos, yPos + roomHeight - 1);

        //Debug.Log("Goal is Placed at position X " + goalX + " and Y " + goalY);

        Vector2 pos = new Vector2(goalX, goalY);

        board.SpawnElement(pos, board.goal);

        TileLocation goalFloorLocation = new TileLocation(goalX, goalY);

        board.SpawnGoalFloor(goalFloorLocation);
    }


    /// <summary>
    /// Checks if a chest is spawned in this room
    /// </summary>
    private void ChestDropCheck()
    {
        float chestRand = UnityEngine.Random.Range(0, 100);

        if (chestRand <= BoardCreator.instance.chanceOfChestPerRoom)
        {
            chestSpawned = true;

            if (chestRand < BoardCreator.instance.chanceOfBetterChestPerRoom)
            {
                chestQuality = 1;
                return;
            }

            chestQuality = 0;
        }
    }

    private void SetMapLocation()
    {
        //int mapX = UnityEngine.Random.Range(xPos, xPos + (roomWidth / 2));
        //int mapY = UnityEngine.Random.Range(yPos, yPos + (roomHeight / 2));

        //Debug.Log("Map is Placed at position X " + mapX + " and Y " + mapY);

        //Vector2 pos = new Vector2(mapX, mapY);

        //BoardCreator.instance.SpawnElement(pos, BoardCreator.instance.map);
    }

    //switch (corridor.direction)
    //    {
    //        // If the corridor entering this room is going north...
    //        case Direction.North:
    //            // ... the height of the room mustn't go beyond the board so it must be clamped based
    //            // on the height of the board (rows) and the end of corridor that leads to the room.
    //            roomHeight = Mathf.Clamp(roomHeight, 1, (rows - corridor.EndPositionY) - buffer);

    //            // The y coordinate of the room must be at the end of the corridor (since the corridor leads to the bottom of the room).
    //            yPos = corridor.EndPositionY;

    //            // The x coordinate can be random but the left-most possibility is no further than the width
    //            // and the right-most possibility is that the end of the corridor is at the position of the room.
    //            xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);

    //            // This must be clamped to ensure that the room doesn't go off the board.
    //            xPos = Mathf.Clamp(xPos, 0, (columns - roomWidth) - buffer);
    //            break;
    //        case Direction.East:
    //            roomWidth = Mathf.Clamp(roomWidth, 1, (columns - corridor.EndPositionX) - buffer);
    //            xPos = corridor.EndPositionX;

    //            yPos = UnityEngine.Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
    //            yPos = Mathf.Clamp(yPos, 0, (rows - roomHeight) - buffer);
    //            break;
    //        case Direction.South:
    //            // clamp the height of the room between 1 and the (int value given from EndPosition why (e.g 8)) and then subtract the buffer
    //            roomHeight = Mathf.Clamp(roomHeight, 1, corridor.EndPositionY - buffer);
    //            yPos = corridor.EndPositionY - roomHeight + 1;

    //            xPos = UnityEngine.Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);
    //            xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);
    //            break;
    //        case Direction.West:
    //            roomWidth = Mathf.Clamp(roomWidth, 1, corridor.EndPositionX - buffer);
    //            xPos = corridor.EndPositionX - roomWidth + 1;

    //            yPos = UnityEngine.Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
    //            yPos = Mathf.Clamp(yPos, buffer, (rows - roomHeight) - buffer);
    //            break;
    //    }
}
