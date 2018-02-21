﻿using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int xPos;                      // The x coordinate of the lower left tile of the room.
    public int yPos;                      // The y coordinate of the lower left tile of the room.
    public int roomWidth;                     // How many tiles wide the room is.
    public int roomHeight;                    // How many tiles high the room is.
    public Direction enteringCorridor;    // The direction of the corridor that is entering this room.
    public int enemyAmount;

    public List<Vector2> enemyPositions;

    int buffer = 5;

    // This is used for the first room.  It does not have a Corridor parameter since there are no corridors yet.
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows)
    {
        // Set a random width and height.
        int width = widthRange.Random;
        int height = heightRange.Random;
        roomWidth = Mathf.Clamp(width, 5, width);
        roomHeight = Mathf.Clamp(height, 5, height);

        // Set the x and y coordinates so the room is roughly in the middle of the board.
        xPos = Mathf.RoundToInt((columns / 2f) - (roomWidth / 2f));
        yPos = Mathf.RoundToInt((rows / 2f) - (roomHeight / 2f));

        Debug.Log(xPos + " " + yPos);

        Vector3 playerPos = new Vector3(xPos + 2, yPos + 2, 0);
        BoardCreator.instance.player.transform.position = playerPos;
    }


    // This is an overload of the SetupRoom function and has a corridor parameter that represents the corridor entering the room.
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor, IntRange enemies)
    {
        // Set the entering corridor direction.
        enteringCorridor = corridor.direction;

        // Set random values for width and height.
        roomWidth = widthRange.Random;
        roomHeight = heightRange.Random;

        //TODO add monster chance to drop
        enemyAmount = enemies.Random;

        switch (corridor.direction)
        {
            // If the corridor entering this room is going north...
            case Direction.North:
                // ... the height of the room mustn't go beyond the board so it must be clamped based
                // on the height of the board (rows) and the end of corridor that leads to the room.
                roomHeight = Mathf.Clamp(roomHeight, 1, (rows - corridor.EndPositionY) - buffer);

                // The y coordinate of the room must be at the end of the corridor (since the corridor leads to the bottom of the room).
                yPos = corridor.EndPositionY;

                // The x coordinate can be random but the left-most possibility is no further than the width
                // and the right-most possibility is that the end of the corridor is at the position of the room.
                xPos = Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);

                // This must be clamped to ensure that the room doesn't go off the board.
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);
                break;
            case Direction.East:
                roomWidth = Mathf.Clamp(roomWidth, 1, (columns - corridor.EndPositionX) - buffer);
                xPos = corridor.EndPositionX;

                yPos = Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, buffer, (rows - roomHeight) - buffer);
                break;
            case Direction.South:
                // clamp the height of the room between 1 and the (int value given from EndPosition why (e.g 8)) and then subtract the buffer
                roomHeight = Mathf.Clamp(roomHeight, 1, corridor.EndPositionY - buffer);
                yPos = corridor.EndPositionY - roomHeight + 1;

                xPos = Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);
                break;
            case Direction.West:
                roomWidth = Mathf.Clamp(roomWidth, 1, corridor.EndPositionX - buffer);
                xPos = corridor.EndPositionX - roomWidth + 1;

                yPos = Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, buffer, (rows - roomHeight) - buffer);
                break;
        }

        CheckForEnemies();

    }


    // This is a further overload only for the last room
    public void SetupRoom(IntRange widthRange, IntRange heightRange, int columns, int rows, Corridor corridor, IntRange enemies, int multiplier)
    {
        // Set the entering corridor direction.
        enteringCorridor = corridor.direction;

        // Set random values for width and height.
        int width = widthRange.Random;
        int height = heightRange.Random;
        roomWidth = Mathf.Clamp(width, 2, width * 2);
        roomHeight = Mathf.Clamp(height, 2, height * 2);


        //TODO add monster chance to drop
        enemyAmount = enemies.Random;

        switch (corridor.direction)
        {
            // If the corridor entering this room is going north...
            case Direction.North:
                // ... the height of the room mustn't go beyond the board so it must be clamped based
                // on the height of the board (rows) and the end of corridor that leads to the room.
                roomHeight = Mathf.Clamp(roomHeight, 1, (rows - corridor.EndPositionY) - buffer);

                // The y coordinate of the room must be at the end of the corridor (since the corridor leads to the bottom of the room).
                yPos = corridor.EndPositionY;

                // The x coordinate can be random but the left-most possibility is no further than the width
                // and the right-most possibility is that the end of the corridor is at the position of the room.
                xPos = Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);

                // This must be clamped to ensure that the room doesn't go off the board.
                xPos = Mathf.Clamp(xPos, 0, (columns - roomWidth) - buffer);
                break;
            case Direction.East:
                roomWidth = Mathf.Clamp(roomWidth, 1, (columns - corridor.EndPositionX) - buffer);
                xPos = corridor.EndPositionX;

                yPos = Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, 0, (rows - roomHeight) - buffer);
                break;
            case Direction.South:
                // clamp the height of the room between 1 and the (int value given from EndPosition why (e.g 8)) and then subtract the buffer
                roomHeight = Mathf.Clamp(roomHeight, 1, corridor.EndPositionY - buffer);
                yPos = corridor.EndPositionY - roomHeight + 1;

                xPos = Random.Range(corridor.EndPositionX - roomWidth + 1, corridor.EndPositionX);
                xPos = Mathf.Clamp(xPos, buffer, (columns - roomWidth) - buffer);
                break;
            case Direction.West:
                roomWidth = Mathf.Clamp(roomWidth, 1, corridor.EndPositionX - buffer);
                xPos = corridor.EndPositionX - roomWidth + 1;

                yPos = Random.Range(corridor.EndPositionY - roomHeight + 1, corridor.EndPositionY);
                yPos = Mathf.Clamp(yPos, buffer, (rows - roomHeight) - buffer);
                break;
        }

        CheckForEnemies();
        SetGoalLocation();
    }

    private void CheckForEnemies()
    {
        enemyPositions = new List<Vector2>();

        for (int i = 0; i < enemyAmount; i++)
        {
            int enemyXPos = Random.Range(xPos, xPos + roomWidth - 1);
            int enemyYPos = Random.Range(yPos, yPos + roomHeight - 1);

            Vector2 enemyVector = new Vector2(enemyXPos, enemyYPos);

            enemyPositions.Add(enemyVector);
            BoardCreator.instance.SpawnElement(enemyVector, BoardCreator.instance.enemy);
        }
    }

    private void SetGoalLocation()
    {
        int goalX = Random.Range(xPos, xPos + roomWidth - 1);
        int goalY = Random.Range(yPos, yPos + roomHeight - 1);

        Debug.Log(goalX + " " + goalY);

        Vector2 pos = new Vector2(goalX, goalY);

        BoardCreator.instance.SpawnElement(pos, BoardCreator.instance.goal);
    }
}