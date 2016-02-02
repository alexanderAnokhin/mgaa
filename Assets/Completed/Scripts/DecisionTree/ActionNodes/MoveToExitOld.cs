using UnityEngine;
using System.Collections;
//****
using System.Collections.Generic;
//****

public class MoveToExitOld : ActionTreeNode
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

        //x is further away from the exit than y
        if (x < y)
        {
            //there is a wall in x direction but none in y direction and there is space to go in y direction
            if (gamestate[x + 1, y].tag == Utils.WALL_TAG && gamestate[x, y + 1].tag != Utils.WALL_TAG && y != Utils.SIZE_Y)
            {
                //Move in y direction
                Debug.Log("x is further away but there's a wall in x direction and none in y, moving y direction");
                xDir2 = 0;
                yDir2 = 1;
            }
            else
            {
                //Move in x direction
                Debug.Log("x is further away and there isn't a wall that isn't in y direction, moving x direction");
                xDir2 = 1;
                yDir2 = 0;
            }
        }
        //y is further away from the exit than x
        else if (y < x)
        {
            //there is a wall in y direction but none in x direction and there is space to go in x direction
            if (gamestate[x, y + 1].tag == Utils.WALL_TAG && gamestate[x + 1, y].tag != Utils.WALL_TAG && x != Utils.SIZE_X)
            {
                //Move in x direction
                Debug.Log("y is further away but there's a wall in y direction and none in x, moving x direction");
                xDir2 = 1;
                yDir2 = 0;
            }
            else
            {
                //Move in y direction
                Debug.Log("y is further away and there isn't a wall that isn't in x direction, moving y direction");
                xDir2 = 0;
                yDir2 = 1;
            }
        }
        //x and y are equally far from the exit
        else
        {
            //Wall in x but not in y direction
            if (gamestate[x + 1, y].tag == Utils.WALL_TAG && gamestate[x, y + 1].tag != Utils.WALL_TAG)
            {
                //Move in y direction
                Debug.Log("both distances are equally far from exit but there is a wall in x and not in y direction, moving y direction");
                xDir2 = 0;
                yDir2 = 1;
            }
            //Wall in y but not in x direction
            else if (gamestate[x + 1, y].tag != Utils.WALL_TAG && gamestate[x, y + 1].tag == Utils.WALL_TAG)
            {
                //Move in x direction
                Debug.Log("both distances are equally far from exit but there is a wall in y and not in x direction, moving x direction");
                xDir2 = 1;
                yDir2 = 0;
            }
            //Either no wall or a wall in both directions
            else
            {
                Debug.Log("both distances are equally far from exit with both having or don't having a wall, moving randomly");
                if (Random.Range(0, 1) == 0)
                {
                    //By random move in x direction
                    Debug.Log("             in x direction");
                    xDir2 = 1;
                    yDir2 = 0;
                }
                else
                {
                    //By random move in y direction
                    Debug.Log("             in y direction");
                    xDir2 = 0;
                    yDir2 = 1;
                }
            }
        }
    }
}