using UnityEngine;
using System.Collections;

public abstract class StateObject { 
    virtual public bool IsEnemy() { return false; }
    virtual public bool IsFood() { return false; }
    virtual public bool IsWall() { return false; }
    virtual public bool IsExit() { return false; }
}

public class EnemyStateObject : StateObject {
    float power;
    
    public EnemyStateObject(float power) { this.power = power; }
    
    public float GetPower() { return power; }
    
    override public bool IsEnemy() { return true; }
}

public class FoodStateObject : StateObject {
    float amount;
    
    public FoodStateObject(float amount) { this.amount = amount; }
    
    public float GetAmount() { return amount; }
    
    override public bool IsFood() { return true; }
}

public class WallStateObject : StateObject {
    float hp;
    
    public WallStateObject(float hp) { this.hp = hp; }
    
    public float GetHp() { return hp; }
    
    public WallStateObject Attacked() { return new WallStateObject(hp - 1f); }
    
    override public bool IsWall() { return true; }
}

public class ExitStateObject : StateObject { 
    override public bool IsExit() { return true; }
}
