using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveToAvoidSingleEnemy : ActionTreeNode
{

    override public ActionTreeNode makeDecision(int sightRng)
    {
        return this;
    }

    override public void doAction(int sightRng, out int xDir2, out int yDir2)
    {
        xDir2 = 1;
        yDir2 = 0;
    }
        /*Vector2 playerPosition = Utils.GetPlayerPosition();
        GameObject[,] gamestate = Utils.GetMap();
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;
        int enemyX, enemyY;
        targetX = -1;
        targetY = -1;
        int closest = Utils.SIZE_X + Utils.SIZE_Y;
        int closestIndex = -1;
        List<int> wallsX = new List<int>();
        List<int> wallsY = new List<int>();
        int[] resultNode;

        //***Search for the enemy

        //For x view area
        for (int i = (x - sightRng); i <= (x + sightRng); i++)
        {
            //Still in map range
            if (i >= 0 && i <= (Utils.SIZE_X - 1))
            {
                //For y view area
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

        //***Search for a wall next to the enemy

        for(int i = (x - sightRng); i <= (x + sightRng); i++)
        {
            if(gamestate[i,enemyY].tag == Utils.WALL_TAG)
            {
                wallsX.Add(i);
                wallsY.Add(enemyY);
            }
        }
        for(int j = (y - sightRng); j <= (y + sightRng); j++)
        {
            if (gamestate[enemyX, j].tag == Utils.WALL_TAG)
            {
                wallsX.Add(enemyX);
                wallsY.Add(j);
            }
        }

        //Wall next to enemy found
        if(wallsX.Count > 0)
        {
            //Look for the closest wall
            for(int i = 0; i < wallsX.Count; i++)
            {
                //Closer than closest so far
                if((Mathf.Abs(wallsX[i] - enemyX) + Mathf.Abs(wallsY[i] - enemyY)) < closest)
                {
                    closestIndex = i;
                    closest = Mathf.Abs(wallsX[i] - enemyX) + Mathf.Abs(wallsY[i] - enemyY);
                }
            }
        }

        //***Compute target area to head for

        //Wall is above enemy
        if(wallsX[closestIndex] > enemyX)
        {
            for (int i = 1; i <= (x + sightRng); i++)
            {
                //Search for first field above the wall that's passable
                if (gamestate[wallsX[closestIndex] + i, y].tag != Utils.WALL_TAG)
                {
                    targetX = wallsX[closestIndex] + i;
                    //Break loop
                    i = x + sightRng + 1;
                }
            }
            targetY = y;
        }
        //Wall is under enemy
        else if(wallsX[closestIndex] < enemyX)
        {
            //Check the space under the wall to the lowest sight range
            for (int i = 1; i <= (wallsX[closestIndex] - (x - sightRng)); i++)
            {
                //Search for first field under the wall that's passable
                if (gamestate[wallsX[closestIndex] - i, y].tag != Utils.WALL_TAG)
                {
                    targetX = wallsX[closestIndex] - i;
                    //Break loop
                    i = (wallsX[closestIndex] - (x - sightRng)) + 1;
                }
            }
            targetY = y;
        }
        //Wall is on enemy's right side
        else if(wallsY[closestIndex] > enemyY)
        {
            for (int i = 1; i <= (y + sightRng); i++)
            {
                //Search for first field under the wall that's passable
                if (gamestate[x, wallsY[closestIndex] + i].tag != Utils.WALL_TAG)
                {
                    targetY = wallsY[closestIndex] + i;
                    //Break loop
                    i = (y + sightRng) + 1;
                }
            }
            targetX = x;
        }
        //Wall is on enemy's left side
        else
        {
            for (int i = 1; i <= (wallsY[closestIndex] - (y - sightRng)); i++)
            {
                //Search for first field under the wall that's passable
                if (gamestate[x, wallsY[closestIndex] - i].tag != Utils.WALL_TAG)
                {
                    targetY = wallsY[closestIndex] - i;
                    //Break loop
                    i = (wallsY[closestIndex] - (y - sightRng)) + 1;
                }
            }
            targetX = x;
        }

        //***Check for free path to target area

        if(targetX != -1 && targetY != -1)
        {
            //Calculate a free path to the target location
            resultNode = Utils.recursivePath(x, y, x, y, sightRng, targetX, targetY, gamestate, new Vector2[] { new Vector2(x, y) });

            //Path found
            if (resultNode[1] != -1)
            {
                //If the next step of the found path is minimum 1 field away from the enemy
                if ((enemyY != resultNode[1]) || (Utils.is2pAway(resultNode[0], enemyX)) && ((enemyX != resultNode[0]) || (Utils.is2pAway(resultNode[1], enemyY))))
                {
                    xDir2 = resultNode[0];
                    yDir2 = resultNode[1];
                }
                else
                {
                    //**Move in wall direction or in one direction of the other dimension but hold 1 field distance to the enemy

                    //Wall is in x direction and x+1 is 1 field away from the enemy
                    if((targetX != x) && !((!Utils.is2pAway(y, enemyY)) && !(Utils.is2pAway(x + 1, enemyX))))
                    {
                        xDir2 = 1;
                        yDir2 = 0;
                    }
                    //Wall is in x direction and x-1 is 1 field away from the enemy
                    if ((targetX != x) && !((!Utils.is2pAway(y, enemyY)) && !(Utils.is2pAway(x - 1, enemyX))))
                    {
                        xDir2 = -1;
                        yDir2 = 0;
                    }
                    //Wall is in x direction but it is not possible to move in that direction
                    if(targetX != x)
                    {
                        //
                        if(enemyY > y) {
                            if (y - 1 >= 0)
                            {
                                xDir2 = 0;
                                yDir2 = -1;
                            }
                            else
                            {
                                //move wall x direction
                            }
                        }
                        else
                        {
                            xDir2 = 0;
                            yDir2 = 1;
                        }
                    }
                    //Wall is in y direction and y+1 is 1 field away from the enemy
                    if ((targetY != y) && !((!Utils.is2pAway(x, enemyX)) && !(Utils.is2pAway(y + 1, enemyY))))
                    {
                        xDir2 = 0;
                        yDir2 = 1;
                    }
                    //Wall is in x direction and x-1 is 1 field away from the enemy
                    if ((targetY != y) && !((!Utils.is2pAway(x, enemyX)) && !(Utils.is2pAway(y - 1, enemyY))))
                    {
                        xDir2 = 0;
                        yDir2 = -1;
                    }
                }
            }
        }
        //No free area found and wall is in x direction
        else if () { }
        //No free area found and wall is in y direction
        else if () { }

        //Move but stay away from enemy
            //If no passable field found -> stay away and went one in wall direction
        //Walk aside enemy to side thats shorter to exit
        //Check for free path
        //Else take other side
    }*/
}