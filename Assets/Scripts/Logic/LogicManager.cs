using JetBrains.Annotations;
using System;
using UnityEngine;

public enum GameMode
{
    TurnOne, // 0
    TurnThree // 1
}

public class LogicManager : MonoBehaviour
{
    // Singleton
    public static LogicManager Instance { get; private set; }
    
    public static bool isPaused { get; private set; } = false;
    public static bool debugMode { get; private set; } = false; // Bool for enabling certain features which are only meant for testing purposes
    public static int moves { get; private set; } = 0; // Moves counter
    [NonSerialized] public static bool isWon = false;
    [NonSerialized] public static bool isAutoPlayOn = false;
    public static bool isAutoPlayAllowed { get; private set; }  = false;
    //public bool isTurnThree { get; private set; } = true;
    [SerializeField] public static GameMode gameMode = GameMode.TurnOne;

    private static Foundation[] foundations = new Foundation[4];

    
    static CardSound sound;

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
        
        sound = GetComponent<CardSound>();
        
        // Setup events
        // EventManager.AddListener("CalibrationComplete", StartGame);
        
        EventManager.AddListener("Connection Success", StartGame);
        EventManager.AddListener("Connection Fail", StartGame);
        EventManager.AddListener("Start Game", StartGame);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        foundations[0] = GameObject.Find("Foundation_Pile_Diamonds").GetComponent<Foundation>();
        foundations[1] = GameObject.Find("Foundation_Pile_Clubs").GetComponent<Foundation>();
        foundations[2] = GameObject.Find("Foundation_Pile_Hearts").GetComponent<Foundation>();
        foundations[3] = GameObject.Find("Foundation_Pile_Spades").GetComponent<Foundation>();

        // Start Calibration
        // EventManager.Trigger("Calibrate");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Increment Move Counter")]
    public static void IncrementMoveCounter()
    {
        // Increment move counter
        moves++;

        // Update UIManager text
        UIManager.MovesText.text = "Moves: " + moves.ToString();

        //UnityEngine.Debug.Log("> Incrementing move counter");
    }

    private static void StartGame()
    {
        Debug.Log("LogicManager.StartGame()");
        // EventManager.Trigger("StartGame");
        Deck.Instance.StartGame();
    }
    
    public void PauseGame()
    {
        isPaused = !isPaused; // Invert
        
        UIManager.ShowPauseScreen(isPaused);
    }
    public static void RestartGame()
    {
        // Disable input
        if (!isWon && (Deck.isShuffling || Deck.isDealingCards || Deck.isDrawingCard 
            || Deck.isReturningCards))
        {
            // Do nothing
            return;
        }

        StopAutoPlay();
        UIManager.ShowNoMovesLeft(false);
        UIManager.ShowWinScreen(false); // Hide win screen

        //UnityEngine.Debug.Log("> Restarting game!");

        moves = 0; // Reset move counter
        UIManager.MovesText.text = "Moves: " + moves.ToString(); // Update UIManager text
        isWon = false; // Reset isWon bool


        //StartCoroutine(Deck.ReturnAllToDeck());
        Deck.Instance.StartGame();
    }

    public static void CheckForWin() 
    {
        if(
            foundations[0].IsFull() &&
            foundations[1].IsFull() &&
            foundations[2].IsFull() &&
            foundations[3].IsFull()
        )
        {
            WinGame();
        }
    }
    public static void WinGame()
    {
        isWon = true;
        //UnityEngine.Debug.Log("> Game Won!");
        sound.PlaySound("win2"); // Play sound
        UIManager.ShowWinScreen(true); // Show win screen
    }

    
    public void AutoPlay()
    {
        // isAutoPlayOn = true; // Set bool
        // UIManager.ShowAutoPlay(true); // Show text
        // StartCoroutine(Deck.Instance.AutoPlay()); // Start AutoPlay
    }

    public static void StopAutoPlay()
    {
        isAutoPlayOn = false; // Set bool
        UIManager.ShowAutoPlay(false); // Hide text
    }

    public static void NoMovesLeft()
    {
        UIManager.ShowNoMovesLeft(true);
        sound.PlaySound("negative");
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
