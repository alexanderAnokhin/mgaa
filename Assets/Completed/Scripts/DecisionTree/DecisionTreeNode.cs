using UnityEngine;
using System.Collections;

public abstract class DecisionTreeNode{

    //Recursively walkes through the tree
    abstract public ActionTreeNode makeDecision(int sightRng);
}
