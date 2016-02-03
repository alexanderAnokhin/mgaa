using UnityEngine;
using System.Collections;

public class MoveToExit : ActionTreeNode
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
        int targetX, targetY;
        int j = 0;
        string[] impassable = new[] { Utils.WALL_TAG, Utils.ENEMY_TAG };
        int[] resultNode;

        //Check map size and sight range and set target accordingly
        if (x + sightRng > Utils.SIZE_X - 1)
            targetX = Utils.SIZE_X - 1;
        else
            targetX = x + sightRng;
        if (y + sightRng > Utils.SIZE_Y - 1)
            targetY = Utils.SIZE_Y - 1;
        else
            targetY = y + sightRng;

        int maxDist;
        if (targetX - x > targetY - y)
            maxDist = targetX - x;
        else
            maxDist = targetY - y;

        //***Search for the furthest target accessible location
        //Debug.Log("targetX: "+targetX+", targetY: "+targetY+", maxDist: "+maxDist);
        for (int i = 0; i <= maxDist; i++)
        {
            //Debug.Log("new i: "+i);
            if (targetX - i > 0)
            {
                targetX = targetX - i;
                //No wall at target location
                //Debug.Log("new targetX: " + targetX);
                if (System.Array.IndexOf(impassable, gamestate[targetX, targetY].tag) == -1)
                {
                    //Debug.Log("i set to MAX");
                    j = maxDist;
                }
            }
            if (targetY - i > 0)
            {
                targetY = targetY - i;
                //No wall at target location
                //Debug.Log("new targetY: " + targetY);
                if (System.Array.IndexOf(impassable, gamestate[targetX, targetY].tag) == -1)
                {
                    //Debug.Log("i set to MAX");
                    i = maxDist;
                }
            }
            if(j == maxDist)
            {
                j = 0;
                i = maxDist;
            }
        }

        //***Find shortest path to target location

        resultNode = Utils.recursivePath(x, y, x, y, sightRng, targetX, targetY, gamestate, new Vector2[] { new Vector2(x,y) });

        //Path found
        if (resultNode[0] != -1)
        {
            xDir2 = resultNode[0] - x;
            yDir2 = resultNode[1] - y;
        }
        //No Path found
        else
        {
            //Randomly choose to move in x direction
            if(Random.Range(0, 1) == 0) {
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