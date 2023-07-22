using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    // Singleton
    public static ConnectionUI Instance { get; private set; }
    
    // Connecting Screen
    public static GameObject ConnectingScreen { get; private set; } 
    public static GameObject ConnectingText { get; private set; } = null;
    public static GameObject ConnectionFailedText { get; private set; } = null;
    public static GameObject ConnectionSuccessText { get; private set; } = null;
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
        
        Init();
    }

    void Init()
    {
        // Attempting Connection Screen
        ConnectingScreen = GameObject.Find("Attempting Connection Screen");
        if(ConnectingScreen == null) {Debug.LogWarning("> ERROR: ConnectingScreen is null");}
        ConnectingText = GameObject.Find("Attempting Connection Text");
        if(ConnectingText == null) {Debug.LogWarning("> ERROR: ConnectingText is null");}
        ConnectionFailedText = GameObject.Find("Connection Failed Text");
        if(ConnectionFailedText == null) {Debug.LogWarning("> ERROR: ConnectionFailedText is null");}
        ConnectionSuccessText = GameObject.Find("Connection Success Text");
        if(ConnectionSuccessText == null) {Debug.LogWarning("> ERROR: ConnectionSuccessText is null");}
        
        ConnectingScreen.SetActive(false);
        ConnectingText.SetActive(false);
        ConnectionFailedText.SetActive(false);
        ConnectionSuccessText.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
