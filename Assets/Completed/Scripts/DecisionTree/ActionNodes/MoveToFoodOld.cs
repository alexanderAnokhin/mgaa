using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveToFoodOld : ActionTreeNode
{

    override public ActionTreeNode makeDecision(int sightRng)
    {
        return this;
    }

    override public void doAction(int sightRng, out int xDir2, out int yDir2)
    {
        Vector2 playerPosition = Utils.GetPlayerPosition();
        GameObject[,] gamestate = Utils.GetMap();
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;
        List<int> foodX = new List<int>();
        List<int> foodY = new List<int>();

        //***Scan for all food in sight of the player

        //For the front x view area
        for (int i = (x - sightRng); i <= (x + sightRng); i++)
        {
            //Still in map range
            if (i >= 0 && i <= (Utils.SIZE_X - 1))
            {
                for (int j = (y - sightRng); j <= (y + sightRng); j++)
                {
                    if (j >= 0 && j <= (Utils.SIZE_Y - 1))
                    {
                        //If the checked area contains food
                        if (gamestate[i, j].tag == Utils.FOOD_TAG || gamestate[i, j].tag == Utils.SODA_TAG)
                        {
                            foodX.Add(i);
                            foodY.Add(j);
                        }
                    }
                }
            }
        }
        int[] foodXa = foodX.ToArray();
        int[] foodYa = foodY.ToArray();
        int index = -1;

        //Set to highest possible value
        int combi = Utils.SIZE_X + Utils.SIZE_Y + (Utils.SIZE_X * Utils.SIZE_Y) - 2;
        int numWalls = 0;
        int difX;
        int difY;

        //***Search for the closest food

        for (int i = 0; i < foodXa.Length; i++)
        {
            //**Calculate penalty if too much walls are in the way

            difX = foodXa[i] - x;
            difY = foodYa[i] - y;

            //Food x coordinate is in positive direction
            if (difX > 0)
            {
                for (int j = x; j <= foodXa[i]; j++)
                {
                    //Food y coordinate is in positive direction
                    if (difY > 0)
                    {
                        for (int k = y; k <= foodYa[i]; k++)
                        {
                            if (gamestate[j, k].tag == Utils.WALL_TAG) //Enemies don't have to be concidered, this Action node is not triggered if there is an enemy in sight
                            {
                                numWalls++;
                            }
                        }
                    }
                    //Food y coordinate is in negative direction
                    else
                    {
                        for (int k = y; k >= foodYa[i]; k--)
                        {
                            if (gamestate[j, k].tag == Utils.WALL_TAG)
                            {
                                numWalls++;
                            }
                        }
                    }
                }
            }
            //Food y coordinate is in negative direction
            else
            {
                for (int j = x; j <= foodXa[i]; j++)
                {
                    //Food y coordinate is in positive direction
                    if (difY > 0)
                    {
                        for (int k = y; k <= foodYa[i]; k++)
                        {
                            if (gamestate[j, k].tag == Utils.WALL_TAG)
                            {
                                numWalls++;
                            }
                        }
                    }
                    //Food y coordinate is in negative direction
                    else
                    {
                        for (int k = y; k >= foodYa[i]; k--)
                        {
                            if (gamestate[j, k].tag == Utils.WALL_TAG)
                            {
                                numWalls++;
                            }
                        }
                    }
                }
            }

            //If the food at index i is closer than the currently closest one
            if ((foodXa[i] + foodYa[i] + numWalls) < combi)
            {
                //Safe this as closest
                combi = foodXa[i] + foodYa[i] + numWalls;
                index = i;
            }

            numWalls = 0;
        }

        //***Calculate the move coordinates to the nearest food**
        Debug.Log("Player Coords: x:" + x + ", y:" + y);
        Debug.Log("Food Coords: x:" + foodXa[index] + ", y:" + foodYa[index]);

        difX = foodXa[index] - x;
        difY = foodYa[index] - y;
        GameObject nextY, nextX;
        if (difY > 0)
            nextY = gamestate[x, y + 1];
        else
            nextY = gamestate[x, y - 1];
        if (difX > 0)
            nextX = gamestate[x + 1, y];
        else
            nextX = gamestate[x - 1, y];

        //x distance is shorter
        if (Mathf.Abs(difX) < Mathf.Abs(difY))
        {

            //**Check for slowing walls

            //If there is a wall in y direction & there is a distance to go in x direction
            if (nextY.tag == Utils.WALL_TAG && Mathf.Abs(difX) > 0)
            {
                //Not another wall in x direction, too
                if (nextX.tag != Utils.WALL_TAG)
                {
                    //Move x direction
                    Debug.Log("Wall in y direction, moving x direction");
                    if (difX > 0)
                        xDir2 = 1;
                    else
                        xDir2 = -1;
                    yDir2 = 0;
                }
                //There is another wall
                else
                {
                    //still go y direction
                    Debug.Log("Wall in y and x direction, still moving y direction");
                    xDir2 = 0;
                    if (difY > 0)
                        yDir2 = 1;
                    else
                        yDir2 = -1;
                }
            }
            //No wall and/or no distance in x direction
            else
            {
                //Wall in y direction but x distance is 0
                if (nextY.tag == Utils.WALL_TAG)
                {
                    //Map size allows to move above wall
                    if (x < Utils.SIZE_X && ((difY > 0 && y < Utils.SIZE_Y - 1) || (difY < 0 && y > 1))
                        //Check for free path dexter above wall
                        && ((difY > 0 && gamestate[x + 1, y].tag != Utils.WALL_TAG && gamestate[x + 1, y + 1].tag != Utils.WALL_TAG && gamestate[x + 1, y + 2].tag != Utils.WALL_TAG))
                        //Check for free path sinistral above wall
                        || (difY < 0 && gamestate[x + 1, y].tag != Utils.WALL_TAG && gamestate[x + 1, y - 1].tag != Utils.WALL_TAG && gamestate[x + 1, y - 2].tag != Utils.WALL_TAG))
                    {
                        Debug.Log("Wall in y direction, distance in x is 0 but there is a path above the wall, moving x direction");
                        xDir2 = 1;
                        yDir2 = 0;
                    }
                    //Map size allows to move underneath wall
                    else if (x > 0 && ((difY > 0 && y < Utils.SIZE_Y - 1) || (difY < 0 && y > 1))
                       //Check for free path dexter underneath wall
                       && ((difY > 0 && gamestate[x - 1, y].tag != Utils.WALL_TAG && gamestate[x - 1, y + 1].tag != Utils.WALL_TAG && gamestate[x - 1, y + 2].tag != Utils.WALL_TAG))
                       //Check for free path sinistral underneath wall
                       || (difY < 0 && gamestate[x - 1, y].tag != Utils.WALL_TAG && gamestate[x - 1, y - 1].tag != Utils.WALL_TAG && gamestate[x - 1, y - 2].tag != Utils.WALL_TAG))
                    {
                        Debug.Log("Wall in y direction, distance in x is 0 but there is a path underneath the wall, moving x direction");
                        xDir2 = -1;
                        yDir2 = 0;
                    }
                    //Wall can't be avoided
                    else
                    {
                        //Go y direction
                        Debug.Log("Wall in y direction that can't be avoided, moving y direction");
                        xDir2 = 0;
                        if (difY > 0)
                            yDir2 = 1;
                        else
                            yDir2 = -1;
                    }
                }
                //No Wall in y direction
                else {
                    //Go y direction
                    Debug.Log("No wall in y direction, moving y direction");
                    xDir2 = 0;
                    if (difY > 0)
                        yDir2 = 1;
                    else
                        yDir2 = -1;
                }
            }
        }
        //y distance is shorter
        else if (Mathf.Abs(difX) > Mathf.Abs(difY))
        {
            //**Check for slowing walls

            //If there is a wall in x direction & there is a distance to go in y direction
            if (nextX.tag == Utils.WALL_TAG && Mathf.Abs(difY) > 0)
            {
                //Not another wall in y direction, too
                if (nextY.tag != Utils.WALL_TAG)
                {
                    //Move y direction
                    Debug.Log("Wall in x direction, moving y direction");
                    xDir2 = 0;
                    if (difY > 0)
                        yDir2 = 1;
                    else
                        yDir2 = -1;
                }
                //There is another wall
                else
                {
                    //still go x direction
                    Debug.Log("Wall in x and y direction, still moving x direction");
                    if (difX > 0)
                        xDir2 = 1;
                    else
                        xDir2 = -1;
                    yDir2 = 0;
                }
            }
            //No wall and/or no distance in y direction
            else
            {
                if (nextX.tag == Utils.WALL_TAG)
                {
                    //Map size allows to move dexter aside wall
                    if (y < Utils.SIZE_Y && ((difX > 0 && x < Utils.SIZE_X - 1) || (difX < 0 && x > 1))
                        //Check for free path dexter aside and above wall
                        && ((difX > 0 && gamestate[x, y + 1].tag != Utils.WALL_TAG && gamestate[x + 1, y + 1].tag != Utils.WALL_TAG && gamestate[x + 2, y + 1].tag != Utils.WALL_TAG))
                        //Check for free path dexter aside and underneath wall
                        || (difX < 0 && gamestate[x, y + 1].tag != Utils.WALL_TAG && gamestate[x - 1, y + 1].tag != Utils.WALL_TAG && gamestate[x - 2, y + 1].tag != Utils.WALL_TAG))
                    {
                        Debug.Log("Wall in x direction, distance in y is 0 but there is a path dexter aside the wall, moving y direction");
                        xDir2 = 0;
                        yDir2 = 1;
                    }
                    //Map size allows to move sinistral aside wall
                    else if (y > 0 && ((difX > 0 && x < Utils.SIZE_X - 1) || (difX < 0 && x > 1))
                       //Check for free path sinistral aside and above wall
                       && ((difX > 0 && gamestate[x, y - 1].tag != Utils.WALL_TAG && gamestate[x + 1, y - 1].tag != Utils.WALL_TAG && gamestate[x + 2, y - 1].tag != Utils.WALL_TAG))
                       //Check for free path sinistral aside and underneath wall
                       || (difX < 0 && gamestate[x, y - 1].tag != Utils.WALL_TAG && gamestate[x - 1, y - 1].tag != Utils.WALL_TAG && gamestate[x - 2, y - 1].tag != Utils.WALL_TAG))
                    {
                        Debug.Log("Wall in x direction, distance in y is 0 but there is a path sinistral aside the wall, moving y direction");
                        xDir2 = 0;
                        yDir2 = -1;
                    }
                    //Wall can't be avoided
                    else
                    {
                        //Go x direction
                        Debug.Log("Wall in x direction that can't be avoided, moving x direction");
                        if (difX > 0)
                            xDir2 = 1;
                        else
                            xDir2 = -1;
                        yDir2 = 0;
                    }
                }
                //No Wall in y direction
                else
                {
                    //Go x direction
                    Debug.Log("No wall in x direction, moving x direction");
                    if (difX > 0)
                        xDir2 = 1;
                    else
                        xDir2 = -1;
                    yDir2 = 0;
                }
            }
        }
        //The distances to the nearest food are equal
        else {
            //Wall in x direction
            if (nextX.tag == Utils.WALL_TAG)
            {
                //Wall in x and y direction
                if (nextY.tag == Utils.WALL_TAG)
                {
                    //Randomly decide to go in x direction
                    if (Random.Range(0, 1) == 0)
                    {
                        Debug.Log("x&y equal distance and wall both directions, decided to move in x direction");
                        if (difX > 0)
                            xDir2 = 1;
                        else
                            xDir2 = -1;
                        yDir2 = 0;
                    }
                    //Or in y direction
                    else
                    {
                        Debug.Log("x&y equal distance and wall both directions, decided to move in y direction");
                        xDir2 = 0;
                        if (difY > 0)
                            yDir2 = 1;
                        else
                            yDir2 = -1;
                    }
                }
                //No Wall in y direction
                else
                {
                    Debug.Log("x&y equal distance and wall in x direction, moving y direction");
                    xDir2 = 0;
                    if (difY > 0)
                        yDir2 = 1;
                    else
                        yDir2 = -1;
                }
            }
            else
            {
                //No wall in x but in y direction
                if (nextY.tag == Utils.WALL_TAG)
                {
                    Debug.Log("x&y equal distance and no wall in x direction, moving x direction");
                    if (difX > 0)
                        xDir2 = 1;
                    else
                        xDir2 = -1;
                    yDir2 = 0;
                }
                //No wall in both directions
                else
                {
                    //Randomly decide to go in x direction
                    if (Random.Range(0, 1) == 0)
                    {
                        Debug.Log("x&y equal distance and no wall both directions, decided to move in x direction");
                        if (difX > 0)
                            xDir2 = 1;
                        else
                            xDir2 = -1;
                        yDir2 = 0;
                    }
                    //Or in y direction
                    else
                    {
                        Debug.Log("x&y equal distance and no wall both directions, decided to move in y direction");
                        xDir2 = 0;
                        if (difY > 0)
                            yDir2 = 1;
                        else
                            yDir2 = -1;
                    }
                }

            }
        }
    }
}