using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;
using System.Linq;

public static class Utils {
    static int SIZE_X = 8;
    static int SIZE_Y = 8;
    static string ENEMY_TAG = "Enemy";
    static string FOOD_TAG = "Food";
    static string WALL_TAG = "Wall";
    static string PLAYER_TAG = "Player";
    static string EXIT_TAG = "Exit";
    static float ONE_STEP_PENALTY = -1f;
    static float HUGE_PENALTY = -1e6f; 
    static float FOOD_INCREASE = 20f;

    public static GameObject[] GetEnemiesGameObjects () {
        return GameObject.FindGameObjectsWithTag (ENEMY_TAG);
    }

    public static GameObject[] GetFoodGameObjects () {
        return GameObject.FindGameObjectsWithTag (FOOD_TAG);
    }

    public static GameObject[] GetWallGameObjects () {
        return GameObject.FindGameObjectsWithTag (WALL_TAG);
    }
    
    public static GameObject GetPlayerGameObject () {
        return GameObject.FindGameObjectWithTag (PLAYER_TAG);
    }

    private static GameObject GetExitGameObject () {
        return GameObject.FindGameObjectWithTag (EXIT_TAG);
    }

    public static Player GetPlayer () {
        return GetPlayerGameObject ().GetComponent <Player> ();
    }

    public static Transform GetPlayerTransform () {
        return GetPlayerGameObject ().GetComponent <Transform> ();
    }

    public static Vector2 GetExitPosition () {
        return new Vector2(GetExitGameObject ().transform.position.x, GetExitGameObject ().transform.position.y);
    }

    public static GameObject[, ] GetMap () {
        GameObject[, ] map = new GameObject[SIZE_X, SIZE_Y];        

        GameObject[] enemies = GetEnemiesGameObjects ();
        GameObject[] foods = GetFoodGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        GameObject player = GetPlayerGameObject ();
        GameObject exit = GetExitGameObject ();
        
        foreach (GameObject enemy in enemies) {
            map[(int)enemy.transform.position.x, (int)enemy.transform.position.y] = enemy;           
        }

        foreach (GameObject food in foods) {
            map[(int)food.transform.position.x, (int)food.transform.position.y] = food;           
        }

        foreach (GameObject wall in walls) {
            map[(int)wall.transform.position.x, (int)wall.transform.position.y] = wall;           
        }
        
        map[(int)player.transform.position.x, (int)player.transform.position.y] = player;
        
        map[(int)exit.transform.position.x, (int)exit.transform.position.y] = exit;        

        return map; 
    }
    
    public static float[, ] GetMapWeights () {
        float [, ] weights = new float[SIZE_X, SIZE_Y];
        GameObject[, ] map = GetMap ();        
        
        for (int i = 0; i < SIZE_X; i++) {
            for (int j = 0; i < SIZE_Y; j++) {
                GameObject obj = map[i, j]; 
                
                if (obj == null) { weights[i, j] = ONE_STEP_PENALTY + ZombiesPenalty (map, i, j); }
                else if (IsEnemy (obj)) { weights[i, j] = HUGE_PENALTY; }
                else if (IsFood (obj)) { weights[i, j] = ONE_STEP_PENALTY + FOOD_INCREASE + ZombiesPenalty (map, i, j); }
                else if (IsWall (obj)) { weights[i, j] = ONE_STEP_PENALTY + -1 * (float)obj.GetComponent<Wall> ().hp + ZombiesPenalty (map, i, j); }
                else if (IsExit (obj)) { weights[i, j] = ONE_STEP_PENALTY; }
                else weights[i, j] = 0f;
            } 
        }
        return null;
    }

    public static float ZombiesPenalty (GameObject[, ] map, int i, int j) {
        float penalty = 0;    
        Debug.Log ("here");
        Vector2[] pointsAround = new Vector2[]{ 
            new Vector2(i - 1, j - 1), new Vector2(i - 1, j),
            new Vector2(i - 1, j + 1), new Vector2(i, j - 1),
            new Vector2(i, j + 1), new Vector2(i + 1, j - 1),
            new Vector2(i + 1, j), new Vector2(i + 1, j + 1) };
        
        foreach (Vector2 point in pointsAround) {
            if (OnMap((int)point.x, (int)point.y)) {
                GameObject obj = map[(int)point.x, (int)point.y];
                if (obj != null && IsEnemy (obj)) {
                    penalty += (float)obj.GetComponent<Enemy> ().playerDamage;
                }
            }
        }
        
        return penalty;
    }
    
    public static bool OnMap(int i, int j) { return 0 <= i && i < SIZE_X && 0 <= j && j < SIZE_Y; }
    public static bool IsEnemy (GameObject obj) { return obj.tag == ENEMY_TAG; }  
    public static bool IsFood (GameObject obj) { return obj.tag == FOOD_TAG; }
    public static bool IsWall (GameObject obj) { return obj.tag == WALL_TAG; }
    public static bool IsPlayer (GameObject obj) { return obj.tag == PLAYER_TAG; }
    public static bool IsExit (GameObject obj) { return obj.tag == EXIT_TAG; }

    public static void PrintMatrix () {
        float[, ] m = GetMapWeights ();
        int rowLength = m.GetLength(0);
        int colLength = m.GetLength(1);
        
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                Debug.Log (string.Format("{0} ", m[i, j]));
            }
        }
    }
}
