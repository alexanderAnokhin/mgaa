using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;
using System.Linq;
using System.Diagnostics;
using System;

public class MCTSController : Controller {
    public static float C = 10f;            // Exploration constant
    public static float ONE_STEP_C = 1f;    // Approximates costs of one step
    public static float FOOD_C = 0.1f;      // Inflates food increase
    public static float EXIT_REWARD = 50f;  // Reward for finishin level
    public static float DIE_REWARD = -50f;  // Limit for diying
    
    // Main function which moves player unit
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
            
            Utils.PlotRoute(route, 1);
        }        
    }
    
    // Creates Initial state
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
    
    // Main cycle of search
    private List<Vector2> MCTS(int x, int y, StateObject[, ] state0, out int xDir, out int yDir) {
        MCTSNode root = new MCTSNode(x, y, 0f, state0, null, false);
        
        Stopwatch s = new Stopwatch();
        int sim = 0;

        s.Start();

        while (s.Elapsed < TimeSpan.FromSeconds((double)Player.GetInstance().decisionDelay * 0.8d)) { root.MakeVisit(); sim ++; }        
        UnityEngine.Debug.Log("Simulations: " + sim);
        s.Stop();        

        List<Vector2> route = GetMostVisitedRoute(root);

        Vector2 nextPos = GetNextPosition(route, new Vector2(x, y));
    
        xDir = (int)nextPos.x - x; 
        yDir = (int)nextPos.y - y;

        return GetMostVisitedRoute(root);
    }

    // Function brings a new state (advance model)
    public static StateObject[, ] NextState(StateObject[, ] state, int x, int y, int xDir, int yDir, out int newX, out int newY, out float reward, out bool isTerminal) {
        Vector2 pos = new Vector2(x, y);    
    
        StateObject[, ] nextState = state;
        reward = 0f;
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
            newX = x + xDir;
            newY = y + yDir;
            reward -= (obj as WallStateObject).GetHp();
            state[newX, newY] = null;
            nextState = MoveEnemies(state, newX, newY, ref reward);            
        }
        // should never happen
        else {
            newX = x;
            newY = y;
            nextState = MoveEnemies(state, x, y, ref reward);
        }       

        // Add heuristics 
        reward += FoodHeuristic(nextState, newX, newY) + ExitHeuristic(nextState, newX, newY);
        
        return nextState;
    }
    
    // Moves enemies if player is in range
    public static StateObject[, ] MoveEnemies(StateObject[, ] state, int x, int y, ref float reward) {
        // Enemies are attacking player
        for (int i = 0; i < Utils.SIZE_X; i++) {
            for (int j = 0; j < Utils.SIZE_Y; j++) {
                float distance = Utils.GetManhattenDistance (new Vector2(i, j), new Vector2(x, y));

                if (state[i, j] != null && state[i, j].IsEnemy () && distance == 1f) {
                    reward -= (state[i, j] as EnemyStateObject).GetPower();
                }
            }   
        }

        // Enemies are moving towards player
        List<Vector2> enemiesToMove = new List<Vector2> ();

        for (int i = 0; i < Utils.SIZE_X; i++) {
            for (int j = 0; j < Utils.SIZE_Y; j++) {
                float distance = Utils.GetManhattenDistance (new Vector2(i, j), new Vector2(x, y));
                
                if (state[i, j] != null && state[i, j].IsEnemy () && distance == 3f) {
                    enemiesToMove.Add(new Vector2(i, j));
                }
            }   
        }

        // Move moving enemies
        for (int i = 0; i < enemiesToMove.Count(); i++) {
            Vector2 pos = enemiesToMove.ElementAt(i); 
            state = MoveEnemy(state, x, y, (int)pos.x, (int)pos.y);
        }
        
        return state;
    }
    
    // Moves enemy towards player unit, here not all possibilities are considered
    public static StateObject[, ] MoveEnemy(StateObject[, ] state, int playerX, int playerY, int enemyX, int enemyY) {
        Vector2 playerPos = new Vector2(playerX, playerY);
        Vector2 enemyPos = new Vector2(enemyX, enemyY);
        EnemyStateObject enemy = (state[enemyX, enemyY] as EnemyStateObject);
        float oldDistance = Utils.GetManhattenDistance(playerPos, enemyPos); 

        foreach (Vector2 point in Utils.POINTS_AROUND) {
            Vector2 newEnemyPos = enemyPos + point;
            float newDistance = Utils.GetManhattenDistance (playerPos, newEnemyPos);
            
            if (
                Utils.OnMap((int)newEnemyPos.x, (int)newEnemyPos.y) &&      // New position should be on map
                !((int)newEnemyPos.x == 7 && (int)newEnemyPos.y == 7) &&    // Should not block exit
                state[(int)newEnemyPos.x, (int)newEnemyPos.y] == null &&    // Nothing is here
                newDistance <= oldDistance) {                               // Movement toward player
                state[(int)newEnemyPos.x, (int)newEnemyPos.y] = enemy;
                state[enemyX, enemyY] = null;
                return state;
            }
        }

        return state;
    }    
    
    // Gets route that consists of most visited nodes in search tree
    private List<Vector2> GetMostVisitedRoute(MCTSNode root) {
        List<Vector2> route = new List<Vector2> ();
        MCTSNode current = root;
        
        while (!current.IsTerminal() && !current.IsNotExpanded()) {
            route.Add (current.GetVector());
            current = current.GetMostVisitedChild();
        }
        
        route.Add (current.GetVector());
        
        return route;
    }
    
    // Gets next position in route after player position
    private Vector2 GetNextPosition(List<Vector2> route, Vector2 playerPos) {
        List<Vector2> notCurrentPos = route.SkipWhile(p => p == playerPos).ToList();
        return notCurrentPos.Count == 0 ? playerPos : notCurrentPos.First();
    }

    // Returns true if exit is reached and false otherwise
    private bool OnExit(Vector2 playerPos) { return playerPos == Utils.GetExitPosition(); }

    // Gets the heuristic which shows how well player is placed in terms of food
    public static float FoodHeuristic(StateObject[, ] state, int x, int y) {
        float reward = 0f;
        
        for (int i = 0; i < state.GetLength(0); i++) {
            for (int j = 0; j < state.GetLength(1); j++) {
                if (state[i, j] != null && state[i, j].IsFood()) {
                    // Zero if there is no sense to go there 
                    reward += FOOD_C * Mathf.Max(0f, (state[i, j] as FoodStateObject).GetAmount() - ONE_STEP_C * Utils.GetManhattenDistance(new Vector2(x, y), new Vector2(i, j))); 
                }
            }
        }
        
        return reward;
    }

    // Gets the heuristic which shows how well player is placed in terms of exit
    public static float ExitHeuristic(StateObject[, ] state, int x, int y) {
        for (int i = 0; i < state.GetLength(0); i++) {
            for (int j = 0; j < state.GetLength(1); j++) {
                if (state[i, j] != null && state[i, j].IsExit()) { 
                    return -ONE_STEP_C * Utils.GetManhattenDistance(new Vector2(x, y), new Vector2(i, j)); 
                }
            }
        }
        
        return -ONE_STEP_C * Utils.GetManhattenDistance(new Vector2(x, y), new Vector2(Utils.SIZE_X, Utils.SIZE_Y));
    }                    
}
