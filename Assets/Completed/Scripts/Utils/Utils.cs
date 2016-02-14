using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;
using System.Linq;

public static class Utils {
    public static Vector2[] POINTS_AROUND = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) }; 
    public static int SIZE_X = 8;
    public static int SIZE_Y = 8;
    // Objects tags for scoping
    public static string ENEMY_TAG = "Enemy";
    public static string FOOD_TAG = "Food";
    public static string SODA_TAG = "Soda";
    public static string WALL_TAG = "Wall";
    public static string FLOOR_TAG = "Floor";
    public static string PLAYER_TAG = "Player";
    public static string EXIT_TAG = "Exit";
    // Predefined values
    public static float ONE_STEP_PENALTY = -1f;
    public static float HUGE_PENALTY = -1e6f; 
    public static float FOOD_INCREASE = 10f;
    public static float SODA_INCREASE = 20f;

    // Various scope
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

    public static GameObject GetExitGameObject () {
        return GameObject.FindGameObjectWithTag (EXIT_TAG);
    }

    public static GameObject[] GetFloorGameObjects () {
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

    // Map of game objects
    public static GameObject[, ] GetMap () {
        GameObject[, ] map = new GameObject[SIZE_X, SIZE_Y];        
        
        GameObject[] enemies = GetEnemiesGameObjects ();
        GameObject[] foods = GetFoodGameObjects ();
        GameObject[] sodas = GetSodaGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        GameObject[] floors = GetFloorGameObjects ();
        GameObject player = GetPlayerGameObject ();
        GameObject exit = GetExitGameObject ();

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

    // Map of game objects with floor
    public static GameObject[, ] GetMapWithFloor () {
        GameObject[, ] map = new GameObject[SIZE_X, SIZE_Y];        

        GameObject[] enemies = GetEnemiesGameObjects ();
        GameObject[] foods = GetFoodGameObjects ();
        GameObject[] sodas = GetSodaGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        GameObject[] floors = GetFloorGameObjects ();
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
    
    // Influence map
    public static float[, ] GetMapWeights (float shift) {
        float [, ] weights = new float[SIZE_X, SIZE_Y];
        GameObject[, ] map = GetMap ();        
        
        for (int i = 0; i < SIZE_X; i++) {
            for (int j = 0; j < SIZE_Y; j++) {
                GameObject obj = map[i, j]; 
                
                if (obj == null) { weights[i, j] = ONE_STEP_PENALTY + ZombiesPenalty (map, i, j, shift); }
                else if (IsPlayer (obj)) { weights[i, j] = -1f; }
                else if (IsEnemy (obj)) { weights[i, j] = HUGE_PENALTY; }
				else if (IsFood (obj)) { weights[i, j] = ONE_STEP_PENALTY + FOOD_INCREASE + ZombiesPenalty (map, i, j, shift); }
				else if (IsSoda (obj)) { weights[i, j] = ONE_STEP_PENALTY + SODA_INCREASE + ZombiesPenalty (map, i, j, shift); }
                else if (IsWall (obj)) {
                    //TODO sometimes not working search by tag 
                    try { 
                        weights[i, j] = -1f * (float)obj.GetComponent<Wall> ().hp + ZombiesPenalty (map, i, j, shift); 
                    } catch {
                        Debug.Log ("Exception!");
						weights[i, j] = -4f + ZombiesPenalty (map, i, j, shift);
                    }
                }
                else if (IsExit (obj)) { weights[i, j] = ONE_STEP_PENALTY; }
                else weights[i, j] = 0f;
            } 
        }
        return weights;
    }

    // Influence of zombies
	public static float ZombiesPenalty (GameObject[, ] map, int x, int y, float shift) {
        float penalty = 0;

		GameObject[] enemies = GetEnemiesGameObjects ();    
        
		foreach (GameObject enemy in enemies) {
            Vector2 enemyPosition = (Vector2)enemy.GetComponent <Transform> ().position;
            float distance = GetManhattenDistance (new Vector2 (x, y), enemyPosition);
            if  (distance <= shift) {
                penalty -= (float)enemy.GetComponent<Enemy> ().playerDamage / distance;
            }
		}

        return penalty;
    }
    
    // Plots influence map
    public static void PlotMatrix (float shift) {
        GameObject[] floors = GetFloorGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        float[, ] m = GetMapWeights (shift);
        
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

    // Plots route
    public static void PlotRoute (List<Vector2> route, int colorType) {
        GameObject[] floors = GetFloorGameObjects ();
        GameObject[] walls = GetWallGameObjects ();
        GameObject exit = GetExitGameObject ();
        Vector2 exitPosition = GetExitPosition ();
        
		Color color;

		switch (colorType) {
		case 1:
			color = Color.gray;
			break;
		case 2:
			color = Color.blue;
			break;
		case 3:
			color = Color.magenta;
			break;
		default:
			color = Color.gray;
			break;
		}

        foreach (GameObject floor in floors) {
            Transform transform = floor.GetComponent<Transform> ();
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (OnMap (x, y) && route.Exists (p => p == (Vector2)transform.position)) {
                SpriteRenderer renderer = floor.GetComponent<SpriteRenderer> ();
                renderer.material.SetColor("_Color", color);
            }
        }
        
        foreach (GameObject wall in walls) {
            Transform transform = wall.GetComponent<Transform> ();
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (OnMap (x, y) && route.Exists (p => p == (Vector2)transform.position)) {
                SpriteRenderer renderer = wall.GetComponent<SpriteRenderer> ();
                renderer.material.SetColor("_Color", color);
            }
        }

        if (OnMap ((int)exitPosition.x, (int)exitPosition.y) && route.Exists (p => p == exitPosition)) {
            SpriteRenderer renderer = exit.GetComponent<SpriteRenderer> ();
            renderer.material.SetColor("_Color", color);
        }
    }

    // Various predicates
    public static bool OnMap(int i, int j) { return 0 <= i && i < SIZE_X && 0 <= j && j < SIZE_Y; }
    public static bool IsEnemy (GameObject obj) { return obj.tag == ENEMY_TAG; }  
    public static bool IsFood (GameObject obj) { return obj.tag == FOOD_TAG; }
    public static bool IsSoda (GameObject obj) { return obj.tag == SODA_TAG; }
    public static bool IsWall (GameObject obj) { return obj.tag == WALL_TAG; }
    public static bool IsPlayer (GameObject obj) { return obj.tag == PLAYER_TAG; }
    public static bool IsExit (GameObject obj) { return obj.tag == EXIT_TAG; }

    // Various distances
    public static float GetManhattenDistance (Vector2 X, Vector2 Y) {
        return Mathf.Abs (X.x - Y.x) + Mathf.Abs (X.y - Y.y);       
    }
    
    public static float GetEuclidianDistance (Vector2 X, Vector2 Y) {
        return Mathf.Pow (X.x - Y.x, 2) + Mathf.Pow (X.y - Y.y, 2); 
    }
    
    //Recursive method that checks all fields in sightrange of the player and calls itself recursively for all adjacent fields.
    //Safes visited fields in an array and isn't allowed to visit them again. If there is no more field to visit in the sightrange the method breaks off with an empty array as result.
    //If the method reaches the target, i.e. there is a free path to the target field, then it gives back an array containing the x and y coordinate of the first field of the shortest path and the length of the path.
    public static int[] recursivePath(int x, int y, int currentX, int currentY, int sightRng, int targetX, int targetY, GameObject[,] gamestate, Vector2[] visited, bool ignoreWalls = false, int maxLength = -1)
    {
        Random random = new Random();

        //Debug.Log("In recursivePath");
        int[] best = { -1, -1, Utils.SIZE_X * Utils.SIZE_Y };
        int[] curNext;
        string[] impassable;
        if (!ignoreWalls)
            impassable = new[] { Utils.WALL_TAG, Utils.ENEMY_TAG };
        else
            impassable = new[] { Utils.ENEMY_TAG };
        Vector2[] nextVisited;
        Vector2 current = new Vector2(currentX, currentY);
        int numEqual = 1;

        //***Ground case

        //Target reached
        if (currentX == targetX && currentY == targetY)
        {
            //Debug.Log("reached target with x: "+currentX+" and y: "+currentY);
            //This is a correct path, return first step of this path
            best[0] = currentX;
            best[1] = currentY;
            best[2] = 1;
            return (best);
        }

        //***Check all adjacent fields and if applicable try a path along them recursively

        else
        {
            //Put this current field to the history of visited ones
            nextVisited = new Vector2[visited.Length + 1];
            nextVisited[visited.Length] = current;

            //If x+1 reachable, not visited before, in sightRng of the player and isn't impassable
            if ((currentX + 1) <= Utils.SIZE_X - 1 && !inVectorArray(visited, currentX + 1, currentY) && (currentX + 1) <= (x + sightRng) && System.Array.IndexOf(impassable, gamestate[currentX + 1, currentY].tag) == -1)
            {
                //Debug.Log("Calling new path with x+1");
                //Debug.Log("gamestate.tag: " + gamestate[currentX + 1, currentY].tag);
                //recursively try the path along x+1
                curNext = recursivePath(x, y, currentX + 1, currentY, sightRng, targetX, targetY, gamestate, nextVisited, ignoreWalls, maxLength);
                //Debug.Log("0 of new path: " + curNext[0]+", 1: "+curNext[1]+", 2: "+curNext[2]);
                //There exists a possible path to the target along this field
                if (curNext[0] != -1)
                {
                    //This path is the shortest so far
                    if (curNext[2] < best[2])
                    {
                        //Debug.Log("!!New shortest!!");
                        best = curNext;
                        best[0] = currentX + 1;
                        best[1] = currentY;
                    }
                }
            }
            //If x-1 reachable, not visited before, in sightRng of the player and isn't impassable
            if (currentX - 1 >= 0 && !inVectorArray(visited, currentX - 1, currentY) && (currentX - 1) >= (x - sightRng) && System.Array.IndexOf(impassable, gamestate[currentX - 1, currentY].tag) == -1)
            {
                //Debug.Log("Calling new path with x-1");
                //Debug.Log("gamestate.tag: " + gamestate[currentX - 1, currentY].tag);
                //recursively try the path along x-1
                curNext = recursivePath(x, y, currentX - 1, currentY, sightRng, targetX, targetY, gamestate, nextVisited, ignoreWalls, maxLength);
                //Debug.Log("0 of new path: " + curNext[0] + ", 1: " + curNext[1] + ", 2: " + curNext[2]);
                //There exists a possible path to the target along this field
                if (curNext[0] != -1)
                {
                    //This path is the shortest so far
                    if (curNext[2] < best[2])
                    {
                        //Debug.Log("!!New shortest!!");
                        best = curNext;
                        best[0] = currentX - 1;
                        best[1] = currentY;
                    }
                    //Path is as short as the shortest so far, randomly choose to safe this as shortest
                    else if (curNext[2] == best[2] && Random.Range(0, numEqual) == 0)
                    {
                        //Debug.Log("!!As short as previous, rndly choosed this!!2");
                        best = curNext;
                        best[0] = currentX - 1;
                        best[1] = currentY;
                        numEqual++;
                    }
                }
            }
            //If y+1 reachable, not visited before, in sightRng of the player and isn't impassable
            if ((currentY + 1) <= Utils.SIZE_Y - 1 && !inVectorArray(visited, currentX, currentY + 1) && (currentY + 1) <= (y + sightRng) && System.Array.IndexOf(impassable, gamestate[currentX, currentY + 1].tag) == -1)
            {
                //Debug.Log("Calling new path with y+1");
                //Debug.Log("gamestate.tag: " + gamestate[currentX, currentY + 1].tag);
                //recursively try the path along y+1
                curNext = recursivePath(x, y, currentX, currentY + 1, sightRng, targetX, targetY, gamestate, nextVisited, ignoreWalls, maxLength);
                //Debug.Log("0 of new path: " + curNext[0] + ", 1: " + curNext[1] + ", 2: " + curNext[2]);
                //There exists a possible path to the target along this field
                if (curNext[0] != -1)
                {
                    //This path is the shortest so far
                    if (curNext[2] < best[2])
                    {
                        //Debug.Log("!!New shortest!!");
                        best = curNext;
                        best[0] = currentX;
                        best[1] = currentY + 1;
                    }
                    //Path is as short as the shortest so far, randomly choose to safe this as shortest
                    else if (curNext[2] == best[2] && Random.Range(0, numEqual) == 0)
                    {
                        //Debug.Log("!!As short as previous, rndly choosed this!!3");
                        best = curNext;
                        best[0] = currentX;
                        best[1] = currentY + 1;
                        numEqual++;
                    }
                }
            }
            //If y-1 reachable, not visited before, in sightRng of the player and isn't impassable
            if (currentY - 1 >= 0 && !inVectorArray(visited, currentX, currentY - 1) && (currentY - 1) <= (y - sightRng) && System.Array.IndexOf(impassable, gamestate[currentX, currentY - 1].tag) == -1)
            {
                //Debug.Log("Calling new path with y-1");
                //Debug.Log("gamestate.tag: " + gamestate[currentX, currentY + 1].tag);
                //recursively try the path along y-1
                curNext = recursivePath(x, y, currentX, currentY - 1, sightRng, targetX, targetY, gamestate, nextVisited, ignoreWalls, maxLength);
                //Debug.Log("0 of new path: " + curNext[0] + ", 1: " + curNext[1] + ", 2: " + curNext[2]);
                //There exists a possible path to the target along this field
                if (curNext[0] != -1)
                {
                    //This path is the shortest so far
                    if (curNext[2] < best[2])
                    {
                        //Debug.Log("!!New shortest!!");
                        best = curNext;
                        best[0] = currentX;
                        best[1] = currentY - 1;
                    }
                    //Path is as short as the shortest so far, randomly choose to safe this as shortest
                    else if (curNext[2] == best[2] && Random.Range(0, numEqual) == 0)
                    {
                        //Debug.Log("!!As short as previous, rndly choosed this!!4");
                        best = curNext;
                        best[0] = currentX;
                        best[1] = currentY - 1;
                    }
                }
            }

            //***Check the shortest found path for acceptability

            //A path was found
            if (best[0] != -1)
            {
                //If no maxLength is given or the path does not exceed it
                if (maxLength == -1 || (best[2] + 1) < maxLength)
                {
                    best[2] = best[2] + 1;
                    return (best);
                }
                else
                {
                    best = new[] { -1, -1, Utils.SIZE_X * Utils.SIZE_Y };
                    return (best);
                }
            }
            else
            {
                return (best);
            }
        }
    }

    //Checks wether a field with the coordinates x and y is in an vector array
    public static bool inVectorArray(Vector2[] array, int x, int y)
    {
        foreach (Vector2 item in array)
        {
            if (item.x == x && item.y == y)
                return true;
        }
        return false;
    }

    //Method that calculates if target coordinates are closer than 2 fields to coordinates of an object
    //If the target coordinates are exactly two fields in x and y direction away it gives out false because from there are 3 steps needed to get next to the object
    public static bool is2FieldsClose(int objectX, int objectY, int targetX, int targetY)
    {
        //Target x is in 2 field range to the enemy and target y is in 2 field range to the enemy and they are not both 2 fields away, i.e. on the diagonal
        return ((targetX > (objectX - 2) && targetX < (objectX + 2)) && (targetY > (objectY - 2) && targetY < (objectY + 2)) && !((targetX == (objectX - 2) && targetY == (objectY - 2)) || (targetX == (objectX + 2) && targetY == (objectY + 2))));
    }
}
