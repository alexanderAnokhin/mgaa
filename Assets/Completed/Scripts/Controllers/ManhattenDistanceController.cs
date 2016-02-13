using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This controller is uses Manhatten distance as a measure of move "goodness"
 */
public class ManhattenDistanceController : Controller {
    
    // Main function which moves player unit
    override public void Move (out int xDir, out int yDir) {
        Vector2[] pointsAround = Utils.POINTS_AROUND;
        
        Vector2 playerPosition = Utils.GetPlayerPosition ();
        Vector2 exitPosition = Utils.GetExitPosition ();
        
        float[] distances = new float[] { 
            Utils.GetManhattenDistance(playerPosition + pointsAround[0], exitPosition),
            Utils.GetManhattenDistance(playerPosition + pointsAround[1], exitPosition),
            Utils.GetManhattenDistance(playerPosition + pointsAround[2], exitPosition),
            Utils.GetManhattenDistance(playerPosition + pointsAround[3], exitPosition) }; 

        int minIndex = 0;
        for(int i= 1; i < distances.Length; i++) {
            if (distances[i] < distances[minIndex]) {
                minIndex = i;
            }
        }

        Vector2 newPoint = pointsAround[minIndex];
        
        xDir = (int)newPoint.x; 
        yDir = (int)newPoint.y;
    } 
}
