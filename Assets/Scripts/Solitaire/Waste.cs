using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Waste : MonoBehaviour
{
    // Singleton
    public static Waste Instance { get; private set; }
    
    public Stack<GameObject> pile { get; private set; } = new Stack<GameObject>(); // Stack data structure because only the top card should be accessible

    public static bool isAutoPlaying = false;
    public static bool isRearranging = false;
    public static bool isCondensing = false;
    
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
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Dont do anything if the stack is empty
        if (pile.Count == 0) return;
        
        // Dont do anything unless the game mode is Turn Three 
        if (LogicManager.gameMode != GameMode.TurnThree) return;
        // Dont do anything if there are any cards being moved
        if (InputManager.lastCard && InputManager.lastCard.isMoving) return;
        // Dont do anything if a card is currently being picked up
        if (InputManager.lastCard && InputManager.lastCard.gameObject.GetComponent<PlayingCard>().isPickedUp) return;
        if (Deck.isDrawingCard) return;
        if (isRearranging) return;
        if (isCondensing) return;

        // Check the position of the top card relative to the pile,
        // If the top card is not in the rightmost position, then
        // The top card(s) must be rearranged
        int numOfCards = pile.Count < 3 ? pile.Count : 3;  
        if (pile.Peek().transform.position.x  <
            transform.position.x + (CardMovement.cardOffsetX.x * (numOfCards - 1.0)) - 0.1f)
        {
            Stack<GameObject> tempStack = new Stack<GameObject>(numOfCards);
            for (int i = 0; i < numOfCards; i++)
            {
                tempStack.Push(pile.Pop());
            }
            for (int i = 0; i < numOfCards; i++)
            {
                AddToPile(tempStack.Pop(), i);
            }
        }
        
    }
    
    public void AddToPile(GameObject obj, int xOffset = 0)
    {
        PlayingCard card = obj.GetComponent<PlayingCard>();
        if (!card) return;

        
        // Move card to waste pile position and flip to be face up
        Vector3 targetPosition = transform.position + (CardMovement.cardOffsetZ * (pile.Count + 1)) + 
                                 (CardMovement.cardOffsetX * xOffset);
        pile.Push(obj); // Push card onto stack
        card.pile = gameObject; // Set card's pile
        
        //card.transform.position += new Vector3(0.0f, 0.0f, -10.0f); // Move card up to avoid collision with other cards
        card.MoveCardTo(targetPosition, true); // Move card to new position

    }

    public IEnumerator Rearrange()
    {
        // Only relevant if game mode is Turn Three
        if (LogicManager.gameMode != GameMode.TurnThree) yield break;
        if (pile.Count == 0) yield break;
        if (isRearranging || isCondensing) yield break;

        isRearranging = true;
        
        int numOfCards = pile.Count < 3 ? pile.Count : 3;
        
        while(InputManager.lastCard && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        while (InputManager.lastCard && InputManager.lastCard.gameObject.GetComponent<PlayingCard>().isPickedUp)
            yield return new WaitForSeconds(0.05f);
        
        // If a card has been taken, rearrange the ones underneath
        Stack<GameObject> topCards = new Stack<GameObject>(numOfCards);
        // Pop the top three cards from the pile and add them to the temp stack
        for(int i = 0; i < numOfCards; i++)
            topCards.Push(pile.Pop());
        
        for(int i = 0; i < numOfCards; i++)
        {
            // Add cards back to pile in reverse order, with appropriate offset
            GameObject card = topCards.Pop();
            AddToPile(card, i);
            
            // Wait before moving next card
            yield return new WaitForSeconds(0.05f);
        }
        
        while(InputManager.lastCard && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isRearranging = false;

    }
    
    // Move all cards in waste onto one spot
    public IEnumerator Condense()
    {
        // Only relevant if game mode is Turn Three
        if (LogicManager.gameMode != GameMode.TurnThree) yield break;
        if (pile.Count == 0) yield break;
        if (isRearranging || isCondensing) yield break;

        isCondensing = true;
        
        while(InputManager.lastCard && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        while (InputManager.lastCard && InputManager.lastCard.gameObject.GetComponent<PlayingCard>().isPickedUp)
            yield return new WaitForSeconds(0.05f);
        
        // If a card has been taken, rearrange the ones underneath
        Stack<GameObject> tempStack = new Stack<GameObject>(pile.Count);
        // Pop all of the cards from the pile and add them to the temp stack
        for(int i = 0; pile.Count > 0; i++)
        {
            GameObject card = pile.Pop();
            //card.transform.position += new Vector3(0.0f, 0.0f, -1.0f * i); // Move card up to avoid collison
            tempStack.Push(card);
        }
        
        // Put them back without any offset
        while(tempStack.Count > 2)
        {
            // Add cards back to pile in reverse order, with no offset
            AddToPile(tempStack.Pop(), 0);
            // Wait before moving next card
            yield return new WaitForSeconds(0.05f);
        }

        // Put the last two back with an offset
        for (int i = 1; i < 3; i++)
        {
            AddToPile(tempStack.Pop(), i);
            yield return new WaitForSeconds(0.05f);
        }
        
        
        while(InputManager.lastCard && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isCondensing = false;

        //StartCoroutine(Rearrange());
    }
    
    public IEnumerator AutoPlayPass()
    {
        if (pile.Count == 0 && Deck.shuffledDeck.Count == 0)
            yield break;

        if (!LogicManager.isAutoPlayOn)
        {
            isAutoPlaying = false;
            yield break;
        }

        isAutoPlaying = true;

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        bool isValid = false;

        // Continuously draw cards and try to autoMove them until either stock is empty,
        // or a valid move is made
        while (Deck.shuffledDeck.Count > 0)
        {
            if (!LogicManager.isAutoPlayOn)
                break;

            Deck.Instance.DrawCard();
            // Wait until the last card is in place
            while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                yield return new WaitForSeconds(0.05f);
            // Wait until drawing card has finished
            while (Deck.isDrawingCard)
                yield return new WaitForSeconds(0.05f);

            yield return new WaitForSeconds(0.2f); // Delay

            GameObject card = pile.Pop();
            PlayingCard cardScript = card.GetComponent<PlayingCard>();
            cardScript.previousPile = cardScript.pile; // Set previous pile
            //card.transform.position += new Vector3(0.0f, 0.0f, -50.0f); // Move card up to avoid collisions
            
            // Try to move card to foundation
            isValid = Deck.Instance.AutoMoveCardToFoundation(card);
            if (!isValid) // If that move failed, attempt to move card to a tableau pile
            {
                // Since the failure to move to the foundation will return the card to its
                // previous pile, we must pop it again
                if (pile.Peek() == card)
                    pile.Pop();
                isValid = Deck.Instance.AutoMoveCard(card);
            }
            if (isValid) // If either move was valid, break loop
                break;

            // Wait until the last card is in place
            while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                yield return new WaitForSeconds(0.05f);

            yield return new WaitForSeconds(0.1f); // Delay
        }

        if (!LogicManager.isAutoPlayOn)
        {
            isAutoPlaying = false;
            yield break;
        }

        // Return cards to stock if stock is empty
        if (Deck.shuffledDeck.Count == 0)
            StartCoroutine(Deck.Instance.DrawCard());
        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isAutoPlaying = false;

        yield break;
    }
}
