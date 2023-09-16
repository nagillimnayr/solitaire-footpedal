using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86;

public enum InputMode
{
    FootPedal, // 0
    Mouse // 1
}

public class InputManager : MonoBehaviour
{
    // Singleton
    public static InputManager Instance { get; private set; }
    
    private static LayerMask layerMask = 1 << 7; // Card layer mask

    // References
    [NonSerialized] public static CardMovement lastCard = null; // Reference to the last card to be moved
    public PointerControls pointerControls = null;
    public static FootPedalReader footPedalReader;
    
    [Serialize] public static Stack<GameObject> carryStack = new Stack<GameObject>(); // Stack to hold the cards being carried
    [NonSerialized] public static bool isDroppingCards = false;
    [Serialize] public bool inputLocked  = false;
    [Serialize] public bool isPointerDown { get; private set; } = false;

    [Serialize] public static InputMode inputMode { get; private set; } = InputMode.FootPedal; // To control whether input should be
                                                                            // accepted from the pedal or the mouse for
                                                                            // drawing cards

    // Timer for enabling mouse input if no input has been successfully read for a certain amount of time 
    private static float timer = 0.0f;
    [SerializeField] private static float timeout = 180.0f; // In seconds

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
        
        pointerControls = new PointerControls();
        Cursor.Instance.pointerControls = pointerControls;
        
        // Setup callback events for pointer controls
        pointerControls.Pointer.PointerDown.started += ctx => OnPointerDown(ctx);
        pointerControls.Pointer.PointerDown.performed += ctx => OnPointerUp(ctx);
        
        EventManager.AddListener("NoConnection", () =>
        {
            inputMode = InputMode.Mouse;
            UIManager.UpdateInputModeText(); // Update text of button
        });
    }


    private void OnEnable()
    {
        pointerControls.Enable();
    }
    private void OnDisable()
    {
        pointerControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //if (!gameObject.scene.IsValid()) return;

    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime; // Increase timer
        
        /*// Check if new input may be processed
        if (!Good()) return;
        
        timer += Time.deltaTime; // Increase timer
        // If timed out, show Input mode switch button
        if (timer >= timeout)
        {
            LogicManager.gui.ShowInputMode(true);
        }

        // Process Keyboard input
        // Dont process input from the keyboard if a card is currently being held
        // Dont process input from keyboard if win screen is showing
        if (Input.anyKeyDown && carryStack.Count == 0 && !LogicManager.isWon) 
        {
            ProcessKeyboardInput();
        }
        /#1#/ Process Mouse input
        if(Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }#1#
        /#1#/ If mouse is not held down but cards are being carried, drop them
        else if (!Input.GetMouseButton(0) && carryStack.Count > 0)
        {
            OnMouseUp();
        }#1#
        // If pointer is not held down but cards are being carried, drop them
        if (!isPointerDown && carryStack.Count > 0)
        {
            StartCoroutine(DropCards());
        }*/
    }

    public void ProcessInput()
    {
        // Check if new input may be processed
        if (!Good()) return;
        
        //timer += Time.deltaTime; // Increase timer
        // If timed out, show Input mode switch button
        if (timer >= timeout)
        {
            UIManager.ShowInputMode(true);
        }

        // Process Keyboard input
        // Don't process input from the keyboard if a card is currently being held
        // Don't process input from keyboard if win screen is showing
        if (Input.anyKeyDown && carryStack.Count == 0 && !LogicManager.isWon) 
        {
            ProcessKeyboardInput();
        }
        /*// Process Mouse input
        if(Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }*/
        /*// If mouse is not held down but cards are being carried, drop them
        else if (!Input.GetMouseButton(0) && carryStack.Count > 0)
        {
            OnMouseUp();
        }*/
        // If pointer is not held down but cards are being carried, drop them
        if (!isPointerDown && carryStack.Count > 0)
        {
            StartCoroutine(DropCards());
        }
    }
    
    public bool Good() // Returns true if reading input is allowed
    {
        // Returns true reading input is allowed
        // Returns false if input should be locked until some other task is complete
        
        // Dont process input if cards are being dealt or drawn or dropped
        if (
            inputLocked
            || LogicManager.isPaused
            || FootPedalReader.Instance.pedalUp
            || Deck.isDealingCards
            || Deck.isDrawingCard 
            || Deck.isReturningCards
            || isDroppingCards 
            || (lastCard && lastCard.isMoving) // Dont process input if any cards are still moving
            || Deck.isAutoPlaying
            || LogicManager.isAutoPlayOn
            || LogicManager.isWon
            )
        {
            return false;
        }

        // If no other task is currently being performed, new input may be processed
        return true;
    }
    void ProcessKeyboardInput()
    {
        // ESC: Pause game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LogicManager.Instance.PauseGame();
        }
        // Space: Draw card
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Only draw if inputMode is set to Mouse
            if (inputMode == InputMode.Mouse || LogicManager.debugMode)
                StartCoroutine(Deck.Instance.DrawCard());
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if(LogicManager.debugMode)
            {
                UnityEngine.Debug.Log("> Drawing all");
                StartCoroutine(Deck.Instance.DrawAll());
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (LogicManager.debugMode)
                LogicManager.Instance.AutoPlay();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            if (LogicManager.debugMode)
                StartCoroutine(Deck.Instance.AutoPlayPass());
        }
    }
    
    private void OnPointerDown(InputAction.CallbackContext context)
    {
        /*if (context.started)
            Debug.Log("Action was started");
        else if (context.performed)
            Debug.Log("Action was performed");
        else if (context.canceled)
            Debug.Log("Action was cancelled");*/

        // If input is locked, return
        if (!Good())
        {
            Debug.Log("> Input Blocked");
            return;
        }
        if (isPointerDown) return;  
        
        timer = 0.0f; // Reset timer
        isPointerDown = true;
        
        // Get position of pointer
        Vector2 pointerPos = pointerControls.Pointer.PointerPosition.ReadValue<Vector2>();
        //Debug.Log("> Pointer Down: " + pointerPos);
        
        // Check for hit on Deck
        GameObject hitObj = RaycastPile(pointerPos);
        // If Deck was hit, draw card
        if (hitObj && hitObj.CompareTag("Deck"))
        {
            //Debug.Log("> Pointer Down: Hit Deck");
            // Only draw if inputMode is set to Mouse
            if (inputMode == InputMode.Mouse)
            {
                StartCoroutine(Deck.Instance.DrawCard());
                return;
            }
        }
        
        // Check for hit on Card
        PlayingCard card = RaycastCard(pointerPos);
        if (!card) return; // If nothing was hit, return
        
        // Check if the card was in the Waste pile
        if (card.pile.CompareTag("Waste"))
        {
            // If the card is in the Waste pile but is not the card on the top of the stack,
            // then return
            if(card.gameObject != Waste.Instance.pile.Peek()) return; 
        }
        else if (card.pile.CompareTag("Deck"))
        {
            // If the card is in the stock pile then return
            return; 
        }

        if (carryStack.Count > 0)
        {
            //Debug.Log("> Already carrying a card");
            return;
        }
        
        // Otherwise, pickup the card
        //Debug.Log("> Picking up: " + card.gameObject.name);
        card.Pickup(Cursor.Instance.gameObject); // Pick up card
        timer = 0.0f; // Reset timer
        
    }
    
    private void OnPointerUp(InputAction.CallbackContext context)
    {
        /*if (context.started)
            Debug.Log("Action was started");
        else if (context.performed)
            Debug.Log("Action was performed");
        else if (context.canceled)
            Debug.Log("Action was cancelled");*/
        
        if (!isPointerDown) return; // If pointer wasn't down, ignore
        isPointerDown = false;
        if (carryStack.Count == 0) return;
        
        timer = 0.0f; // Reset timer
        
        // Get position of pointer
        Vector2 pointerPos = pointerControls.Pointer.PointerPosition.ReadValue<Vector2>();
        //Debug.Log("> Pointer Up" + pointerPos);
        
        // Check for hit on Pile
        GameObject hitObj = RaycastPile(pointerPos);
        
        // If hit was on a Tableau or Foundation Pile, attempt to place the cards
        if (hitObj && (hitObj.CompareTag("Tableau") || hitObj.CompareTag("Foundation")))
        {
            // Place cards
            StartCoroutine(PlaceCards(hitObj));
            return; // End function call
        }

        // Check for hit on card
        PlayingCard card = RaycastCard(pointerPos);
        if (card)
        {
            GameObject pile = card.pile;
            if (pile.CompareTag("Tableau") || pile.CompareTag("Foundation"))
            {
                // Place cards
                StartCoroutine(PlaceCards(pile));
                return; // End function call}
            }
        }
        
        // If not over any pile, return all cards to their previous pile
        StartCoroutine(DropCards());
        
    }

    GameObject RaycastPile(Vector2 position)
    {
        //Get the mouse position on the screen and send a raycast into the game world from that position.
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(position);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        // If nothing was hit, return null
        if (!hit.collider) return null;
        
        // Otherwise return the GameObject that was hit
        //Debug.Log("---> Hit Pile: " + hit.collider.name);
        return hit.collider.gameObject;
    }
    
    PlayingCard RaycastCard(Vector2 position)
    {
        // Raycast to check for hit on cards
        Ray cardRay = Camera.main.ScreenPointToRay(position);
        RaycastHit cardHit;
        
        
        //// Check for hit on card
        if (Physics.Raycast(cardRay, out cardHit, 300, layerMask))
        {
            if (cardHit.collider.CompareTag("Card"))
            {
                //Debug.Log("---> Hit Card: " + cardHit.collider.name);
                // If a card was hit, return it
                return cardHit.collider.gameObject.GetComponent<PlayingCard>();
            }
            else return null;
        }
        else return null;
    }
    
    /*
    void OnMouseDown()
    {
        // Get mouse position
        Vector3 mousePos = Input.mousePosition;

        //Get the mouse position on the screen and send a raycast into the game world from that position.
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        //If something was hit, the RaycastHit2D.collider will not be null.
        if (hit.collider != null)
        {
            Debug.Log("---> Hit: " + hit.collider.name);
            // If Deck was hit, draw card
            if (hit.collider.CompareTag("Deck"))
            {
                // Only draw if inputMode is set to Mouse
                if (inputMode == InputMode.Mouse)
                    StartCoroutine(Deck.DrawCard());
            }
        }

        // Raycast to check for hit on cards
        Ray cardRay = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit cardHit;

        //// Check for hit on card
        if (Physics.Raycast(cardRay, out cardHit, 300, layerMask))
        {
            if (cardHit.collider.CompareTag("Card"))
            {
                // Get reference to card that was hit
                GameObject obj = cardHit.collider.gameObject;
                PlayingCard card = obj.GetComponent<PlayingCard>();
                if (!card) return;
                // Check if the card was in the Waste pile
                if (card.pile.CompareTag("Waste"))
                {
                    // If the card is in the Waste pile but is not the card on the top of the stack,
                    // then return
                    if(obj != Waste.pile.Peek()) return; 
                }
                
                Debug.Log("> Picking up: " + card.gameObject.name);
                card.Pickup(Cursor); // Pick up card
                timer = 0.0f; // Reset timer
            }
        }

    }
    */

    /*
    private void OnMouseUp()
    {
        timer = 0.0f; // Reset timer
        
        // List of 5 positions from which to send out raycasts,
        // Mouse position, and each corner of the card
        List<Vector3> rayPos = new List<Vector3>(5);

        Bounds cardBounds = carryStack.Peek().GetComponent<MeshFilter>().mesh.bounds;
        float cardWidth = cardBounds.max.x - cardBounds.min.x;
        float cardHeight = cardBounds.max.y - cardBounds.min.y;

        Vector3 mousePos = Input.mousePosition;

        rayPos.Add(mousePos); // Mouse position
        //rayPos.Add(mousePos + new Vector3(cardWidth / 2f, cardHeight / 2f, 0f)); // Top right corner
        //rayPos.Add(mousePos + new Vector3(-cardWidth / 2f, cardHeight / 2f, 0f)); // Top left corner;
        //rayPos.Add(mousePos + new Vector3(-cardWidth / 2f, -cardHeight / 2f, 0f)); // Bottom left corner;
        //rayPos.Add(mousePos + new Vector3(cardWidth / 2f, -cardHeight / 2f, 0f)); // Bottom right corner;


        for (int i = 0; i < rayPos.Count; i++)
        {
            // Check for hit on an empty pile
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(rayPos[i]);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            // If something was hit, the RaycastHit2D.collider will not be null.
            if (hit.collider != null)
            {
                // Get reference to GameObject that was hit
                GameObject obj = hit.collider.gameObject;


                Debug.Log("---> Hit: " + obj.name);

                if (obj.CompareTag("Tableau") || obj.CompareTag("Foundation"))
                {
                    // Place cards
                    StartCoroutine(PlaceCards(obj));
                    return; // End function call
                }
            }
            

            // Raycast to check for hit on cards
            Ray cardRay = Camera.main.ScreenPointToRay(rayPos[i]); // Need to avoid collision with currently held card
            RaycastHit cardHit;

            //// Check for hit on card
            if (Physics.Raycast(cardRay, out cardHit, 300, layerMask))
            {
                if (cardHit.collider.CompareTag("Card"))
                {
                    // Get reference to card that was hit
                    GameObject card = cardHit.collider.gameObject; // Get reference to card that was hit
                    GameObject pile = card.GetComponent<PlayingCard>().pile; // Get reference to pile that card is in

                    // Place cards
                    StartCoroutine(PlaceCards(pile));
                    return; // End function call
                }
            }
        }

        // If not over any pile, return all cards to their previous pile
        StartCoroutine(DropCards());
    }
    */
    
    

    public void PedalInputDraw()
    {
        if (carryStack.Count > 0) return;
        //if (!Good()) return;
        
        // Set bool to prevent multiple inputs being read, pedal must be lowered
        // before input will be accepted again 
        //pedalUp = true; 
        StartCoroutine(Deck.Instance.DrawCard());
        timer = 0.0f; // Reset timer
    }

    // Place cards on pile with slight delay between them
    IEnumerator PlaceCards(GameObject pile)
    {
        //Debug.Log("> Placing cards!");
        //CardMovement.moveSpeed = maxSpeed; // Increase card movement speed
        isDroppingCards = true;
        PlayingCard card = null;
        bool isValid = false;

        // Place cards until stack is empty
        while (carryStack.Count > 0)
        {
            card = carryStack.Peek().GetComponent<PlayingCard>();

            // Only place card if pile is not the pile that it was taken from
            if (pile != card.previousPile)
            {
                isValid = (card.Place(pile)); // Attempt to place card on pile, record validity
            }

            // If move was invalid, drop all cards and break loop
            if (!isValid)
            {
                StartCoroutine(DropCards());
                break;
            }

            // If move was valid, pop card from stack
            carryStack.Pop();
            yield return new WaitForSeconds(0.15f); // Delay before next iteration
        }

        // If move was valid:
        if (isValid && card != null)
        {
            // Increment move counter
            LogicManager.IncrementMoveCounter();

            // Check previous pile to see if it needs to flip its top card
            GameObject previousPile = card.GetComponent<PlayingCard>().previousPile; // Get reference to previous pile
            // Only relevant if previous pile was a Tableau pile
            if (previousPile != null && previousPile.CompareTag("Tableau")) 
            {
                Tableau tableau = previousPile.GetComponent<Tableau>(); // Get reference to Tableau script
                // If tableau has no more face up cards, flip the top face down card
                if (tableau.pile.Count == 0)
                {
                    StartCoroutine(tableau.FlipTopCard());
                }
            }
            yield return new WaitForSeconds(0.75f); // Delay before re-enabling input
            isDroppingCards = false;
        }

    }

    // Return cards to their previous piles with slight delay between them
    IEnumerator DropCards()
    {
        if (carryStack.Count == 0) yield break;
        //Debug.Log("> Dropping cards!");
        //CardMovement.moveSpeed = maxSpeed; // Increase card movement speed
        isDroppingCards = true; 

        // Return cards until stack is empty
        while (carryStack.Count > 0)
        {
            GameObject card = carryStack.Pop();
            //card.transform.DetachChildren(); // Detach children so they return one-by-one
            card.GetComponent<PlayingCard>().Drop(); // Drop card
            yield return new WaitForSeconds(0.15f); // Delay before next iteration
        }
        //yield return new WaitForSeconds(0.1f); // Delay before re-enabling input
        isDroppingCards = false;
    }

    // Lock input for specified number of seconds
    public IEnumerator LockInput(float seconds)
    {
        inputLocked = true; // Lock input
        yield return new WaitForSeconds(seconds); // Delay
        inputLocked = false; // Unlock input

    }

    // Switch input mode
    public void SwitchInputMode()
    {
        switch (inputMode)
        {
            case InputMode.FootPedal:
                inputMode = InputMode.Mouse;
                break;
            case InputMode.Mouse:
                inputMode = InputMode.FootPedal;
                break;
        }
        UIManager.UpdateInputModeText(); // Update text of button
    }

  
} // End of class definition
