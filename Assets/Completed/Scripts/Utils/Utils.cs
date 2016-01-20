using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;

public static class Utils {
    /*public static List<MovingObject> GetEnemies () {
        
    }*/
    
    public static GameObject GetPlayerGameObject () {
        return GameObject.FindGameObjectWithTag ("Player");
    }

    public static Player GetPlayer () {
        return GetPlayerGameObject ().GetComponent <Player> ();
    }

    public static Transform GetPlayerTransform () {
        return GetPlayerGameObject ().GetComponent <Transform> ();
    }
}
