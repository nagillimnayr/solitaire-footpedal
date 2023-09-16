using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPedalReader : MonoBehaviour
{
    // Singleton
    public static FootPedalReader Instance { get; private set; }

    // Serial Controller
    [SerializeField] public static FootPedalSerialController serialController = null; // Reference to the Serial Controller 
    //[SerializeField] public FootPedalSerialController serialController = null; // Reference to the Serial Controller 
    
    // The average baseline measurement reading when the foot is down
    public static int baseline { get; private set; } = 0;
    public static int maxBaselineReading { get; private set; } = 0; // The highest distance read during baseline calibration
    
    // The minimum measurement reading above the baseline to consider as an attempted lift (for collecting data on avg ROM)
    // Also used to re-enable input once foot has been lowered back down
    public static int LowerThreshold { get; private set; } = 0; 
    // The minimum measurement reading above the baseline to consider as a successful lift
    public static int UpperThreshold { get; private set; } = 0;

    // Ratio of average max distance with which to calculate the thresholds
    public float lowerThresholdRatio = 0.15f;
    public float upperThresholdRatio = 0.5f;
    
    public static bool isConnected { get; private set; } = false;
    public static bool isCalibrating { get; private set; } = false;
    [SerializeField] public bool pedalUp = false;
    
    /* Since we don't know which serial port will be used, we will try to connect to each until
     * either a successful connection is made, or a limit is reached. */
    private const int numOfPortsToTry = 9;
    [SerializeField] public static string portNamePrefix = "COM";
    private static int portNum = 1;  // COM port to try

    public static Queue<int> inputQueue { get; private set; } = null;
    public const int QueueCapacity = 5;
    
    public static int footPedalMoves { get; private set; } = 0;
    
    /* ##### Methods ##### */
    
    private void Awake()
    {
        // To prevent multiple instances of the class existing, delete this object if it is not 
        // the static global instance
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        else // If Instance is null, then this is the only instance
        {
            Instance = this;
        }

        if (inputQueue != null)
        {
            inputQueue.Clear();
            inputQueue = null;
        }

        inputQueue = new Queue<int>(QueueCapacity);
        
        footPedalMoves = 0;
        isConnected = false;
        
        // Setup events
        // EventManager.AddListener("Calibrate", Calibrate);
    }

    // Start is called before the first frame update
    void Start()
    {
        ConnectionUI.ConnectingScreen.SetActive(true);
        ConnectionUI.ConnectingText.SetActive(true);
        ConnectionUI.ConnectionFailedText.SetActive(false);
        // Attempt to connect to COM Port
        AttemptConnection();
    }

    private void Update()
    {
        // If serial controller is inactive, do nothing
        if (!serialController) return;
        // If calibrating, do nothing
        // if (isCalibrating) return;

        if (isConnected)
        {
            ReadMessage();
        }
        
        if (inputQueue.Count >= QueueCapacity)
        {
            ProcessDistanceMeasurement(ProcessInputQueue());
        }
    }

    void AttemptConnection()
    {
        
        // If we've tried every port, break
        if (portNum > numOfPortsToTry)
        {
            Debug.LogWarning("> Error: No valid COM ports were found.");
            ConnectionUI.ConnectingText.SetActive(false);
            ConnectionUI.ConnectionFailedText.SetActive(true);
            serialController.messageListener = null;
            
            StartCoroutine(ConnectionFailed());
            //LogicManager.QuitGame();
            return;
        }
        
        // Destroy previous serial controller
        if (serialController != null)
        {
            serialController.enabled = false;
            Destroy(serialController);
            serialController = null;
        }
        // Create new serial controller
        serialController = gameObject.AddComponent<FootPedalSerialController>();
        //serialController = gameObject.AddComponent<SerialController>();
        
        // Concatenate port name prefix with the port number that we are trying
        string portName = portNamePrefix + portNum.ToString();

        //Debug.Log("Attempting to connect to: " + portName);
        
        // Set the serial controller's port name
        serialController.portName = portName;

        serialController.messageListener = gameObject;
        
        // Attempt to connect
        serialController.AttemptConnection();
        
    }
    
    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string message)
    {
        //Debug.Log(message);
        if (message == null)
        {
            return;
        }
        
        // Try to get distance reading from message
        int distance = 0;
        // Convert string to int
        if (int.TryParse(message, out distance))
        {
            //Debug.Log(message);
            // Pass distance to Input Manager
            //inputManager.ProcessFootPedalInput(distance);
            //ProcessDistanceMeasurement(distance);
            inputQueue.Enqueue(distance);
        }
        else
        {
            // If conversion to int failed, display message
            Debug.Log(message);
        }
    }

    int ProcessInputQueue()
    {
        // Get average of all items in queue
        int count = 0;
        int sum = 0;
        while (count < QueueCapacity && inputQueue.Count > 0)
        {
            sum += inputQueue.Dequeue();
            count++;
        }

        // Return the average
        if (count == 0) return 0;
        return sum / count;
    }

    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.
    void OnConnectionEvent(bool success)
    {
        //Debug.Log(success ? "Device connected" : "Device disconnected" );

        // if connection was successful, disable message listener so we can poll for messages
        if (success)
        {
            Debug.Log("> " + portNamePrefix +  portNum.ToString() + ": Device Connected");
            
            serialController.messageListener = null;
            StartCoroutine(ConnectionSuccess());
        }
        // If disconnected, retry connection
        else
        {
            Debug.Log("> " + portNamePrefix +  portNum.ToString() + ": Device Disconnected");
            isConnected = false;
            serialController.Disconnect();
            // Try next port
            portNum++;
            AttemptConnection();
        }
    }
    
    IEnumerator ConnectionSuccess()
    {
        // Wait a few seconds before disabling screen
        ConnectionUI.ConnectingText.SetActive(false);
        ConnectionUI.ConnectionFailedText.SetActive(false);
        ConnectionUI.ConnectionSuccessText.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        isConnected = true;
        yield return new WaitForSeconds(2.0f);
        ConnectionUI.ConnectionSuccessText.SetActive(false);
        ConnectionUI.ConnectingScreen.SetActive(false);
    }

    IEnumerator ConnectionFailed()
    {
        yield return new WaitForSeconds(2.0f);
        ConnectionUI.ConnectionFailedText.SetActive(false);
        ConnectionUI.ConnectingScreen.SetActive(false);
        EventManager.Trigger("NoConnection");
    }

    // Poll Serial Controller for message
    void ReadMessage()
    {
        // Pass it to OnMessageArrived() to process
        OnMessageArrived(serialController.ReadSerialMessage());
    }

    void ProcessDistanceMeasurement(float distance)
    {
        // If lowered beneath lower threshold
        if (distance < maxBaselineReading + LowerThreshold)
        {
            if (pedalUp)
            {
                StartCoroutine(InputManager.Instance.LockInput(0.25f)); // Lock input briefly just to be cautious
            }
            // Reset bool to re-enable input
            pedalUp = false;
        }
        
        if (pedalUp) return;

        if (!InputManager.Instance.Good()) return;
        
        if (distance >= maxBaselineReading + UpperThreshold)
        {
            Debug.Log("Reading: " + distance.ToString());
            InputManager.Instance.PedalInputDraw(); // Draw a card
            pedalUp = true; // Set bool so that input is locked until foot is lowered again
        }
    }

    /*
    private void Calibrate()
    {
        isCalibrating = true;
        
        // Attempt to connect to COM Port
        //AttemptConnection();

        Debug.Log("> Calibrating...");
        StartCoroutine(GetBaseline());
    }
    */

    /*
    // Determine the average reading when foot is down
    IEnumerator GetBaseline()
    {
        while (!isConnected)
        {
            yield return new WaitForSeconds(1.0f);
        }
        
        // Activate Baseline Screen
        BaselineUI.BaselineScreen.SetActive(true);
        
        maxBaselineReading = 0; // Reset
        const int MAX_NUM_OF_READINGS = 50;
        int numOfReadings = 0;
        int sum = 0;
        while (numOfReadings < MAX_NUM_OF_READINGS)
        {
            // Update text
            BaselineUI.BaselineNumOfReadingsText.text = numOfReadings.ToString() + " of " + MAX_NUM_OF_READINGS.ToString();
            
            // Get message from serial controller
            string message = serialController.ReadSerialMessage();
            int distance = 0;
            // Attempt to convert to an int
            if (int.TryParse(message, out distance) && distance != 0)
            {
                // If conversion was successful, add to sum
                numOfReadings++;
                sum += distance;
                
                // Update current reading text
                BaselineUI.BaselineCurrentReadingText.text = distance.ToString();
                
                // Update maximum baseline reading
                if (distance > maxBaselineReading)
                {
                    maxBaselineReading = distance;
                }
            }
            yield return new WaitForSeconds(0.1f); 
        }

        // Get average reading
        baseline = sum / MAX_NUM_OF_READINGS;

        Debug.Log("> Baseline: " + baseline.ToString());
        Debug.Log("> Max Baseline Reading: " + maxBaselineReading.ToString());

        // Once baseline has been determined, determine threshold
        StartCoroutine(CalibrateThreshold());
        // Deactivate Baseline Screen
        BaselineUI.BaselineScreen.SetActive(false);
        yield break;
    }
    */
    
    // Get average of max distance readings to determine upper and lower thresholds
    // IEnumerator CalibrateThreshold()
    // {
    //     // Activate Calibration Screen
    //     CalibrationUI.CalibrationScreen.SetActive(true);
        
    //     const int MAX_NUM_OF_READINGS = 2;
    //     int numOfReadings = 0;
        
    //     int[] maxReadings = new int[MAX_NUM_OF_READINGS];
    //     int maxReading = 0;
    //     int buffer = maxBaselineReading - baseline; // To account for variations in the readings
    //     int prevLowerReading = 0; // To record the reading when the user lowers their foot, to prevent immediately registering it as a new attempt

    //     while (numOfReadings < MAX_NUM_OF_READINGS)
    //     {
    //         // Update text
    //         CalibrationUI.CalibrationNumOfReadingsText.text = (numOfReadings).ToString() + " of " + MAX_NUM_OF_READINGS.ToString();
    //         CalibrationUI.CalibrationCurrentReadingText.text = maxReading.ToString();
            
    //         /*// Get message from serial controller
    //         string message = serialController.ReadSerialMessage();
    //         int distance = 0;*/

    //         // Get average measurement
    //         for (int i = 0; i < QueueCapacity; i++)
    //         {
    //             ReadMessage();
    //         }
    //         int distance = ProcessInputQueue();
            
    //         // Attempt to convert to an int
    //         //if (int.TryParse(message, out distance) && distance != 0)
    //         if (distance != 0)
    //         {
    //             // If conversion was successful, check it against the max reading
    //             if (distance > maxReading 
    //                 && distance > maxBaselineReading + buffer // Make sure the reading is above the baseline
    //                 && (prevLowerReading == 0 || distance > prevLowerReading + buffer)
    //             )
    //             {
    //                 maxReading = distance;
    //             }
    //             // If foot has been lowered back down, then this attempt is over
    //             else if(distance < maxBaselineReading
    //                     && maxReading > maxBaselineReading + buffer  // Make sure the max reading is above the baseline 
    //                     && distance < maxReading - buffer // Make sure distance has dropped beneath maxReading
    //                     )   
    //             {
    //                 // Record max reading for this attempt
    //                 maxReadings[numOfReadings] = maxReading;
    //                 numOfReadings++;
    //                 prevLowerReading = distance;
    //                 Debug.Log("> Max Reading" + numOfReadings.ToString() + ": " + maxReading.ToString());
    //                 maxReading = 0; // Reset max reading
    //                 yield return new WaitForSeconds(2.0f); // Wait a bit before starting next attempt
    //             }
    //         }
    //         yield return new WaitForSeconds(0.1f);
    //     }
        
    //     // Calculate average of attempts
    //     int sum = 0;
    //     for (int i = 0; i < numOfReadings; i++)
    //     {
    //         sum += maxReadings[i] - baseline;
    //     }

    //     //int average = sum / numOfReadings;
    //     int average = sum / numOfReadings;
        
    //     // Set thresholds
    //     UpperThreshold = (int)(average * upperThresholdRatio);
    //     LowerThreshold = (int)(average * lowerThresholdRatio);
        
    //     Debug.Log("> Calibration Complete.");
    //     Debug.Log("> Baseline: " + baseline.ToString());
    //     Debug.Log("> Max Baseline Reading: " + maxBaselineReading.ToString());
    //     Debug.Log("> UpperThreshold: " + UpperThreshold.ToString());
    //     Debug.Log("> LowerThreshold: " + LowerThreshold.ToString());

    //     // Wait for a second
    //     yield return new WaitForSeconds(1.0f);
    //     isCalibrating = false;
    //     // Trigger Calibration Complete Event
    //     EventManager.Trigger("CalibrationComplete");
    //     // Deactivate Calibration Screen
    //     CalibrationUI.CalibrationScreen.SetActive(false);
        
        
    //     yield return new WaitForSeconds(3.0f);
        
    //     yield break;
    // }
}
