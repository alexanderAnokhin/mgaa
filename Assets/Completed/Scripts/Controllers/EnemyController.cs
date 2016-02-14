using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Completed;

public class EnemyController {

    private Transform target;
    private Transform current;
    private string zombieType;
    private int xDir;
    private int yDir;

    public IEnumerator MoveEnemies(List<Enemy> enemies) {
        //Loop through List of Enemy objects.
        foreach (Enemy enemy in enemies) {

            //Set required variables: vectros for target, current and movement positions
            xDir = 0;
            yDir = 0;
            target = Utils.GetPlayerTransform();
            current = enemy.GetComponent<Transform>();

            Vector2 tryMoveDown = new Vector2(current.position.x, current.position.y - 1);
            Vector2 tryMoveUp = new Vector2(current.position.x, current.position.y + 1);
            Vector2 tryMoveLeft = new Vector2(current.position.x - 1, current.position.y);
            Vector2 tryMoveRight = new Vector2(current.position.x + 1, current.position.y);

            //Check the type of zombie. If the type of enemy is 1, then it is Runner Zombie - he tries to catch player by avoiding obstacles
            if (enemy.GetComponent<Animator>().runtimeAnimatorController.name == "Enemy1") {
                zombieType = "Runner";
                //Check if the difference between zombie and player on X axis is lower than the difference on Y axis
                if (Mathf.Abs(target.position.x - current.position.x) < Mathf.Abs(target.position.y - current.position.y)) {
                    //If the difference on Y axis is bigger, we try to make that difference shorter and move up\down to the player
                    //But if there are walls on the way, we try to move zombie on X axis
                    if (!Utils.IsWall(tryMoveDown) && target.position.y < current.position.y || !Utils.IsWall(tryMoveUp) && target.position.y > current.position.y)
                        yDir = target.position.y > current.position.y ? 1 : -1;
                    else
                        xDir = target.position.x > current.position.x ? 1 : -1;
                }
                //If the difference on X axis is bigger, we try to make that difference shorter and move left\right to the player
                //But if there are walls on the way, we try to move zombie on Y axis
                else if (!Utils.IsWall(tryMoveLeft) && target.position.x < current.position.x || !Utils.IsWall(tryMoveRight) && target.position.x > current.position.x)
                    xDir = target.position.x > current.position.x ? 1 : -1;
                else yDir = target.position.y > current.position.y ? 1 : -1;
            }
            //If the type of enemy is 2, then it is Digger Zombie - he tries to catch player by destroying walls
            else if (enemy.GetComponent<Animator>().runtimeAnimatorController.name == "Enemy2") {
                //Check if the difference between zombie and player on X axis is lower than the difference on Y axis
                if (Mathf.Abs(target.position.x - current.position.x) <
                    Mathf.Abs(target.position.y - current.position.y)) {
                    //If the difference on Y axis is bigger, we try to make that difference shorter and move up\down to the player
                    //But if there are walls on the way, we try to move zombie on X axis
                    if (Utils.IsWall(tryMoveDown) && target.position.y < current.position.y ||
                        Utils.IsWall(tryMoveUp) && target.position.y > current.position.y) {
                        zombieType = "Digger";
                    }
                    else {
                        zombieType = "Runner";
                    }
                    yDir = target.position.y > current.position.y ? 1 : -1;
                }
                //If the difference on X axis is bigger, we try to make that difference shorter and move left\right to the player
                //But if there are walls on the way, we try to move zombie on Y axis
                else {
                    if (Utils.IsWall(tryMoveLeft) && target.position.x < current.position.x ||
                        Utils.IsWall(tryMoveRight) && target.position.x > current.position.x) {
                        zombieType = "Digger";
                    }
                    else {
                        zombieType = "Runner";
                    }
                    xDir = target.position.x > current.position.x ? 1 : -1;
                }
            }

            //Set movement vector
            Vector2 move = new Vector2(xDir, yDir);
            //If zombie is near exit and he is not the nearest zombie to the player, then this zombie tries to create ambush and waits for the player near exit
            //Moreover, zombie will not stay on the exit
            if (Utils.EnemyInAmbush(enemy.transform.position))
                move.Set(0, 0);
            //Call the MoveEnemy function of Enemy at index i in the enemies List.
            enemy.MoveEnemy((int)move.x, (int)move.y, zombieType);

            //Wait for Enemy's moveTime before moving next Enemy, 
            yield return new WaitForSeconds(enemy.moveTime);
        }
    }
}
