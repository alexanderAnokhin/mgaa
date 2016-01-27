using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

    [Header(("AI Active"))]
    public bool active = true;

    [Header(("AI Decision Delay Interval"))]
    [RangeAttribute(0.05f, 2.0f)]
    public float decisionDelay = 1.0f;

    public static EnemyController instance;

    private float nextDecisionTime;
    private float currentTime;
    

    // Use this for initialization
    void Start() {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (currentTime >= nextDecisionTime && active) {
            Play();
            currentTime += Time.fixedDeltaTime;
            nextDecisionTime += decisionDelay;
        }
        else {
            currentTime += Time.fixedDeltaTime;
        }
    }

    void Play() {

    }
}
