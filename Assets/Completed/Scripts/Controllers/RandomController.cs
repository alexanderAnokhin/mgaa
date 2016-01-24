using UnityEngine;
using System.Collections;

public class RandomController : Controller {

    override public void Move (out int xDir, out int yDir) {
        Vector2[] pointsAround = Utils.POINTS_AROUND;
        
        Vector2 newPoint = pointsAround[Random.Range (0, 4)];

        xDir = (int)newPoint.x; 
        yDir = (int)newPoint.y;
    }
}
