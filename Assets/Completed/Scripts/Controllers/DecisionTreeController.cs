using UnityEngine;
using System.Collections;

public class DecisionTreeController : Controller {
    private int sightRng = 2;
    private SGEInSight node = new SGEInSight();

    override public void Move(out int xDir, out int yDir)
    {
        GameObject player = Utils.GetPlayerGameObject();

        Vector2 playerPosition = (Vector2)player.transform.position;
        Vector2 exitPosition = Utils.GetExitPosition();

        //Safe Gamestate
        GameObject[,] gamestate = Utils.GetMap();


        //Call DecisionTree to get a proper action for the current gamestate
        ActionTreeNode action = node.makeDecision(sightRng);
        Debug.Log("Got Action"+action);
        int xDir2, yDir2;
        xDir = 1;
        yDir = 0;
        //Do proper action
        action.doAction(sightRng, out xDir2, out yDir2);
        xDir = xDir2;
        yDir = yDir2;
    }
}
