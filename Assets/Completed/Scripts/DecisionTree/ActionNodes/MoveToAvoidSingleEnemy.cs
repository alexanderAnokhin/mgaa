using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveToAvoidSingleEnemy : ActionTreeNode
{
    private MoveToExit moveToExitNode = new MoveToExit();

    override public ActionTreeNode makeDecision(int sightRng)
    {
        return this;
    }

    override public void doAction(int sightRng, out int xDir2, out int yDir2)
    {
        Vector2 playerPosition = Utils.GetPlayerPosition();
        GameObject[,] gamestate = Utils.GetMapWithFloor();
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;
        int enemyX = -1;
        int enemyY = -1;
        int targetX = -1;
        int targetY = -1;
        int closest = Utils.SIZE_X + Utils.SIZE_Y;
        int closestIndex = -1;
        List<int> wallsX = new List<int>();
        List<int> wallsY = new List<int>();
        int[] resultNode;
        bool targetFound = false;

        //***Search for the enemy

        //For x view area
        for (int i = (x - sightRng); i <= (x + sightRng); i++)
        {
            //Still in map range
            if (i >= 0 && i <= (Utils.SIZE_X - 1))
            {
                //For y view area in front of the player
                for (int j = (y - sightRng); j <= (y + sightRng); j++)
                {
                    //Still in map range
                    if (j >= 0 && j <= (Utils.SIZE_Y - 1))
                    {
                        //If the checked area holds an enemy
                        if (gamestate[i, j].tag == Utils.ENEMY_TAG)
                        {
                            enemyX = i;
                            enemyY = j;
                            //Break loop
                            i = x + sightRng + 1;
                            j = y + sightRng + 1;
                        }
                    }
                }
            }
        }

        //***Search for a wall on one side of the enemy, from the players view of direction

        //The distance on the y-axis to the enemy is shorter than the distance on x-axis.
        //I.e. the enemy is on y axis to the player
        if (y < enemyY && (y - enemyY) < (x - enemyX))
        {
            //Search for a wall on x axis next to the enemy
            for (int i = (x - sightRng); i <= (x + sightRng); i++)
            {
                //Still in map range
                if (i >= 0 && i <= (Utils.SIZE_X - 1))
                {
                    if (gamestate[i, enemyY].tag == Utils.WALL_TAG)
                    {
                        wallsX.Add(i);
                        wallsY.Add(enemyY);
                    }
                }
            }
        }

        //The distance on the x-axis to the enemy is shorter than the distance on y-axis.
        //I.e. the enemy is on x axis to the player
        else if (x < enemyX && (x - enemyX) < (y - enemyY))
        {
            //Search for a wall on y axis next to the enemy
            for (int j = (y - sightRng); j <= (y + sightRng); j++)
            {
                //Still in map range
                if (j >= 0 && j <= (Utils.SIZE_Y - 1))
                {
                    if (gamestate[enemyX, j].tag == Utils.WALL_TAG)
                    {
                        wallsX.Add(enemyX);
                        wallsY.Add(j);
                    }
                }
            }
        }

        //The distances on the x- and y-axis to the enemy are equal
        else {
            //Check both directions

            //Search for a wall on x axis next to the enemy
            for (int i = (x - sightRng); i <= (x + sightRng); i++)
            {
                //Still in map range
                if (i >= 0 && i <= (Utils.SIZE_X - 1))
                {
                    if (gamestate[i, enemyY].tag == Utils.WALL_TAG)
                    {
                        wallsX.Add(i);
                        wallsY.Add(enemyY);
                    }
                }
            }
            //Search for a wall on y axis next to the enemy
            for (int j = (y - sightRng); j <= (y + sightRng); j++)
            {
                //Still in map range
                if (j >= 0 && j <= (Utils.SIZE_Y - 1))
                {
                    if (gamestate[enemyX, j].tag == Utils.WALL_TAG)
                    {
                        wallsX.Add(enemyX);
                        wallsY.Add(j);
                    }
                }
            }
        }

        //**Search for the nearest wall to the enemy

        //Wall next to enemy found
        if (wallsX.Count > 0)
        {
            //Look for the closest wall
            for (int i = 0; i < wallsX.Count; i++)
            {
                //Closer than closest so far
                if ((Mathf.Abs(wallsX[i] - enemyX) + Mathf.Abs(wallsY[i] - enemyY)) < closest)
                {
                    closestIndex = i;
                    closest = Mathf.Abs(wallsX[i] - enemyX) + Mathf.Abs(wallsY[i] - enemyY);
                }
            }

            //***Compute target area to head for in order to seperate the enemy with the wall from the player

            //Wall is above enemy
            if (wallsX[closestIndex] > enemyX)
            {
                //Check fields above the wall
                for (int i = wallsX[closestIndex] + 1; i <= (x + sightRng); i++)
                {
                    //Still in map range
                    if (i >= 0 && i <= (Utils.SIZE_X - 1))
                    {
                        //Search for first field above the wall that's passable
                        if (gamestate[i, y].tag != Utils.WALL_TAG)
                        {
                            targetX = i;
                            targetY = enemyY;
                            targetFound = true;
                            //Break loop
                            i = x + sightRng + 1;
                        }
                    }
                }
            }

            //Wall is under enemy
            else if (wallsX[closestIndex] < enemyX)
            {
                //Check the space under the wall to the lowest sight range
                for (int i = wallsX[closestIndex] - 1; i >= (x - sightRng); i--)
                {
                    //Still in map range
                    if (i >= 0 && i <= (Utils.SIZE_X - 1))
                    {
                        //Search for first field under the wall that's passable
                        if (gamestate[i, y].tag != Utils.WALL_TAG)
                        {
                            targetX = i;
                            targetY = enemyY;
                            targetFound = true;
                            //Break loop
                            i = (x - sightRng) - 1;
                        }
                    }
                }
            }

            //Wall is on enemy's right side
            else if (wallsY[closestIndex] > enemyY)
            {
                for (int i = wallsY[closestIndex] + 1; i <= (y + sightRng); i++)
                {
                    //Still in map range
                    if (i >= 0 && i <= (Utils.SIZE_Y - 1))
                    {
                        //Search for first field under the wall that's passable
                        if (gamestate[x, i].tag != Utils.WALL_TAG)
                        {
                            targetX = enemyX;
                            targetY = i;
                            targetFound = true;
                            //Break loop
                            i = (y + sightRng) + 1;
                        }
                    }
                }
            }

            //Wall is on enemy's left side
            else
            {
                for (int i = wallsY[closestIndex] - 1; i >= (y - sightRng); i--)
                {
                    //Still in map range
                    if (i >= 0 && i <= (Utils.SIZE_Y - 1))
                    {
                        //Search for first field under the wall that's passable
                        if (gamestate[x, i].tag != Utils.WALL_TAG)
                        {
                            targetX = enemyX;
                            targetY = i;
                            targetFound = true;
                            //Break loop
                            i = (y - sightRng) - 1;
                        }
                    }
                }
            }

            //**No free space found to move to

            if (!targetFound)
            {
                //Check if there is a field between the wall and the border

                //Wall is above enemy
                if (wallsX[closestIndex] > enemyX)
                {
                    //Wall isn't directly under the border
                    if (wallsX[closestIndex] != (Utils.SIZE_X - 1))
                    {
                        //Head for the area above the wall, no matter if it is free
                        targetX = wallsX[closestIndex] + 1;
                        targetY = enemyY;
                        targetFound = true;
                    }
                }
                //Wall is under enemy
                else if (wallsX[closestIndex] < enemyX)
                {
                    //Wall isn't directly above the border
                    if (wallsX[closestIndex] != 0)
                    {
                        //Head for the area under the wall, no matter if it is free
                        targetX = wallsX[closestIndex] - 1;
                        targetY = enemyY;
                        targetFound = true;
                    }
                }
                //Wall is on enemy's right side
                else if (wallsY[closestIndex] > enemyY)
                {
                    //Wall isn't directly under the border
                    if (wallsY[closestIndex] != (Utils.SIZE_Y - 1))
                    {
                        //Head for the area on the right side of the wall, no matter if it is free
                        targetX = enemyX;
                        targetY = wallsY[closestIndex] + 1;
                        targetFound = true;
                    }
                }
                //Wall is on enemy's left side
                else
                {
                    //Wall isn't directly above the border
                    if (wallsY[closestIndex] != 0)
                    {
                        //Head for the area on the left side of the wall, no matter if it is free
                        targetX = enemyX;
                        targetY = wallsY[closestIndex] - 1;
                        targetFound = true;
                    }
                }
            }
        }

        //**No wall found next to the enemy or there is no passable area behind it

        if (!targetFound)
        {

            //***Check for the fastest path to the exit while avoiding getting to close to the enemy

            //Debug.Log("No wall found next to the enemy. Checking for fastest path to the exit while avoiding getting to close to the enemy.");
            //Compute the next field on the fastest path to the exit
            moveToExitNode.doAction(sightRng, out targetX, out targetY);

            //The computed field is 2 fields next to the enemy
            if (Utils.is2FieldsClose(enemyX, enemyY, targetX, targetY))
            {
                //The computed field is reached by a x-axis move
                if (targetX > 0)
                {
                    //It is possible to move one to the right
                    if ((x + 1) < (Utils.SIZE_X - 1))
                    {
                        //Move y-axis to avoid getting to close to the enemy
                        xDir2 = 0;
                        yDir2 = 1;
                    }
                    else
                    {
                        //Move away from the border and the enemy
                        xDir2 = 0;
                        yDir2 = -1;
                    }
                }
                //The computed field is reached by a y-axis move
                else
                {
                    //It is possible to move one to the top
                    if ((x + 1) < (Utils.SIZE_X - 1))
                    {
                        //Move x-axis to avoid getting to close to the enemy
                        xDir2 = 1;
                        yDir2 = 0;
                    }
                    else
                    {
                        //Move away from the border and the enemy
                        xDir2 = -1;
                        yDir2 = 0;
                    }
                }
            }
            else
            {
                //Move to the computed field
                xDir2 = targetX;
                yDir2 = targetY;
            }
        }

        //***Target area found
        else
        {
            //Calculate a free path to the target location
            resultNode = Utils.recursivePath(x, y, x, y, sightRng, targetX, targetY, gamestate, new Vector2[] { new Vector2(x, y) });

            //Path found
            if (resultNode[0] != -1)
            {
                targetX = resultNode[0] - x;
                targetY = resultNode[1] - y;

                //**Check for free path to target area

                //The computed field is 2 fields next to the enemy
                if (Utils.is2FieldsClose(enemyX, enemyY, targetX, targetY))
                {
                    //The computed field is reached by a x-axis move
                    if (targetX > 0)
                    {
                        //It is possible to move one to the right
                        if ((x + 1) < (Utils.SIZE_X - 1))
                        {
                            //Move y-axis to avoid getting to close to the enemy
                            xDir2 = 0;
                            yDir2 = 1;
                        }
                        else
                        {
                            //Move away from the border and the enemy
                            xDir2 = 0;
                            yDir2 = -1;
                        }
                    }
                    //The computed field is reached by a y-axis move
                    else
                    {
                        //It is possible to move one to the top
                        if ((x + 1) < (Utils.SIZE_X - 1))
                        {
                            //Move x-axis to avoid getting to close to the enemy
                            xDir2 = 1;
                            yDir2 = 0;
                        }
                        else
                        {
                            //Move away from the border and the enemy
                            xDir2 = -1;
                            yDir2 = 0;
                        }
                    }
                }
                else
                {
                    //Move to the computed field
                    xDir2 = targetX;
                    yDir2 = targetY;
                }
            }
            //No Path found
            else
            {
                //Randomly choose to move in x direction
                if (Random.Range(0, 1) == 0)
                {
                    //Debug.Log("Random is 0");
                    xDir2 = 1;
                    yDir2 = 0;
                }
                //Randomly choose to move in y direction
                else
                {
                    //Debug.Log("Random is 1");
                    xDir2 = 0;
                    yDir2 = 1;
                }
            }
        }
    }
}