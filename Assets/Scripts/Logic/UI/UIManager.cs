
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager Instance { get; private set; }

    
    // References
    //[FormerlySerializedAs("input")] public InputManager inputManager = null;
    public static TextMeshProUGUI MovesText { get; private set; } // Moves counter text
    // public static GameObject AutoPlayText { get; private set; } // Moves counter text
    public static GameObject NoMovesLeftText { get; private set; } // No moves left text
    public static GameObject WinScreen { get; private set; } // Win Screen
    public static GameObject PauseScreen { get; private set; } // Pause Screen
    
    
    public static TextMeshProUGUI InputModeButtonText { get; private set; } // Text of input mode button 
    public static GameObject InputModePanel { get; private set; }

    public static string[] InputModeStrings = new string[2] { "Foot Pedal", "Mouse" };

    
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
        
        //inputManager = GameObject.Find("Input Manager").GetComponent<InputManager>();
        
        MovesText = GameObject.Find("Text_Moves (TMP)").GetComponent<TextMeshProUGUI>();
        // AutoPlayText = GameObject.Find("Text_AutoPlay (TMP)");
        NoMovesLeftText = GameObject.Find("Text_NoMovesLeft (TMP)");
        WinScreen = GameObject.Find("GameWon_Screen");
        PauseScreen = GameObject.Find("Pause_Screen");
        InputModeButtonText = GameObject.Find("Text_InputMode_Switch (TMP)").GetComponent<TextMeshProUGUI>();
        InputModePanel = GameObject.Find("InputMode_Panel");
        
        
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
        UpdateInputModeText();
        ShowWinScreen(false);
        ShowPauseScreen(false);
        // ShowAutoPlay(false);
        ShowNoMovesLeft(false);
        ShowInputMode(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ShowWinScreen(bool show)
    {
        WinScreen.SetActive(show);
    }
    public static void ShowPauseScreen(bool show)
    {
        PauseScreen.SetActive(show);
    }

    public static void ShowAutoPlay(bool show)
    {
        // AutoPlayText.SetActive(show);
    }
    public static void ShowNoMovesLeft(bool show)
    {
        // NoMovesLeftText.SetActive(show);
    }

    public static void ShowInputMode(bool show)
    {
        InputModePanel.SetActive(show);
    }

    public static void UpdateInputModeText()
    {
        InputModeButtonText.text = InputModeStrings[(int)InputManager.inputMode];
    }

    
}
