using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalibrationUI : MonoBehaviour
{
    // Singleton
    public static CalibrationUI Instance { get; private set; }
    
    public static GameObject CalibrationScreen { get; private set; } // Calibration Screen
    public static GameObject CalibrationInstructionsText { get; private set; } // Calibration Screen
    public static TextMeshProUGUI CalibrationNumOfReadingsText;
    public static TextMeshProUGUI CalibrationCurrentReadingText;
    
    
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
        // Calibration Screen 
        CalibrationScreen = GameObject.Find("Calibration Screen");
        if(CalibrationScreen == null) {Debug.LogWarning("> ERROR: CalibrationScreen is null");}
        CalibrationInstructionsText = GameObject.Find("Calibration Instructions Text");
        if(CalibrationInstructionsText == null) {Debug.LogWarning("> ERROR: CalibrationInstructionsText is null");}
        
        CalibrationNumOfReadingsText = GameObject.Find("Calibration Number Of Readings Number Text").GetComponent<TextMeshProUGUI>();
        CalibrationCurrentReadingText = GameObject.Find("Calibration Current Reading Number Text").GetComponent<TextMeshProUGUI>();
        
        CalibrationScreen.SetActive(false);
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
