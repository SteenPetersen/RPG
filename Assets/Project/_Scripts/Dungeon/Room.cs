﻿using System;
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

    public List<TileLocation> middleFloorTilesInThisRoom = new List<TileLocation>();
    public List<TileLocation> edgeFloorTilesOfThisRoom = new List<TileLocation>();


    public bool round; // testing round rooms

    // must be above 5 or the building of walla on the outside of rooms can overreach the tile borders 
    // and result in a null reference
    public int buffer = 1;

    // This is used for the first room.  It does not have a Corridor parameter since there are no corridors yet.
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows)
    {
        // Set a random width and height.
        int width = widthRange.Random;
        int height = heightRange.Random;
        roomWidth = 9;
        roomHeight = 9;

        // Set the x and y coordinates so the room is roughly in the middle of the board.
        xPos = 10;
        yPos = Mathf.RoundToInt((rows / 2f) - (roomHeight / 2f));



        Vector3 playerPos = new Vector3(xPos + (roomWidth / 2), yPos + (roomHeight / 2), 0);
        BoardCreator.instance.player.transform.position = playerPos;

        Vector3[] corners = new[] { new Vector3(xPos, yPos, 0f), new Vector3(xPos + roomWidth - 1, yPos, 0f), new Vector3(xPos, yPos + roomHeight - 1, 0f), new Vector3(xPos + roomWidth - 1, yPos + roomHeight - 1, 0f) };

        foreach (var corner in corners)
        {
            BoardCreator.instance.SpawnElement(corner, BoardCreator.instance.entranceTorch);
        }
    }

    // This is an overload of the SetupRoom function and has a corridor parameter that represents the corridor entering the room.
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor, IntRange enemies, IntRange destructables, bool middle = false)
    {
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
                yPos = Mathf.Clamp(yPos, buffer, (rows - BoardCreator.instance.roomHeight.m_Max) - buffer);
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

        if (middle)
        {
            SetMapLocation();
        }

        ChestDropCheck();
    }



    // This is a further overload only for the last room
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor, IntRange enemies, int multiplier, bool bossRoom)
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
                    round = true;

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
                    round = true;

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
                    round = true;

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
                    round = true;

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

        SetGoalLocation();
    }

    private void SetGoalLocation()
    {
        int goalX = UnityEngine.Random.Range(xPos, xPos + roomWidth - 1);
        int goalY = UnityEngine.Random.Range(yPos, yPos + roomHeight - 1);

        //Debug.Log("Goal is Placed at position X " + goalX + " and Y " + goalY);

        Vector2 pos = new Vector2(goalX, goalY);

        BoardCreator.instance.SpawnElement(pos, BoardCreator.instance.goal);
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
