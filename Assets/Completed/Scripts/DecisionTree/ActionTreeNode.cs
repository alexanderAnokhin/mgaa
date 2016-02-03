using UnityEngine;
using System.Collections;

public abstract class ActionTreeNode : DecisionTreeNode {

    //Does the action itself
    abstract public void doAction(int sightRng, out int xDir, out int yDir);
}