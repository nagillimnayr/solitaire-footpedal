using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    // Singleton
    public static StateController Instance { get; private set; }

    private State currentState = null;

    public LogicManager logicManager { get; private set; } = null;
    public InputManager inputManager { get; private set; } = null;
    
    
    private void Awake()
    {
        // To prevent multiple instances of the class existing, delete this object if it is not 
        // the static global instance
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else // If Instance is null, then this is the only instance
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get references
        if (!logicManager)
        {
            logicManager = GameObject.Find("Logic Manager").GetComponent<LogicManager>();
        }
        if (!inputManager)
        {
            inputManager = GameObject.Find("Input Manager").GetComponent<InputManager>();
        }
        
        // Enter Calibration state
        ChangeState(new CalibrationState());
    }

    // Update is called once per frame
    void Update()
    {
        // Update current state
        currentState?.Update(Time.deltaTime);
    }

    public void ChangeState(State newState)
    {
        // Exit current state
        currentState?.Exit();
        // Change to new state
        currentState = newState;
        // Enter new state
        currentState?.Enter();
    }
}
