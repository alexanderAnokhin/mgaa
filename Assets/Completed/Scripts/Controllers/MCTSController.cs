using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;
using System.Linq;
using System.Diagnostics;
using System;

public class MCTSController : Controller {
    public static float C = 10f;
    public static float ONE_STEP_C = 1.5f;
    public static float EXIT_REWARD = 200f;
    public static float DIE_REWARD = -100f;

    override public void Move (out int xDir, out int yDir) {
        Utils.PlotMatrix(1f);
        
        GameObject player = Utils.GetPlayerGameObject ();
        Vector2 playerPos = (Vector2)player.transform.position;
        
        if (OnExit(playerPos)) { 
            xDir = 0; yDir = 0;
        }
        else {
            StateObject[, ] state0 = CreateInitialState();
            
            List<Vector2> route = MCTS((int)playerPos.x, (int)playerPos.y, state0, out xDir, out yDir);
            
            Vector2 move = route.First();
            Utils.PlotRoute(route, 1);
        }        
    }
    
    private StateObject[, ] CreateInitialState() {
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

    private List<Vector2> MCTS(int x, int y, StateObject[, ] state0, out int xDir, out int yDir) {
        MCTSNode root = new MCTSNode(x, y, 0f, state0, null, false);
        
        int sim = 0;
        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < TimeSpan.FromMilliseconds(500)) 
        {
            root.MakeDecision(); sim++;
        }        
        s.Stop();        

        UnityEngine.Debug.Log("simulated " + sim);

        List<Vector2> route = GetMostVisitedRoute(root);

        Vector2 nextPos = GetNextPosition(route, new Vector2(x, y));

        xDir = x - (int)nextPos.x; 
        yDir = y - (int)nextPos.y;

        return GetMostVisitedRoute(root);
    }

    public static StateObject[, ] NextState(StateObject[, ] state, int x, int y, int xDir, int yDir, out int newX, out int newY, out float reward, out bool isTerminal) {
        Vector2 pos = new Vector2(x, y);
        Vector2 exitPos = new Vector2(7, 7);    
    
        StateObject[, ] nextState;
        reward = -1f;
        isTerminal = false;

        StateObject obj = state[x + xDir, y + yDir];
        // nothing is here, can move
        if (obj == null) {
            newX = x + xDir;
            newY = y + yDir;
            nextState = MoveEnemies(state, newX, newY, ref reward);    
        }
        // enemy is here, cannot move
        else if (obj.IsEnemy ()) {
            newX = x;
            newY = y;
            nextState = MoveEnemies(state, newX, newY, ref reward);
        }
        // exit is here, can move and terminal
        else if (obj.IsExit ()) {
            newX = x + xDir;
            newY = y + yDir;
            nextState = MoveEnemies(state, newX, newY, ref reward);
            isTerminal = true;
            reward += EXIT_REWARD;
        }
        // food is here, can move
        else if (obj.IsFood ()) {
            newX = x + xDir;
            newY = y + yDir;
            reward += (obj as FoodStateObject).GetAmount ();
            state[newX, newY] = null;
            nextState = MoveEnemies(state, newX, newY, ref reward);
        }
        // wall is here
        else if (obj.IsWall ()) {
            WallStateObject wall = (obj as WallStateObject);
            // can destroy and move
            if (wall.GetHp () == 1f) {
                //UnityEngine.Debug.Log("can destroy!");
                newX = x + xDir;
                newY = y + yDir;
                state[newX, newY] = null;
                nextState = MoveEnemies(state, newX, newY, ref reward);
            }
            // cannot destroy and move
            else {
                UnityEngine.Debug.Log("cannot destroy!");
                newX = x;
                newY = y;
                state[x + xDir, y + yDir] = wall.Attacked();
                nextState = MoveEnemies(state, x, y, ref reward);  
            }
        }
        // should never happen
        else {
            newX = x;
            newY = y;
            nextState = MoveEnemies(state, x, y, ref reward);
        }       

        if (Utils.GetManhattenDistance(pos, exitPos) < Utils.GetManhattenDistance(new Vector2(newX, newY), exitPos)) { reward -= 1f; }
        else { reward += 1f; }

        return nextState;
    }

    public static StateObject[, ] MoveEnemies(StateObject[, ] state, int x, int y, ref float reward) {
        for (int i = 0; i < Utils.SIZE_X; i++) {
            for (int j = 0; j < Utils.SIZE_Y; j++) {
                float distance = Utils.GetManhattenDistance (new Vector2(i, j), new Vector2(x, y));

                if (state[i, j] != null && state[i, j].IsEnemy () && distance == 1f) {
                    reward -= (state[i, j] as EnemyStateObject).GetPower();
                }
                else if (state[i, j] != null && state[i, j].IsEnemy () && 2f <= distance && distance <= 4f) {
                    state = MoveEnemy(state, x, y, i, j);
                }
            }   
        }

        return state;
    }
    
    public static StateObject[, ] MoveEnemy(StateObject[, ] state, int playerX, int playerY, int enemyX, int enemyY) {
        Vector2 playerPos = new Vector2(playerX, playerY);
        Vector2 enemyPos = new Vector2(enemyX, enemyY);
        EnemyStateObject enemy = (state[enemyX, enemyY] as EnemyStateObject);
        float oldDistance = Utils.GetManhattenDistance(playerPos, enemyPos); 

        foreach (Vector2 point in Utils.POINTS_AROUND) {
            Vector2 newEnemyPos = enemyPos + point;
            float newDistance = Utils.GetManhattenDistance (playerPos, newEnemyPos);
            
            if (Utils.OnMap((int)newEnemyPos.x, (int)newEnemyPos.y) && state[(int)newEnemyPos.x, (int)newEnemyPos.y] == null && newDistance <= oldDistance) {
                state[(int)newEnemyPos.x, (int)newEnemyPos.y] = enemy;
                state[enemyX, enemyY] = null;
            }
        }

        return state;
    }    
    
    private List<Vector2> GetMostVisitedRoute(MCTSNode root) {
        List<Vector2> route = new List<Vector2> ();
        MCTSNode current = root;
        
        while (!current.IsTerminalOrNoChilds()) {
            route.Add (current.GetVector());
            current = current.GetMostVisitedChild();
        }
        
        route.Add (current.GetVector());
        
        return route;
    }

    private Vector2 GetNextPosition(List<Vector2> route, Vector2 playerPos) {
        IEnumerable<Vector2> notCurrentPos = route.SkipWhile(p => p == playerPos);
        UnityEngine.Debug.Log(notCurrentPos.First());
        return notCurrentPos.Count() == 0 ? playerPos : notCurrentPos.First();
    }

    private bool OnExit(Vector2 playerPos) {
        return playerPos == Utils.GetExitPosition();    
    }                    
}
