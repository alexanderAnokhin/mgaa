using UnityEngine;
using System.Collections;

public class MoveRndDirection : ActionTreeNode
{

    override public ActionTreeNode makeDecision(int sightRng)
    {
        return this;
    }

    override public void doAction(int sightRng, out int xDir, out int yDir)
    {
        Vector2[] pointsAround = Utils.POINTS_AROUND;

        Vector2 newPoint = pointsAround[Random.Range(0, 4)];

        xDir = (int)newPoint.x;
        yDir = (int)newPoint.y;
    }
}
