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
    }

    class EnemyStateObject : StateObject {
        float power;
        
        public EnemyStateObject(float power) { this.power = power; }
        
        public int GetPower() { return power; }

        override public bool IsEnemy() { return true; }
        override public bool IsFood() { return false; }
        override public bool IsWall() { return false; }
        override public bool IsExit() { return false; }
    }

    class FoodStateObject : StateObject {
        float amount;

        public FoodStateObject(float amount) { this.amount = amount; }
        
        public int GetAmount() { return amount; }

        override public bool IsEnemy() { return false; }
        override public bool IsFood() { return true; }
        override public bool IsWall() { return false; }
        override public bool IsExit() { return false; }
    }

    class WallStateObject : StateObject {
        float hp;
        
        public WallStateObject(float hp) { this.hp = hp; }

        public void SetHp(float hp) { this.hp = hp; }
        public float GetHp() { return hp; }

        override public bool IsEnemy() { return false; }
        override public bool IsFood() { return false; }
        override public bool IsWall() { return true; }
        override public bool IsExit() { return false; }
    }

    class ExitStateObject : StateObject { 
        override public bool IsEnemy() { return false; }
        override public bool IsFood() { return false; }
        override public bool IsWall() { return false; }
        override public bool IsExit() { return true; }
    }

    class Node {
        int x;
        int y;
        float reward;
        StateObject[, ] state;
        int visited;
        List<Node> childs;
        Node parent;
                
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

    private StateObject[, ] NextState(StateObject[, ] state, int playerX, int playerY, int xDir, int yDir, out int x, out int y, out float reward) {
        StateObject[, ] nextState = state;
        reward = -1f;

        StateObject obj = state[playerX + xDir, playerY + yDir];
        // nothing is here
        if (obj == null) {
            x = playerX + xDir;
            y = playerY + yDir;
            reward += MoveEnemies(nextState, playerX, playerY);    
        }
        // exit is here
        else if (obj.IsExit ()) {
            x = playerX + xDir;
            y = playerY + yDir;
        }
        // food is here
        else if (obj.IsFood ()) {
            x = playerX + xDir;
            y = playerY + yDir;
            reward += ((FoodStateObject)obj).GetAmount ();
            reward += MoveEnemies(nextState, playerX, playerY);
        }
        // wall is here
        else if (obj.IsWall ()) {
            WallStateObject wall = (WallStateObject)obj;
            if (wall.GetHp () == 0) {
                
            }        
            x = playerX + xDir;
            y = playerY + yDir;
            reward += MoveEnemies(nextState, playerX, playerY);
        }
        // enemy is here
        else if (obj.IsEnemy ()) {
            x = playerX;
            y = playerY;
            reward += MoveEnemies(nextState, playerX, playerY);
        }
        else {
            x = playerX;
            y = playerY;
            reward += MoveEnemies(nextState, playerX, playerY);
        }       

        return nextState;
    }

    private float MoveEnemies(ref StateObject[, ] state, int playerX, int playerY) {
        float penalty = 0f;         

        for (int i = 0; i < Utils.SIZE_X; i++) {
            for (int j = 0; j < Utils.SIZE_Y; j++) {
                float distance = Utils.GetManhattenDistance (new Vector2(i, j), new Vector2(playerX, playerY));

                if (state[i, j].IsEnemy () && distance == 1f) {
                    penalty -= ((EnemyStateObject)state[i, j]).GetPower ();
                }
                else if (state[i, j].IsEnemy () && distance == 2f) {
                    MoveEnemy (state, playerX, playerY, i, j);
                }
            }   
        }
    }
    
    private void MoveEnemy(ref StateObject[, ] state, int playerX, int playerY, int enemyX, int enemyY) {
        Vector2 playerPos = new Vector2(playerX, playerY);
        Vector2 enemyPos = new Vector2(enemyX, enemyY);
        EnemyStateObject enemy = (EnemyStateObject)state[enemyX, enemyY];

        foreach (Vector2 point in Utils.POINTS_AROUND) {
            Vector2 newPos = enemyPos + point; 
            
            if (Utils.OnMap(newPos.x, newPos.y) && state[newPos.x, newPos.y] == null) {
                state[newPos.x, newPos.y] = enemy;
                state[enemyX, enemyY] = null;
            }
        }
    }    
}
*/