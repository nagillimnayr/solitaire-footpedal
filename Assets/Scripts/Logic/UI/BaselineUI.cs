using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaselineUI : MonoBehaviour
{
    // Singleton
    public static BaselineUI Instance { get; private set; }
    
    
    public static GameObject BaselineScreen { get; private set; } // Calibration Screen
    public static GameObject BaselineInstructionsText { get; private set; } // Calibration Screen
    public static TextMeshProUGUI BaselineNumOfReadingsText;
    public static TextMeshProUGUI BaselineCurrentReadingText;
    
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
        // Getting Baseline Screen 
        BaselineScreen = GameObject.Find("Baseline Screen");
        if(BaselineScreen == null) {Debug.LogWarning("> ERROR: BaselineScreen is null");}
        BaselineInstructionsText = GameObject.Find("Baseline Instructions Text");
        if(BaselineInstructionsText == null) {Debug.LogWarning("> ERROR: BaselineInstructionsText is null");}

        BaselineNumOfReadingsText = GameObject.Find("Baseline Number Of Readings Number Text").GetComponent<TextMeshProUGUI>();
        BaselineCurrentReadingText = GameObject.Find("Baseline Current Reading Number Text").GetComponent<TextMeshProUGUI>();
        
        BaselineScreen.SetActive(false);
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
