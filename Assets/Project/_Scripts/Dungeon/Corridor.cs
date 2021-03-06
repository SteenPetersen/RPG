﻿using UnityEngine;
using System.Collections.Generic;

// Enum to specify the direction is heading.
public enum Direction
{
    North, East, South, West,
}

public class Corridor
{
    public int startXPos;         // The x coordinate for the start of the corridor.
    public int startYPos;         // The y coordinate for the start of the corridor.
    public int corridorLength;            // How many units long the corridor is.

    public Direction direction;   // Which direction the corridor is heading from it's room.

    public List<TileLocation> tilesInCorridor = new List<TileLocation>();
    public List<TileLocation> secretFloorTiles = new List<TileLocation>();


    public TileLocation secretRoomStartPosition;

    /// <summary>
    /// Used to know which secret Covers are related to which secret doors
    /// </summary>
    public Room mySecretRoom;

    /// Get the end position of the corridor based on it's 
    /// start position and which direction it's heading.
    public int EndPositionX
    {
        get
        {
            if (direction == Direction.North || direction == Direction.South)
                return startXPos;
            if (direction == Direction.East)
                return startXPos + corridorLength - 1;
            return startXPos - corridorLength + 1;
        }
    }


    public int EndPositionY
    {
        get
        {
            if (direction == Direction.East || direction == Direction.West)
                return startYPos;
            if (direction == Direction.North)
                return startYPos + corridorLength - 1;
            return startYPos - corridorLength + 1;
        }
    }


    public void SetupCorridor(Room room, IntRange length, IntRange roomWidth, IntRange roomHeight, int columns, int rows, bool firstCorridor)
    {
        // Set a random direction (a random index from 0 to 3, cast to Direction).
        direction = (Direction)Random.Range(0, 4);

        // Find the direction opposite to the one entering the room this corridor is leaving from.
        // Cast the previous corridor's direction to an int between 0 and 3 and add 2 (a number between 2 and 5).
        // Find the remainder when dividing by 4 (if 2 then 2, if 3 then 3, if 4 then 0, if 5 then 1).
        // Cast this number back to a direction.
        // Overall effect is if the direction was South then that is 2, becomes 4, remainder is 0, which is north.
        Direction oppositeDirection = (Direction)(((int)room.enteringCorridor + 2) % 4);

        // If this is not the first corridor and the randomly selected direction is opposite to the previous corridor's direction...
        if (!firstCorridor && direction == oppositeDirection)
        {
            // Rotate the direction 90 degrees clockwise (North becomes East, East becomes South, etc).
            // This is a more broken down version of the opposite direction operation above but instead of adding 2 we're adding 1.
            // This means instead of rotating 180 (the opposite direction) we're rotating 90.
            int directionInt = (int)direction;
            directionInt++;
            directionInt = directionInt % 4;
            direction = (Direction)directionInt;

        }

        // Set a random length.
        corridorLength = length.Random;

        // Create a cap for how long the length can be (this will be changed based on the direction and position).
        int maxLength = length.m_Max;

        if (firstCorridor)
        {
            corridorLength = corridorLength * 2;
            direction = Direction.East;

            startXPos = room.xPos + room.roomWidth;
            startYPos = Random.Range(room.yPos + 1, room.yPos + room.roomHeight - 1);
            maxLength = (columns - startXPos - roomWidth.m_Min) - 2;
        }

        if (!firstCorridor)
        {
            switch (direction)
            {
                // If the choosen direction is North (up)...
                case Direction.North:
                    // ... the starting position in the x axis can be random but within the width of the room.
                    startXPos = Random.Range(room.xPos, room.xPos + room.roomWidth - 2);

                    // The starting position in the y axis must be the top of the room.
                    startYPos = room.yPos + room.roomHeight;

                    // The maximum length the corridor can be is the height of the board (rows) but from the top of the room (y pos + height).
                    maxLength = (rows - startYPos - roomHeight.m_Max) - 2;
                    break;

                case Direction.East:

                    startXPos = room.xPos + room.roomWidth;
                    startYPos = Random.Range(room.yPos + 1, room.yPos + room.roomHeight - 1);
                    maxLength = (columns - startXPos - roomWidth.m_Max) - 2;
                    break;

                case Direction.South:
                    startXPos = Random.Range(room.xPos, room.xPos + room.roomWidth - 2);
                    startYPos = room.yPos;
                    maxLength = (startYPos - roomHeight.m_Max) - 2;
                    break;

                case Direction.West:

                    startXPos = room.xPos;
                    startYPos = Random.Range(room.yPos + 1, room.yPos + room.roomHeight - 1);
                    maxLength = (startXPos - roomWidth.m_Max) - 2;
                    break;
            }
        }

        // We clamp the length of the corridor to make sure it doesn't go off the board.
        corridorLength = Mathf.Clamp(corridorLength, 1, maxLength);
    }

    public bool SetupCorridor(Room room, Direction dir)
    {
        direction = dir;

        switch (dir)
        {
            case Direction.North:

                /// Is there space for a secret room?
                bool north = CheckSpaceForSecretRoom(room, dir);

                if (north)
                {
                    if (DebugControl.debugDungeon)
                    {
                        Debug.Log("There is space for a secret room in the North");
                    }

                    /// Make the corridor
                    startXPos = secretRoomStartPosition.x;
                    startYPos = secretRoomStartPosition.y;
                    corridorLength = 2;

                    return true;

                }

                return false;


            case Direction.East:

                break;
            case Direction.South:

                /// Is there space for a secret room?
                bool south = CheckSpaceForSecretRoom(room, dir);

                if (south)
                {
                    if (DebugControl.debugDungeon)
                    {
                        Debug.Log("There is space for a secret room in the South");
                    }

                    /// Make the corridor
                    startXPos = secretRoomStartPosition.x;
                    startYPos = secretRoomStartPosition.y;
                    corridorLength = 2; //TODO boardcreator can determine how long these are with a varaible later

                    return true;

                }

                return false;

            case Direction.West:

                break;

            default:
                return false;
        }

        return false;
    }

    private bool CheckSpaceForSecretRoom(Room myRoom, Direction dir)
    {
        BoardCreator board = BoardCreator.instance;

        if (DebugControl.debugDungeon)
        {
            Debug.LogWarning("Entering CheckSpaceForSecretRoom ");
        }

        int westMostPoint = 5;
        int eastMostPoint = board.columns - 5;
        int northmostPoint = board.rows - 5;
        int southmostPoint = 5;
        int countX = 0;
        int countY = 0;

        switch (dir)
        {
            case Direction.North:

                secretRoomStartPosition = new TileLocation(myRoom.middleTile.x, myRoom.yPos + myRoom.roomHeight);

                /// Sanity Check
                if (secretRoomStartPosition.x > westMostPoint && secretRoomStartPosition.x + myRoom.roomWidth < eastMostPoint)
                {
                    if (secretRoomStartPosition.y > southmostPoint && secretRoomStartPosition.y + myRoom.roomHeight < northmostPoint)
                    {
                        for (int x = secretRoomStartPosition.x - 4; x <= secretRoomStartPosition.x + myRoom.roomWidth + 4; x += 2)
                        {
                            int xCoord = myRoom.xPos + countX;

                            for (int y = secretRoomStartPosition.y; y <= myRoom.roomWidth; y += 2)
                            {
                                if (DebugControl.debugDungeon)
                                {
                                    Debug.LogWarning("Checking Tile " + xCoord + " " + (myRoom.yPos + y));
                                }

                                int yCoord = myRoom.yPos + countY;

                                if (board.tiles[xCoord][yCoord] != BoardCreator.TileType.BlackArea)
                                {
                                    return false;
                                }
                                countY++;
                            }

                            countX++;
                            countY = 0;
                        }

                        return true;
                    }
                }

                return false;


            case Direction.East:
                break;
            case Direction.South:

                secretRoomStartPosition = new TileLocation(myRoom.middleTile.x, myRoom.yPos - 1);

                /// Sanity Check
                if (secretRoomStartPosition.x > westMostPoint && secretRoomStartPosition.x + myRoom.roomWidth < eastMostPoint)
                {
                    if (secretRoomStartPosition.y > southmostPoint && secretRoomStartPosition.y + myRoom.roomHeight < northmostPoint)
                    {
                        for (int x = secretRoomStartPosition.x - 4; x <= secretRoomStartPosition.x + myRoom.roomWidth + 4; x += 2)
                        {
                            int xCoord = myRoom.xPos + countX;

                            for (int y = secretRoomStartPosition.y; y <= 20; y += 2)
                            {
                                if (DebugControl.debugDungeon)
                                {
                                    Debug.LogWarning("Checking Tile " + xCoord + " " + (myRoom.yPos + y));
                                }

                                int yCoord = myRoom.yPos + countY;

                                if (board.tiles[xCoord][yCoord] != BoardCreator.TileType.BlackArea)
                                {
                                    return false;
                                }
                                countY++;
                            }

                            countX++;
                            countY = 0;
                        }

                        return true;
                    }
                }

                return false;

            case Direction.West:
                break;
            default:
                break;
        }

        return false;
    }


}
