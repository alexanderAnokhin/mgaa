using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class MCTSNode {
    int x;
    int y;
    float food;
    float reward;
    int visited;
    StateObject[, ] state;
    List<MCTSNode> childs;
    MCTSNode parent;
    bool isTerminal;
    
    public MCTSNode(int x, int y, float food, float reward, StateObject[, ] state, MCTSNode parent, bool isTerminal) {
        this.x = x;
        this.y = y;
        this.food = food;
        this.reward = reward;
        visited = 1;
        this.state = state;
        this.parent = parent;
        this.isTerminal = isTerminal;
    }
    
    public int GetVisits() { return visited; }                    // Gives the number of visits
    public bool IsNotExpanded() { return childs == null; }
    public bool IsTerminal() { return isTerminal; }
    public Vector2 GetVector() { return new Vector2(x, y); }      // Position as vector
    public float AvgReward() { return reward / (float)visited; }  // Average rewards
    public float GetFood() { return food; }                       // Food

    // Expands current node
    public void ExpandChilds() {
        if (IsNotExpanded() && !IsTerminal()) {
            childs = new List<MCTSNode> ();
            Vector2 pos = new Vector2(x, y);
            
            foreach (Vector2 point in Utils.POINTS_AROUND) {
                Vector2 newPos = pos + point;
                
                if (Utils.OnMap((int)newPos.x, (int)newPos.y)) {
                    int newX, newY;
                    float newFood, newReward;
                    bool newIsTerminal;
                    
                    StateObject[, ] nextState = MCTSController.NextState(state, x, y, (int)point.x, (int)point.y, food, out newX, out newY, out newFood, out newReward, out newIsTerminal);
                    
                    childs.Add(new MCTSNode(newX, newY, newFood, newReward, nextState, this, newIsTerminal));                     
                }
            }
        }
    }

    // Makes visit of node, expands it if necessary and propagates result back to root
    public void MakeVisit() {
        visited ++;

        if (IsTerminal()) { Propagate(AvgReward()); }
        else if (IsNotExpanded()) { ExpandChilds(); PropagateFromChilds(); }
        else { GetBestChild().MakeVisit(); }
    }
    
    // Propogates rewards from each expanded child to parents
    private void PropagateFromChilds() { foreach (MCTSNode child in childs) { child.Propagate(child.AvgReward()); } }
    
    // Propagates reward from child to parent
    public void Propagate(float weight) { if (parent != null) { parent.UpdateReward(weight); parent.Propagate(weight); } }
    
    // Updates reward from parent taken from child
    public void UpdateReward(float weight) { reward += weight; }    

    // Gets UCT measure
    public float GetUCT(int parentVisited) {
        return AvgReward() + 2 * MCTSController.C * Mathf.Sqrt(2 * Mathf.Log((float)parentVisited, Mathf.Exp(1f)) / (float)visited);
    }
    
    // Gets the best child in terms of UCT measure
    public MCTSNode GetBestChild() { return childs.OrderBy(n => -n.GetUCT(visited)).First(); }

    // Gets the most visited child
    public MCTSNode GetMostVisitedChild() { return childs.OrderBy(n => -n.GetVisits()).First(); }
}
