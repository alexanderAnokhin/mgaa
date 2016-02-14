using UnityEngine;
using System.Collections;

/*
 * The below classes are used to represent a game state for advance model 
 */

// Abstract preliminary class for MCTS
public abstract class StateObject { 
    virtual public bool IsEnemy() { return false; }
    virtual public bool IsFood() { return false; }
    virtual public bool IsWall() { return false; }
    virtual public bool IsExit() { return false; }
}

// Represents enemy object 
public class EnemyStateObject : StateObject {
    float power;
    
    public EnemyStateObject(float power) { this.power = power; }
    
    public float GetPower() { return power; }
    
    override public bool IsEnemy() { return true; }
}

// Represents food object
public class FoodStateObject : StateObject {
    float amount;
    
    public FoodStateObject(float amount) { this.amount = amount; }
    
    public float GetAmount() { return amount; }
    
    override public bool IsFood() { return true; }
}

// Represents wall object
public class WallStateObject : StateObject {
    float hp;
    
    public WallStateObject(float hp) { this.hp = hp; }
    
    public float GetHp() { return hp; }
    
    override public bool IsWall() { return true; }
}

// Represents exit object
public class ExitStateObject : StateObject { 
    override public bool IsExit() { return true; }
}
