using UnityEngine;
using System.Collections;
//****
using System.Collections.Generic;
//****

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
        //****
        List<int> foodX = new List<int>();
        List<int> foodY = new List<int>();
        bool pathFound = true;
        int currentX = x;
        int currentY = y;
        int maxSightRngXp, maxSightRngYp, maxSightRngXn, maxSightRngYn;
        /*
        if ((x + sightRng) > Utils.SIZE_X)
            maxSightRngX = Utils.SIZE_X;
        else
            maxSightRngX = x + sightRng;
        if ((y + sightRng) > Utils.SIZE_Y)
            maxSightRngY = Utils.SIZE_Y;
        else
            maxSightRngY = y + sightRng;*/
        //****
        //x is further away from the exit than y
        if (x < y)
        {
            //****
            while (pathFound)
            {
                if ((currentX + 1) < Utils.SIZE_X && (currentX + 1) < (x + sightRng))
                    maxSightRngXp = currentX + 1;
                else
                    maxSightRngXp = currentX;
                if ((currentY + 1) < Utils.SIZE_Y && (currentY + 1) < (y + sightRng))
                    maxSightRngYp = currentY + 1;
                else
                    maxSightRngYp = currentY;

                //test x fields in sight range
                for(int i = currentX; i <= maxSightRngXp; i++)
                {
                    for(int j = currentY; j <= maxSightRngYp; j++)
                    {
                        if(gamestate[i,j].tag != Utils.WALL_TAG)
                        {

                        }
                    }
                }
            }
            //****
            //there is a wall in x direction but none in y direction and there is space to go in y direction
            if(gamestate[x + 1, y].tag == Utils.WALL_TAG && gamestate[x, y + 1].tag != Utils.WALL_TAG && y != Utils.SIZE_Y)
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
            if (gamestate[x , y + 1].tag == Utils.WALL_TAG && gamestate[x + 1, y].tag != Utils.WALL_TAG && x != Utils.SIZE_X)
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
            if(gamestate[x + 1, y].tag == Utils.WALL_TAG && gamestate[x, y + 1].tag != Utils.WALL_TAG)
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

    //Rekursive Methode die alle Flächen im Sichtradius des Spielers durchgeht und für alle benachbarten der aktuellen Fläche sich selbst rekursiv aufruft.
    //Speichert besuchte Flächen in einem Array und darf diese nicht erneut besuchen. Wenn keine besuchbare Fläche im Sichtradius vorhanden ist -> abbruch (leeres array)
    //Wenn am target angekommen = Array mit path
    //Gibt Länge des Path zurück; wenn keiner gefunden -1
    /*private int recursivePath(int x, int y, int currentX, int currentY, int sightRng, int targetX, int targetY, GameObject[,] gamestate, Vector2[] visited, int maxLength)
    {
        int startX, endX, startY, endY;
        Vector2 current;
        //Target reached
        if(currentX == targetX && currentY == targetY)
        {
            //This is a correct path, return first step of this path
            return (visited.Length);
        }
        else
        {
            //Check borders
            if ((currentX - 1) <= 0)
                startX = 0;
            else
                startX = currentX - 1;
            if ((currentX + 1) >= Utils.SIZE_X)
                endX = Utils.SIZE_X;
            else
                endX = currentX + 1;
            if ((currentY - 1) <= 0)
                startY = 0;
            else
                startY = currentY - 1;
            if ((currentY + 1) >= Utils.SIZE_Y)
                endY = Utils.SIZE_Y;
            else
                endY = currentY + 1;

            /*for (int i = startX; i < endX; i++) {
                for(int j = startY; j < endY; j++)
                {
                    current = new Vector2(i, j);
                    //this field was not visited before
                    if(System.Array.IndexOf(visited, current) == -1) {

                    }
                }
            }*/
            //If x+1 reachable and not visited
            //TODO instead of IndexOf: helper methode that goes through the array and checks for same coords
           /* if((currentX + 1) <= Utils.SIZE_X && System.Array.IndexOf(visited, new Vector2(currentX + 1, currentY)) > -1)
            {

            }
            if(currentX - 1 >= 0 && System.Array.IndexOf(visited, new Vector2(currentX - 1, currentY)) > -1)
            {

            }
            if((currentY + 1) <= Utils.SIZE_Y && System.Array.IndexOf(visited, new Vector2(currentX, currentY + 1)) > -1)
            {

            }
            if(currentY - 1 >= 0 && System.Array.IndexOf(visited, new Vector2(currentX, currentY - 1)) > -1)
            {

            }
        }
    }*/
}