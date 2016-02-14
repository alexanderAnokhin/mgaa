using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;

public class EnemyController {

    public IEnumerator MoveEnemies(List<Enemy> enemies) {
        /*
         * do collective brain here, decide how to move the properly, i.e. decide xDir-yDir for every enemy
         */

        //Loop through List of Enemy objects.
        foreach (Enemy enemy in enemies) {
            System.Random rnd = new System.Random();

            Vector2 move = Utils.POINTS_AROUND[rnd.Next(0, 4)];
            //Call the MoveEnemy function of Enemy at index i in the enemies List.
            enemy.MoveEnemy ((int)move.x, (int)move.y); // stupid example, always move them into 0-1 direction
            
            //Wait for Enemy's moveTime before moving next Enemy, 
            yield return new WaitForSeconds(enemy.moveTime);
        }
    }
}
