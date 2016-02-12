using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class MCTSNode {
    int x;
    int y;
    int xDir;
    int yDir;
    float reward;
    int visited;
    StateObject[, ] state;
    List<MCTSNode> childs;
    MCTSNode parent;
    bool isTerminal;
    
    public MCTSNode(int x, int y, int xDir, int yDir, float reward, StateObject[, ] state, MCTSNode parent, bool isTerminal) {
        this.x = x;
        this.y = y;
        this.xDir = xDir;
        this.yDir = yDir;
        this.reward = reward;
        visited = 1;
        this.state = state;
        this.parent = parent;
        this.isTerminal = isTerminal;
    }
    
    public int XDir() { return xDir; }
    public int YDir() { return yDir; }
    public int GetVisits() { return visited; }
    public bool IsTerminalOrNoChilds() { return isTerminal || childs == null; }
    public Vector2 GetVector() { return new Vector2(x, y); }

    public void ExpandChilds() {
        if (childs == null && !isTerminal) {
            childs = new List<MCTSNode> ();
            Vector2 pos = new Vector2(x, y);
            
            foreach (Vector2 point in Utils.POINTS_AROUND) {
                Vector2 newPos = pos + point;
                
                if (Utils.OnMap((int)newPos.x, (int)newPos.y)) {
                    int newX, newY;
                    float newReward;
                    bool newIsTerminal;
                    
                    StateObject[, ] nextState = MCTSController.NextState(state, x, y, (int)point.x, (int)point.y, out newX, out newY, out newReward, out newIsTerminal);
                    
                    childs.Add(new MCTSNode(newX, newY, (int)point.x, (int)point.y, newReward, nextState, this, newIsTerminal || newReward < MCTSController.DIE_REWARD));                     
                }
            }
        }
    }

    public void MakeDecision() {
        visited ++;

        if (isTerminal) { Propagate(reward); }
        else { ExpandChilds(); GetBestChild().MakeDecision(); }
    }
    
    public void Propagate(float weight) {
        if (parent == null) { UpdateReward(weight); }
        else { UpdateReward(weight); parent.Propagate(reward); }
    }

    public void UpdateReward(float weight) {
        reward = (reward * (float)visited + weight) / (float)visited;
    }
    
    public float GetUCT(int parentVisited) {
        return reward + 2 * MCTSController.C * Mathf.Sqrt(2 * Mathf.Log((float)parentVisited, Mathf.Exp(1f)) / (float)visited);
    }
    
    public MCTSNode GetBestChild() {
        return childs.OrderBy(n => -n.GetUCT(visited)).First();    
    }

    public MCTSNode GetMostVisitedChild() {
        return childs.OrderBy(n => -n.GetVisits()).First();    
    }
}
