using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class Deck : MonoBehaviour
{
    // Singleton
    public static Deck Instance { get; private set; }
    
    public const int NUM_OF_CARDS = 52; // Total number of cards in deck (const so cannot be changed during runtime)
    private static Vector3 cardOffsetZ = new Vector3(0.0f, 0.0f, -0.1f); // Vector3 for card offset in Z-axis (for stacking cards on top of each other)

    // Deck
    private static List<GameObject> deckOfCards = new List<GameObject>(52); // List of playing card objects 
    public static Stack<GameObject> shuffledDeck { private set; get; } = new Stack<GameObject>(52); // Stack to hold the the cards after being shuffled
    // Stack data structure because only the top card should be accessible

    // References
    [SerializeField] public GameObject playingCard; // Reference to playingCard prefab
    static List<GameObject> tableauPiles = new List<GameObject>(7); // List of references to the Tableau piles
    static List<GameObject> foundationPiles = new List<GameObject>(4); // List of references to the Foundation piles
    
    public static CardSound sound;

    // Booleans
    public static bool isShuffling { get; private set; } = false; // Disable input while shuffling cards
    public static bool isDealingCards { get; private set; } = false; // Disable input while cards are being dealt
    public static bool isDrawingCard { get; private set; } = false; // Disable input while card is being drawn
    public static bool isReturningCards { get; private set; } = false; // Disable input while returning cards to deck
    public static bool isAutoPlaying { get; private set; } = false;
    public static int autoMoves { get; private set; } = 0;
    public static bool noMovesLeft { get; private set; } = false;


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
        
        // Get sound script
        sound = gameObject.GetComponent<CardSound>();
        
        // Get references to the Tableau piles
        for (int i = 1; i <= 7; i++) 
        {
            tableauPiles.Add(GameObject.Find("Tableau_Pile_" + i.ToString()));
        }
        // Get references to the Foundation piles
        for (int i = 0; i < 4; i++)
        {
            string suit = PlayingCard.suitNames[i];
            foundationPiles.Add(GameObject.Find("Foundation_Pile_" + suit + "s"));
        }

        // Setup Events
        SetupEvents();
        
        // Create Playing Cards
        LoadCards();
        
    }

    void SetupEvents()
    {
        // Setup events
        EventManager.AddListener("StartGame", StartGame);
        EventManager.AddListener("DrawCard", StartDrawCard);
    }
    // Start is called before the first frame update
    void Start()
    {

        //LoadCards();

        //StartGame();
    }


    // Loads the cards in the deck,
    // Uses nested for loop to iterate through each rank of each suit
    // and instantiates a playingCard GameObject for each
    void LoadCards()
    {
        if (deckOfCards.Count > 0)
        {
            Debug.LogWarning("> Error: Cards have already been loaded");
            return;
        }
        Vector3 deckPos = transform.position; // Get position of stock pile
        Quaternion cardRotation = Quaternion.Euler(-90.0f, 0.0f, -180.0f); // Start face down
        for (int suit = 0; suit < 4; suit++)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                // Get index of current card (used to access deckOfCards array)
                int index = (suit * 13) + (rank - 1);

                // Get position for new card based on position of deck,
                // each new card will be placed slightly above the previous
                Vector3 cardPos = deckPos + cardOffsetZ * index; // Stack on top of each other

                // Instantiate playingCard GameObject
                PlayingCard card = Instantiate(playingCard, cardPos, cardRotation).GetComponent<PlayingCard>();
                // Add it to the deck
                deckOfCards.Add(card.gameObject);

                // Initialize suit and rank of card, function will handle the setting of appropriate material
                card.InitializeCard(suit, rank);
            }
        }
        // Initialize input.lastCard
        InputManager.lastCard = deckOfCards[deckOfCards.Count - 1].GetComponent<CardMovement>();
    }

    // Starts the game
    public void StartGame()
    {
        // Debug.Log("> Deck.StartGame()");
        StartCoroutine(ReturnAllToDeck()); // Return all cards to deck
        StartCoroutine(ShuffleDeck()); // Shuffle the deck
        StartCoroutine(DealCards()); // Deal out the cards
    }

    // Shuffles the deck
    IEnumerator ShuffleDeck()
    {
        // If cards haven't been loaded yet, load them
        if (shuffledDeck.Count == 0 && deckOfCards.Count == 0)
        {
            LoadCards();
        }
        
        // Wait until other operations are done
        while(isShuffling || isDealingCards || isDrawingCard || isReturningCards)
            yield return new WaitForSeconds(0.05f);

        // Wait until all cards are in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        //Debug.Log("> Shuffling Deck!");

        isShuffling = true;

        // Wait until sounds have been initialized
        while (!CardSound.cardSounds.ContainsKey("shuffle"))
            yield return new WaitForSeconds(0.05f);

        // Play shuffle sound
        while(!sound.PlaySound("shuffle"))
            yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(0.1f);

        // Move any cards that are in the shuffled deck back into the unshuffled deck
        while (shuffledDeck.Count > 0)
        {
            AddToDeck(shuffledDeck.Pop());
        }

        // Return all cards to deck
        while (deckOfCards.Count > 0)
        {
            // Pick random card from unshuffled deck and move to shuffled deck
            int index = UnityEngine.Random.Range(0, deckOfCards.Count);
            AddToShuffledDeck(deckOfCards[index]); // Add to shuffled deck
            deckOfCards.RemoveAt(index); // Remove from unshuffled deck
        }

        yield return new WaitForSeconds(0.1f);
        isShuffling = false;
        yield break;
    }

    public void AddToDeck(GameObject card)
    {
        // Move card to position
        Vector3 cardPos = transform.position + cardOffsetZ * (deckOfCards.Count + 1);
        card.GetComponent<PlayingCard>().MoveCardTo(cardPos, false); // Face down

        deckOfCards.Add(card); // Add to unshuffled deck

        // Set card's pile
        card.GetComponent<PlayingCard>().pile = gameObject;
    }
    private void AddToShuffledDeck(GameObject card)
    {
        // Move card to position
        Vector3 cardPos = transform.position + cardOffsetZ * shuffledDeck.Count; 
        card.GetComponent<PlayingCard>().MoveCardTo(cardPos, false); // Face down

        shuffledDeck.Push(card); // Add to shuffled deck

        // Set card's pile
        card.GetComponent<PlayingCard>().pile = gameObject;
    }

    public void StartDrawCard()
    {
        StartCoroutine(DrawCard());
    }
    
    // Draw card from top of stock pile, flip to be face up and put on top of Waste pile
    public IEnumerator DrawCard()
    {
        // Check game mode
        if (LogicManager.gameMode == GameMode.TurnThree)
        {
            // If game mode is TurnThree, pass control to DrawCards()
            StartCoroutine(DrawCards());
            yield break;
        }
        
        // Check if stock pile is empty
        if (shuffledDeck.Count == 0)
        {
            //Debug.Log("> Stock pile is empty!");
            // Play sound
            gameObject.GetComponent<CardSound>().PlaySound("fan");
            // Return cards from Waste pile to deck
            StartCoroutine(ReturnWasteToStockPile(Waste.Instance.pile));

            yield break;
        }

        // Debug.Log("> Drawing Card!");
        isDrawingCard = true;

        // Get reference to card being drawn and pop from stack
        GameObject card = shuffledDeck.Pop();
        // Move card up to avoid collision
        card.transform.position += new Vector3(0.0f, 0.0f, -50.0f); 
        // Add card to Waste pile
        Waste.Instance.AddToPile(card);

        // Play sound
        card.GetComponent<CardSound>().PlayRandomFlipSound();

        // Increment move counter
        LogicManager.IncrementMoveCounter();

        // Wait until all cards are in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        // Set bool to false
        isDrawingCard = false;
    }
    
    // Draw card(s) from top of stock pile, flip to be face up and put on top of Waste pile
    public IEnumerator DrawCards()
    {
        // Check if stock pile is empty
        if (shuffledDeck.Count == 0)
        {
            //Debug.Log("> Stock pile is empty!");
            // Play sound
            gameObject.GetComponent<CardSound>().PlaySound("fan");
            // Return cards from Waste pile to deck
            StartCoroutine(ReturnWasteToStockPile(Waste.Instance.pile));

            yield break;
        }

        // Debug.Log("> Drawing Cards!");
        isDrawingCard = true;

        StartCoroutine(Waste.Instance.Condense()); // Condense the Waste pile
        
        // Wait until all cards are in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        while (Waste.isCondensing)
            yield return new WaitForSeconds(0.05f);
        
        // If less than 3 cards are left in the deck, only draw that many cards
        int numOfCards = shuffledDeck.Count < 3 ? shuffledDeck.Count : 3;

        for (int i = 0; i < numOfCards; i++)
        {
            // Get reference to card being drawn and pop from stack
            GameObject card = shuffledDeck.Pop();
            card.transform.position += new Vector3(0.0f, 0.0f, -50.0f); // Move card up to avoid collison
            Waste.Instance.AddToPile(card, i); // Add card to Waste pile
            card.GetComponent<CardSound>().PlayRandomFlipSound(); // Play sound
        
            // Wait before drawing next card
            yield return new WaitForSeconds(0.1f);
        }

        // Increment move counter
        LogicManager.IncrementMoveCounter();

        // Wait until all cards are in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        
        
        StartCoroutine(Waste.Instance.Condense()); // Condense the Waste pile

        // Set bool to false
        isDrawingCard = false;
    }

    public static bool IsEmpty()
    {
        return shuffledDeck.Count == 0;
    }
    
    // Deals the cards to setup the game
    IEnumerator DealCards()
    {
        // Wait until other operations are done
        while (isShuffling || isDealingCards || isDrawingCard || isReturningCards)
            yield return new WaitForSeconds(0.05f);

        // Wait until all cards are in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        //Debug.Log("> Dealing cards!");
        isDealingCards = true;

        yield return new WaitForSeconds(1.0f);

        CardMovement lastCard = null;
        for (int i = 0; i < tableauPiles.Count; i++)
        {
            for (int j = 0; j <= i; j++) {
                // Move card to tableau pile
                GameObject card = shuffledDeck.Pop(); // Pop card from deck
                lastCard = card.gameObject.GetComponent<CardMovement>(); // Get reference to movement script
                tableauPiles[i].GetComponent<Tableau>().AddToFaceDownPile(card); // Move card to tableau pile

                // Play sound
                card.GetComponent<CardSound>().PlayRandomMoveSound();

                // Delay before next iteration
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.1f);
        }

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        //StartCoroutine(FlipTableauCards());

        // Flip the top card of each tableau pile
        for (int i = 0; i < tableauPiles.Count; i++)
        {
            // Flip top card of each Tableau pile

            Tableau tableau = tableauPiles[i].GetComponent<Tableau>();
            StartCoroutine(tableau.FlipTopCard());

            //// Delay before next iteration
            //yield return new WaitForSeconds(0.25f);

            // Wait until the last card is in place
            while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                yield return new WaitForSeconds(0.05f);
        }

        isDealingCards = false;
    }
    

    IEnumerator ReturnWasteToStockPile(Stack<GameObject> pile)
    {
        isReturningCards = true;
        while (pile.Count > 0)
        {
            GameObject card = pile.Pop(); // Pop card from stack
            card.transform.position += new Vector3(0.0f, 0.0f, -50.0f); // Move card up to avoid collision
            AddToShuffledDeck(card);  // Return card to deck
            yield return new WaitForSeconds(0.05f); // Delay before next iteration
        }
        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        isReturningCards = false; // Set bool
        yield break;
    }

    // Returns all cards to the deck
    public IEnumerator ReturnAllToDeck()
    {
        // Debug.Log("> Returning all cards to deck!");

        // Wait until other operations have finished
        while(isShuffling || isReturningCards || isDrawingCard || isDealingCards)
            yield return new WaitForSeconds(0.05f);

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        // Play sound
        if (Waste.Instance.pile.Count > 0)
        {
            gameObject.GetComponent<CardSound>().PlaySound("fan");
            StartCoroutine(ReturnWasteToStockPile(Waste.Instance.pile)); // Return cards from Waste pile
        }

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        // Return cards from Tableau piles
        for (int i = 0; i < tableauPiles.Count; i++)
        {
            Tableau tableau = tableauPiles[i].GetComponent<Tableau>();
            StartCoroutine(ReturnWasteToStockPile(tableau.pile)); // Face-up pile
            //while (isReturningCards)
            //    yield return new WaitForSeconds(0.05f);
            StartCoroutine(ReturnWasteToStockPile(tableau.faceDownPile)); // Face-down pile
            //while (isReturningCards)
            //    yield return new WaitForSeconds(0.05f);
        }

        // Return cards from Foundation piles
        for (int i = 0; i < foundationPiles.Count; i++)
        {
            StartCoroutine(ReturnWasteToStockPile(foundationPiles[i].GetComponent<Foundation>().pile));
            //while (isReturningCards)
            //    yield return new WaitForSeconds(0.05f);
        }

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isReturningCards = false;


        yield break;
    }


    // Draws all of the cards from the deck to the Waste pile (for testing purposes)
    public IEnumerator DrawAll()
    {
        if(!LogicManager.debugMode) yield break;
        if (shuffledDeck.Count == 0)
            yield break;

        isReturningCards = true;
        // Play sound
        //gameObject.GetComponent<CardSoundScript>().PlaySound("fan");

        while (shuffledDeck.Count > 0)
        {
            GameObject card = shuffledDeck.Pop();
            Waste.Instance.AddToPile(card);  // Pop card from Waste pile and add card to deck
            card.GetComponent<CardSound>().PlayRandomFlipSound();
            yield return new WaitForSeconds(0.05f); // Delay before next iteration
        }

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isReturningCards = false;
    }

    // Check all Tableau piles to see if they have any face down cards left
    public void CheckForAutoWin()
    {
        if(!LogicManager.debugMode && !LogicManager.isAutoPlayAllowed ) return;
        for (int i = 0; i < tableauPiles.Count; i++)
        {
            Tableau tableau = tableauPiles[i].GetComponent<Tableau>();
            if (tableau.faceDownPile.Count > 0)
            {
                // If any Tableau piles still have face down cards, game has not been won
                return;
            }
        }
        // If no face down cards were found, game has been won
        AutoWinGame();
    }

    public void AutoWinGame()
    {
        if(!LogicManager.debugMode && !LogicManager.isAutoPlayAllowed ) return;
        // Play win sound
        //sound.PlaySound("treasure");
        sound.PlaySound("win1"); // Play autoWin sound

        // If autoPlay is not already on, turn it on
        if (!LogicManager.isAutoPlayOn)
        {
            // Start AutoPlay
            LogicManager.Instance.AutoPlay();
        }
    }

    // Autoplace card on foundation if valid move exists
    public bool AutoMoveCardToFoundation(GameObject card)
    {
        card.transform.position += new Vector3(0.0f, 0.0f, -30.0f); // Move card up to avoid collisions
        PlayingCard cardScript = card.GetComponent<PlayingCard>();
        cardScript.previousPile = cardScript.pile; // Set previous pile
        // Get reference to foundation of matching suit
        Foundation foundation = foundationPiles[cardScript.suit].GetComponent<Foundation>();

        bool isValid = cardScript.Place(foundation.gameObject);
        if (!isValid)
            cardScript.Drop(); // Return card to its previous pile
        else
        {
            autoMoves++; // Increment autoMove counter
            LogicManager.IncrementMoveCounter(); // Increment move counter
            noMovesLeft = false; // Count as valid move

            // Check previous pile to see if it needs to flip its top card
            // Only relevant if previous pile was a Tableau pile
            if (cardScript.previousPile != null && cardScript.previousPile.CompareTag("Tableau"))
            {
                // Get reference to Tableau script
                Tableau prevTableau = cardScript.previousPile.GetComponent<Tableau>(); 
                // If tableau has no more face up cards, flip the top face down card
                if (prevTableau.pile.Count == 0)
                {
                    StartCoroutine(prevTableau.FlipTopCard());
                    // Check if all face down cards have been flipped
                    //CheckForWin();
                }
            }
        }

        return isValid; // Return bool

    }

    // Check each Tableau to see if card can be moved to any of them
    public bool AutoMoveCard(GameObject card)
    {
        bool isValid = false;

        //UnityEngine.Debug.Log("> Automoving: " + card.name);
        PlayingCard cardScript = card.GetComponent<PlayingCard>(); // Get reference to card script

        card.transform.position += new Vector3(0.0f, 0.0f, -30.0f); // Move card up to avoid collisions
        cardScript.previousPile = cardScript.pile; // Set previous pile

        // Iterate through each tableau from right to left
        //for (int i = 0; i < tableauPiles.Count; i++)
        // Iterate through each tableau from left to right (Hopefully this will result in cards moving leftward more often)
        for (int i = tableauPiles.Count - 1; i >= 0; i--)
        {
            if (tableauPiles[i] == cardScript.pile) // Dont check the pile that the card was already in
                continue;

            // Check top card of tableau
            Tableau tableau = tableauPiles[i].GetComponent<Tableau>();
            PlayingCard topCard = null;
            if (tableau.pile.Count > 0) // Only if pile is not empty
                topCard = tableau.pile.Peek().GetComponent<PlayingCard>();

            isValid = cardScript.Place(tableauPiles[i]);
            // Attempt to place card on tableau pile
            if (isValid)
            {
                // If move was valid:
                autoMoves++; // Increment auto move counter
                LogicManager.IncrementMoveCounter(); // Increment move counter

                // Check previous pile to see if it needs to flip its top card
                // Only relevant if previous pile was a Tableau pile
                if (cardScript.previousPile != null && cardScript.previousPile.CompareTag("Tableau"))
                {
                    Tableau prevTableau = cardScript.previousPile.GetComponent<Tableau>(); // Get reference to Tableau script
                    // If tableau has no more face up cards, flip the top face down card
                    if (prevTableau.pile.Count == 0)
                    {
                        noMovesLeft = false; // Set noMovesLeft bool
                        StartCoroutine(prevTableau.FlipTopCard());
                        // Check if all face down cards have been flipped
                        //CheckForWin();
                    }
                    // If previous tableau is not empty, check top card to see if its the same rank as the top card of the new tableau
                    else if (topCard != null)
                    {
                        PlayingCard prevTopCard = prevTableau.pile.Peek().GetComponent<PlayingCard>();
                        // If ranks are the same then we are moving card back and forth between two piles
                        if (prevTopCard.rank == topCard.rank)
                            autoMoves--; // Don't count this move towards autoMove counter
                        else // Otherwise count as valid move
                            noMovesLeft = false;  // Count as valid move
                    }
                }
                else
                {
                    noMovesLeft = false; // Count as valid move
                }

                // If move was valid, return true
                return true; // Exit function
            }
        }
        // If no moves were valid, attempt to move card to foundation
        //isValid = AutoMoveCardToFoundation(card);

        // If no moves were valid, drop card
        if (!isValid)
            cardScript.Drop();

        return isValid;
    }
    
    public IEnumerator AutoMoveStack(Stack<GameObject> stack)
    {
        // if stack is empty, break
        if (stack.Count == 0)
            yield break;

        PlayingCard cardScript = stack.Peek().GetComponent<PlayingCard>(); // Get reference to card script
        // Iterate through each tableau
        for (int i = 0; i < tableauPiles.Count; i++)
        { 
            if (tableauPiles[i] == cardScript.pile) // Dont check the pile that the card was already in
                continue;

            // Attempt to place card on tableau pile
            if (cardScript.Place(tableauPiles[i]))
            { // If move was valid
                autoMoves++; // Increment autoMove counter
                LogicManager.IncrementMoveCounter(); // Increment move counter
                noMovesLeft = false; // Set noMovesLeft bool

                // If move was valid, pop from stack
                stack.Pop();
                // Place rest of stack on that tableau
                while (stack.Count > 0)
                {
                    yield return new WaitForSeconds(0.05f); // Delay before next iteration
                    cardScript = stack.Pop().GetComponent<PlayingCard>();
                    if (!cardScript.Place(tableauPiles[i]))
                        cardScript.Drop();
                }

                // If move was valid:
                // Check previous pile to see if it needs to flip its top card
                GameObject previousPile = cardScript.previousPile; // Get reference to previous pile
                // Only relevant if previous pile was a Tableau pile
                if (previousPile != null && previousPile.CompareTag("Tableau"))
                {
                    Tableau tableau = previousPile.GetComponent<Tableau>(); // Get reference to Tableau script
                    // If tableau has no more face up cards, flip the top face down card
                    if (tableau.pile.Count == 0)
                    {
                        StartCoroutine(tableau.FlipTopCard());
                        // Check if all face down cards have been flipped
                        //CheckForWin();
                    }
                }
                yield break;
            }

        }

        // If no valid moves were found, drop all of the cards in the stack
        while (stack.Count > 0)
        {
            stack.Pop().GetComponent<PlayingCard>().Drop();
        }

        yield break;
    }


    public IEnumerator AutoPlayPass()
    {
        if(!LogicManager.debugMode && !LogicManager.isAutoPlayAllowed ) yield break;
        /*
        Each AutoPlay-Pass will repeatedly check the Tableau piles from right to left
        (To increase the chances of uncovering face - down cards) and first attempt to autoMove
        the top card of the pile.If a top card is successfully moved, it will attempt to move
        the card under it. It will repeat this until no valid moves can be made with the top card
        of that stack. Once it has exhausted moves for the top card, it will attempt to move the
        full stack of face-up cards from that pile to another Tableau. It will then move to the
        next Tableau and repeat the process. 
        
        Once all Tableaus have been checked, it will check to see if any valid moves were made 
        during that Tableau-Pass. If any valid moves were made, it will repeat the Tableau-Passes 
        until no valid moves can be made from any Tableau piles. 
        (If a card is being repeatedly moved back and forth between the same stacks, this will not 
        be counted, so as to prevent infinite loops)
        
        Once no more moves can be made from the Tableau piles, a Waste-Pass will be performed, 
        which will draw a card from the deck and attemp to make a valid move for it. If no valid 
        move can be made, it will draw another card and attempt to make a move for that one. This
        will repeat until either a valid move is made, or the Stock runs out of cards. 

        Once All moves have been exhausted for the Tableau piles, and at least one valid move has 
        been made from the Waste pile, the AutoPlay-Pass is finished.
        
        (If the Waste-Pass cycles through the entire Stock and doesn't make any valid moves, then
        no moves can be made, and game has been lost) *(unless returning cards from the foundations 
        can make more moves possible but programming that seems like a massive pain)
         */

        if (!LogicManager.isAutoPlayOn)
            yield break;

        isAutoPlaying = true;
        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        // Autoplay each tableau:
        do
        {
            autoMoves = 0;// Set counter to zero

            if (!LogicManager.isAutoPlayOn)
                break;

            // Iterate through tableau piles from right to left (Hopefully this will have a better chance of uncovering face-down cards)
            for (int i = tableauPiles.Count - 1; i >= 0; i--)
            {
                if (!LogicManager.isAutoPlayOn)
                    break;

                // Wait until the last card is in place
                while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                    yield return new WaitForSeconds(0.05f);

                Tableau tableau = tableauPiles[i].GetComponent<Tableau>();

                StartCoroutine(tableau.AutoPlayPass());
                // Wait until finished
                while (tableau.isAutoPlaying)
                    yield return new WaitForSeconds(0.05f);

            }
        } while (autoMoves > 0);

        if (!LogicManager.isAutoPlayOn)
        {
            isAutoPlaying = false;
            yield break;
        }

        // AutoPlay Waste pile
        StartCoroutine(Waste.Instance.AutoPlayPass());
        // Wait until finished
        while (Waste.isAutoPlaying)
            yield return new WaitForSeconds(0.1f);

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isAutoPlaying = false;

        yield break;
    }

    // Begin autoplaying
    public IEnumerator AutoPlay()
    {
        if(!LogicManager.debugMode && !LogicManager.isAutoPlayAllowed ) yield break;
        
        // AutoPlay will continuously execute AutoPlay passes until the foundations are all full

        bool foundationsFull = false;
        noMovesLeft = false;
        while (LogicManager.isAutoPlayOn && !foundationsFull && !noMovesLeft)
        {
            // Wait until the last card is in place
            while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                yield return new WaitForSeconds(0.05f);

            // If any valid moves are made during the AutoPlay pass
            // this bool will be set to false
            noMovesLeft = true;

            // Repeatedly perform AutoPlay passes until all foundations are full
            StartCoroutine(AutoPlayPass());

            // Wait until pass has finished
            while (isAutoPlaying)
                yield return new WaitForSeconds(0.1f);

            // We can try running two full passes before checking if any moves are left
            // Just to make extra sure
            StartCoroutine(AutoPlayPass());
            // Wait until pass has finished
            while (isAutoPlaying)
                yield return new WaitForSeconds(0.1f);

            // Check if foundations are full
            foundationsFull = true;
            for (int i = 0; i < foundationPiles.Count; i++)
            {
                Foundation foundation = foundationPiles[i].GetComponent<Foundation>();
                if (foundation.pile.Count < 13)
                    foundationsFull = false; // If any foundations are not full, set bool to false
            }
            // If all foundations are full, game has been won
            if (foundationsFull)
                LogicManager.WinGame();

            // If game won, end AutoPlay
            if (LogicManager.isWon || !LogicManager.isAutoPlayOn)
                yield break;

            // If no moves were made during the last two passes,
            // then there are no moves left
            if (noMovesLeft)
            {
                //UnityEngine.Debug.Log(" >>> No Moves Left!");
                LogicManager.NoMovesLeft();
                yield break;
            }

            yield return new WaitForSeconds(0.1f); // Delay
        }
        // If no moves left, disable autoPlay
        if (noMovesLeft)
            LogicManager.NoMovesLeft();

        yield break;
    }
}
