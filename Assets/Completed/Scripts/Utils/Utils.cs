using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;
using System.Linq;

public static class Utils {
    public static Vector2[] POINTS_AROUND = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) }; 
    public static int SIZE_X = 8;
    public static int SIZE_Y = 8;
    public static string ENEMY_TAG = "Enemy";
    public static string FOOD_TAG = "Food";
    public static string SODA_TAG = "Soda";
    public static string WALL_TAG = "Wall";
    public static string FLOOR_TAG = "Floor";
    public static string PLAYER_TAG = "Player";
    public static string EXIT_TAG = "Exit";
    public static float ONE_STEP_PENALTY = -1f;
    public static float HUGE_PENALTY = -1e6f; 
    public static float FOOD_INCREASE = 10f;
    public static float SODA_INCREASE = 20f;

    public static GameObject[] GetEnemiesGameObjects () {
        return GameObject.FindGameObjectsWithTag (ENEMY_TAG);
    }

    public static GameObject[] GetFoodGameObjects () {
        return GameObject.FindGameObjectsWithTag (FOOD_TAG);
    }

    public static GameObject[] GetSodaGameObjects () {
        return GameObject.FindGameObjectsWithTag (SODA_TAG);
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

    private static GameObject[] GetFloorGameObjects () {
        return GameObject.FindGameObjectsWithTag (FLOOR_TAG);
    }

    public static Player GetPlayer () {
        return GetPlayerGameObject ().GetComponent <Player> ();
    }

    public static Transform GetPlayerTransform () {
        return GetPlayerGameObject ().GetComponent <Transform> ();
    }

    public static Vector2 GetPlayerPosition () {
        return (Vector2)GetPlayerTransform ().position;
    }
    
    public static Vector2 GetExitPosition () {
        return (Vector2)GetExitGameObject ().transform.position;
    }

    public static GameObject[, ] GetMap () {
        GameObject[, ] map = new GameObject[SIZE_X, SIZE_Y];        

        GameObject[] enemies = GetEnemiesGameObjects ();
        GameObject[] foods = GetFoodGameObjects ();
        GameObject[] sodas = GetSodaGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        GameObject[] floors = GetFloorGameObjects();
        GameObject player = GetPlayerGameObject ();
        GameObject exit = GetExitGameObject ();

        foreach (GameObject floor in floors)
        {
            map[(int)floor.transform.position.x, (int)floor.transform.position.y] = floor;
        }

        foreach (GameObject food in foods) {
            map[(int)food.transform.position.x, (int)food.transform.position.y] = food;           
        }
        
        foreach (GameObject soda in sodas) {
            map[(int)soda.transform.position.x, (int)soda.transform.position.y] = soda;           
        }

        foreach (GameObject wall in walls) {
            map[(int)wall.transform.position.x, (int)wall.transform.position.y] = wall;           
        }

        foreach (GameObject enemy in enemies) {
            map[(int)enemy.transform.position.x, (int)enemy.transform.position.y] = enemy;           
        }
        
        map[(int)player.transform.position.x, (int)player.transform.position.y] = player;
        
        map[(int)exit.transform.position.x, (int)exit.transform.position.y] = exit;        

        return map; 
    }
    
    public static float[, ] GetMapWeights () {
        float [, ] weights = new float[SIZE_X, SIZE_Y];
        GameObject[, ] map = GetMap ();        
        
        for (int i = 0; i < SIZE_X; i++) {
            for (int j = 0; j < SIZE_Y; j++) {
                GameObject obj = map[i, j]; 
                
                if (obj == null) { weights[i, j] = ONE_STEP_PENALTY + ZombiesPenalty (map, i, j); }
                else if (IsPlayer (obj)) { weights[i, j] = -1f; }
                else if (IsEnemy (obj)) { weights[i, j] = HUGE_PENALTY; }
                else if (IsFood (obj)) { weights[i, j] = ONE_STEP_PENALTY + FOOD_INCREASE + ZombiesPenalty (map, i, j); }
                else if (IsSoda (obj)) { weights[i, j] = ONE_STEP_PENALTY + SODA_INCREASE + ZombiesPenalty (map, i, j); }
                else if (IsWall (obj)) {
                    //TODO sometimes not working search by tag 
                    try { 
                        weights[i, j] = -1f * (float)obj.GetComponent<Wall> ().hp + ZombiesPenalty (map, i, j); 
                    } catch {
                        Debug.Log ("Exception!");
                        weights[i, j] = -4f + ZombiesPenalty (map, i, j);
                    }
                }
                else if (IsExit (obj)) { weights[i, j] = ONE_STEP_PENALTY; }
                else weights[i, j] = 0f;
            } 
        }
        return weights;
    }

    public static float ZombiesPenalty (GameObject[, ] map, int i, int j) {
        float penalty = 0;    
        
        Vector2[] pointsAround = new Vector2[]{ 
            new Vector2(i - 1, j - 1), new Vector2(i - 1, j),
            new Vector2(i - 1, j + 1), new Vector2(i, j - 1),
            new Vector2(i, j + 1), new Vector2(i + 1, j - 1),
            new Vector2(i + 1, j), new Vector2(i + 1, j + 1) };
        
        foreach (Vector2 point in pointsAround) {
            if (OnMap((int)point.x, (int)point.y)) {
                GameObject obj = map[(int)point.x, (int)point.y];
                if (obj != null && IsEnemy (obj)) {
                    penalty -= (float)obj.GetComponent<Enemy> ().playerDamage;
                }
            }
        }
        
        return penalty;
    }
    
    public static void PlotMatrix () {
        GameObject[] floors = GetFloorGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        float[, ] m = GetMapWeights ();
        
        foreach (GameObject floor in floors) {
            Transform transform = floor.GetComponent<Transform> ();
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (OnMap (x, y)) {
                SpriteRenderer renderer = floor.GetComponent<SpriteRenderer> ();
                if (0f <= m[x, y]) { renderer.material.SetColor("_Color", Color.green); }
                else if (ONE_STEP_PENALTY <= m[x, y] && m[x, y] < 0f) { renderer.material.SetColor("_Color", Color.cyan); }
                else if (15f * ONE_STEP_PENALTY <= m[x, y] && m[x, y] < ONE_STEP_PENALTY) { renderer.material.SetColor("_Color", Color.yellow); }
                else { renderer.material.SetColor("_Color", Color.red); }
            }
        }

        foreach (GameObject wall in walls) {
            Transform transform = wall.GetComponent<Transform> ();
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (OnMap (x, y)) {
                SpriteRenderer renderer = wall.GetComponent<SpriteRenderer> ();
                if (0f <= m[x, y]) { renderer.material.SetColor("_Color", Color.green); }
                else if (ONE_STEP_PENALTY <= m[x, y] && m[x, y] < 0f) { renderer.material.SetColor("_Color", Color.cyan); }
                else if (15f * ONE_STEP_PENALTY <= m[x, y] && m[x, y] < ONE_STEP_PENALTY) { renderer.material.SetColor("_Color", Color.yellow); }
                else { renderer.material.SetColor("_Color", Color.red); }
            }
        }
    }

    public static void PlotRoute (List<Vector2> route) {
        GameObject[] floors = GetFloorGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        GameObject exit = GetExitGameObject ();
        Vector2 exitPosition = GetExitPosition ();
        
        foreach (GameObject floor in floors) {
            Transform transform = floor.GetComponent<Transform> ();
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (OnMap (x, y) && route.Exists (p => p == (Vector2)transform.position)) {
                SpriteRenderer renderer = floor.GetComponent<SpriteRenderer> ();
                renderer.material.SetColor("_Color", Color.gray);
            }
        }
        
        foreach (GameObject wall in walls) {
            Transform transform = wall.GetComponent<Transform> ();
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (OnMap (x, y) && route.Exists (p => p == (Vector2)transform.position)) {
                SpriteRenderer renderer = wall.GetComponent<SpriteRenderer> ();
                renderer.material.SetColor("_Color", Color.gray);
            }
        }

        if (OnMap ((int)exitPosition.x, (int)exitPosition.y) && route.Exists (p => p == exitPosition)) {
            SpriteRenderer renderer = exit.GetComponent<SpriteRenderer> ();
            renderer.material.SetColor("_Color", Color.gray);
        }
    }

    public static bool OnMap(int i, int j) { return 0 <= i && i < SIZE_X && 0 <= j && j < SIZE_Y; }
    public static bool IsEnemy (GameObject obj) { return obj.tag == ENEMY_TAG; }  
    public static bool IsFood (GameObject obj) { return obj.tag == FOOD_TAG; }
    public static bool IsSoda (GameObject obj) { return obj.tag == SODA_TAG; }
    public static bool IsWall (GameObject obj) { return obj.tag == WALL_TAG; }
    public static bool IsPlayer (GameObject obj) { return obj.tag == PLAYER_TAG; }
    public static bool IsExit (GameObject obj) { return obj.tag == EXIT_TAG; }

    public static float GetManhattenDistance (Vector2 X, Vector2 Y) {
        return Mathf.Abs (X.x - Y.x) + Mathf.Abs (X.y - Y.y);       
    }
    
    public static float GetEuclidianDistance (Vector2 X, Vector2 Y) {
        return Mathf.Pow (X.x - Y.x, 2) + Mathf.Pow (X.y - Y.y, 2); 
    }
}
