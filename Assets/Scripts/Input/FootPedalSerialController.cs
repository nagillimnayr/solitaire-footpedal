using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FootPedalSerialController : SerialController // Inherit from SerialController
{
    void Awake()
    {
        maxUnreadMessages = 10;
    }
    
    // Start is called before the first frame update
    void Start()
    {
           
    }

    /* This is just here to hide the SerialController's OnEnable() which will automatically
     * try to connect */
    void OnEnable()
    {

    }



    /* Ryan: Added this because I wanted to control when the connection occurs
     * so that I could try each serial port until a successful
     * connection was found. So I moved the creation of the serial thread into a separate
     * public function that could be called externally at will, after changing the portName,
     * rather than when the gameObject is activated. */
    public void AttemptConnection()
    {
        serialThread = new SerialThreadLines(portName, 
            baudRate, 
            reconnectionDelay,
            maxUnreadMessages);
        
        thread = new Thread(new ThreadStart(serialThread.RunForever));
        thread.Start();
    }

    public void Disconnect()
    {
        // If there is a user-defined tear-down function, execute it before
        // closing the underlying COM port.
        if (userDefinedTearDownFunction != null)
            userDefinedTearDownFunction();

        // The serialThread reference should never be null at this point,
        // unless an Exception happened in the OnEnable(), in which case I've
        // no idea what face Unity will make.
        if (serialThread != null)
        {
            serialThread.RequestStop();
            serialThread = null;
        }

        // This reference shouldn't be null at this point anyway.
        if (thread != null)
        {
            thread.Join();
            thread = null;
        }
    }
}
