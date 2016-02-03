using UnityEngine;
using System.Collections;
using Completed;

public class SGEInSight : DecisionTreeNode {

    private MoveToExit oneEnemyNode = new MoveToExit();
    private MoveToExit multiEnemyNode = new MoveToExit();
    //private MoveRndDirection foodNode = new MoveRndDirection();
    private MoveToFood foodNode = new MoveToFood();
    private MoveToExit noSGENode = new MoveToExit();

    override public ActionTreeNode makeDecision(int sightRng) {
        Debug.Log("*********************");
        GameObject[,] gamestate = Utils.GetMapWithFloor();
        Vector2 playerPosition = Utils.GetPlayerPosition();
        int x = (int) playerPosition.x;
        int y = (int) playerPosition.y;
        string ENEMY_TAG = Utils.ENEMY_TAG;
        string current_tag = "";
        bool sgeFound = false;
        int enemies = 0;
        //Tags of the special game elements
        string[] sge = { Utils.ENEMY_TAG, Utils.FOOD_TAG, Utils.SODA_TAG };
        //For the x view area
        for (int i = (x - sightRng); i <= (x + sightRng);i++) {
            //Only check available map space
            if (i >= 0 && i <= (Utils.SIZE_X - 1)) {
                //For the y view area
                for(int j = (y-sightRng); j <= (y + sightRng); j++)
                {   
                    if(j >= 0 && j <= (Utils.SIZE_Y - 1))
                    {
                        current_tag = gamestate[i, j].tag;
                        //Check if there is a special game element at the current position
                        if (System.Array.IndexOf(sge, current_tag) > -1)
                        {
                            //If the checked area is in front of the player and holds an enemy
                            if ((i >= x || j >= y) && current_tag == ENEMY_TAG)  
                            {
                                sgeFound = true;
                                enemies++;
                            }else if(current_tag != ENEMY_TAG)
                            {
                                sgeFound = true;
                            }
                        }
                    }
                }
            }
        }

        //There is a special game element in sight
        if (sgeFound) {
            //Enemy in sight
            if (enemies > 0)
            {
                if (enemies == 1)
                {
                    return oneEnemyNode.makeDecision(sightRng);
                }
                else {
                    return multiEnemyNode.makeDecision(sightRng);
                }
            }
            //Food in sight
            else
            {
                return foodNode.makeDecision(sightRng);
            }
        }
        //There is no special game element in sight
        else
        {
            return noSGENode.makeDecision(sightRng);
        }
    }
}
