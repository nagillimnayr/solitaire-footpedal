using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Global Event Manager
public class EventManager : MonoBehaviour
{
    // Singleton
    public static EventManager Instance { get; private set; }
    
    // Dictionary of Events
    private static Dictionary<string, UnityEvent> Events = null;



    private void Init()
    {
        if (Events == null)
        {
            Events = new Dictionary<string, UnityEvent>();
        }
    }
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
            Init();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Add Listener to Events
    public static void AddListener(string eventName, UnityAction listener)
    {
        if (Instance == null) return;
        UnityEvent evt = null;
        // If event name already exists, add listener to it
        if (Events.TryGetValue(eventName, out evt))
        {
            evt.AddListener(listener);
        }
        else // If it doesn't exist, create it
        {
            evt = new UnityEvent();
            evt.AddListener(listener);
            Events.Add(eventName, evt); // Add to Events
        }
    }
    
    // Remove Listener from Events
    public static void RemoveListener(string eventName, UnityAction listener)
    {
        if (Instance == null) return;
        UnityEvent evt = null;
        
        // If the event exists, remove this listener from it
        if (Events.TryGetValue(eventName, out evt))
        {
            evt.RemoveListener(listener);
        }
        // If it doesn't exist, do nothing
    }
    
    // Trigger the specified Event
    public static void Trigger(string eventName)
    {
        UnityEvent evt = null;
        if (Events.TryGetValue(eventName, out evt))
        {
            // If event exists, invoke it
            evt.Invoke();
        }
        else // If event does not exist, log warning
        {
            Debug.LogWarning("> Error: Event (" + eventName + ") does not exist");
        }
    }
}
