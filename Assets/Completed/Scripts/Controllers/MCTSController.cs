/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;

public class MCTSController : Controller {
    abstract class StateObject { 
        abstract public bool IsEnemy();
        abstract public bool IsFood();
        abstract public bool IsWall();
        abstract public bool IsExit();
        abstract public bool CanBeVisited();
    }

    class EnemyStateObject : StateObject {
        float power;
        
        public EnemyStateObject(float power) { this.power = power; }
        
        public int GetPower() { return power; }

        override public bool IsEnemy() { return true; }
        override public bool IsFood() { return false; }
        override public bool IsWall() { return false; }
        override public bool IsExit() { return false; }
        override public bool CanBeVisited() { return false; }
    }

    class FoodStateObject : StateObject {
        float amount;

        public FoodStateObject(float amount) { this.amount = amount; }
        
        public int GetAmount() { return amount; }

        override public bool IsEnemy() { return false; }
        override public bool IsFood() { return true; }
        override public bool IsWall() { return false; }
        override public bool IsExit() { return false; }
        override public bool CanBeVisited() { return true; }
    }

    class WallStateObject : StateObject {
        float hp;
        
        public WallStateObject(float hp) { this.hp = hp; }

        public WallStateObject Attacked() { this.hp --; return this;}
        public float GetHp() { return hp; }

        override public bool IsEnemy() { return false; }
        override public bool IsFood() { return false; }
        override public bool IsWall() { return true; }
        override public bool IsExit() { return false; }
        override public bool CanBeVisited() { return true; }
    }

    class ExitStateObject : StateObject { 
        override public bool IsEnemy() { return false; }
        override public bool IsFood() { return false; }
        override public bool IsWall() { return false; }
        override public bool IsExit() { return true; }
        override public bool CanBeVisited() { return true; }
    }

    class Node {
        int x;
        int y;
        float reward;
        StateObject[, ] state;
        int visited;
        List<Node> childs;
        Node parent;
        
        

        public void Expand(){
            foreach (Vector2 point in Utils.POINTS_AROUND) {
                Vector2 newPos = new Vector2(x, y) + point;
                
                if (Utils.OnMap(newPos.x, newPos.y) && state[newPos.x, newPos.y].CanBeVisited()) {
                    float newReward;
                    int newX, newY;

                    StateObject[, ] nextState = NextState(state, x, y, point.x, point.y, newX, newY, newReward);
                    Node child = new Node(newX, newY, reward + newReward, nextState, );            
                }    
            }
        }                
    }            

    override public void Move (out int xDir, out int yDir) {
        

        Vector2[] pointsAround = Utils.POINTS_AROUND;
        
        Vector2 newPoint = pointsAround[Random.Range (0, 4)];
        
        xDir = (int)newPoint.x; 
        yDir = (int)newPoint.y;
    }

    private StateObject[, ] CreateInitialState () {
        StateObject[, ] state = new StateObject[Utils.SIZE_X, Utils.SIZE_Y];
        
        GameObject[] enemies = Utils.GetEnemiesGameObjects ();
        GameObject[] foods = Utils.GetFoodGameObjects ();
        GameObject[] sodas = Utils.GetSodaGameObjects ();
        GameObject[] walls = Utils.GetWallGameObjects ();
        GameObject exit = Utils.GetExitGameObject ();
        
        foreach (GameObject enemy in enemies) {
            state[(int)enemy.transform.position.x, (int)enemy.transform.position.y] = new EnemyStateObject ((float)enemy.GetComponent<Enemy> ().playerDamage);           
        }

        foreach (GameObject food in foods) {
            state[(int)food.transform.position.x, (int)food.transform.position.y] = new FoodStateObject (Utils.FOOD_INCREASE);           
        }
        
        foreach (GameObject soda in sodas) {
            state[(int)soda.transform.position.x, (int)soda.transform.position.y] = new FoodStateObject (Utils.SODA_INCREASE);           
        }
        
        foreach (GameObject wall in walls) {
            state[(int)wall.transform.position.x, (int)wall.transform.position.y] = new WallStateObject ((float)wall.GetComponent<Wall> ().hp);           
        }
        
        state[(int)exit.transform.position.x, (int)exit.transform.position.y] = new ExitStateObject ();        
        
        return state;
    }

    private List<Vector2> MCTS (StateObject[, ] state0, float reward0) {
        

        return null;
    }

    public StateObject[, ] NextState(StateObject[, ] state, int playerX, int playerY, int xDir, int yDir, out int x, out int y, out float reward) {
        reward = -1f;
        float penalty;

        StateObject obj = state[playerX + xDir, playerY + yDir];
        // nothing is here
        if (obj == null) {
            x = playerX + xDir;
            y = playerY + yDir;
            state = MovePlayer(state, playerX, playerY, x, y);
            state = MoveEnemies(state, x, y, penalty);
            reward += penalty;    
        }
        // exit is here
        else if (obj.IsExit ()) {
            x = playerX + xDir;
            y = playerY + yDir;
            state = MovePlayer(state, playerX, playerY, x, y);
            state = MoveEnemies(state, x, y, penalty);
            reward += penalty;        
        }
        // food is here
        else if (obj.IsFood ()) {
            x = playerX + xDir;
            y = playerY + yDir;
            state = MovePlayer(state, playerX, playerY, x, y);
            reward += ((FoodStateObject)obj).GetAmount ();
            state = MoveEnemies(state, x, y, penalty);
            reward += penalty;
        }
        // wall is here
        else if (obj.IsWall ()) {
            x = playerX + xDir;
            y = playerY + yDir;
            
            WallStateObject wall = (WallStateObject)obj;

            if (wall.GetHp () == 0) {
                state = MovePlayer(state, playerX, playerY, x, y);
                state = MoveEnemies(state, x, y, penalty);
                reward += penalty;
            }
            else {
                state[x, y] = wall.Attacked();
                state = MoveEnemies(state, playerX, playerY, penalty);
                reward += penalty;
            }
        }
        else {
            x = playerX;
            y = playerY;
        }       

        return reward;
    }

    private StateObject[, ] MoveEnemies(StateObject[, ] state, int playerX, int playerY, out float penalty) {
        penalty = 0f;
        Vector2 playerPos = new Vector2(playerX, playerY);         

        for (int i = 0; i < Utils.SIZE_X; i++) {
            for (int j = 0; j < Utils.SIZE_Y; j++) {
                float distance = Utils.GetManhattenDistance (new Vector2(i, j), playerPos);
                // attack player
                if (state[i, j].IsEnemy () && distance == 1f) {
                    penalty -= ((EnemyStateObject)state[i, j]).GetPower ();
                }
                // move randomly enemy
                else if (state[i, j].IsEnemy () && distance < 4f) {
                    state = MoveEnemy (state, i, j);
                }
            }   
        }

        return state;
    }
    
    private StateObject[, ] MovePlayer(StateObject[, ] state, int x, int y, int newX, int newY) {
        StateObject player = state[x, y];
        
        state[newX, newY] = player;
        state[x, y] = null;

        return state; 
    }

    private StateObject[, ] MoveEnemy(StateObject[, ] state, int enemyX, int enemyY) {
        Vector2 enemyPos = new Vector2(enemyX, enemyY);
        EnemyStateObject enemy = (EnemyStateObject)state[enemyX, enemyY];

        foreach (Vector2 point in Utils.POINTS_AROUND) {
            Vector2 newPos = enemyPos + point; 
            
            if (Utils.OnMap(newPos.x, newPos.y) && state[newPos.x, newPos.y] == null) {
                state[newPos.x, newPos.y] = enemy;
                state[enemyX, enemyY] = null;
            }
        }

        return state;
    }    
}
*/