using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Tableau : MonoBehaviour
{
    // Properties
    public Stack<GameObject> faceDownPile = new Stack<GameObject>(); // Stack for the face-down cards because only the top one should be accessible
    public Stack<GameObject> pile = new Stack<GameObject>();


    GameObject bottomCard = null;

    public bool isAutoPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Adds card to pile, returns false if move is invalid, returns true if valid
    public bool AddToPile(GameObject card)
    {
        PlayingCard cardScript = card.GetComponent<PlayingCard>(); // get reference to card script

        // Validate move
        // If card on top of pile is 1 rank higher and a different color, then move is valid

        PlayingCard topCard = null;
        // If pile is empty, card must be a king or it must be returning to its original location
        // Otherwise, move is invalid
        if (pile.Count == 0)
        {
            // Check if card is king or if it is being flipped / returned to its previous spot
            if (cardScript.rank != 13 && cardScript.previousPile != null && cardScript.previousPile != gameObject)
            {
                //UnityEngine.Debug.Log("> Invalid Move: Card must be a king");
                return false; // Return false to indicate that move was invalid
            }
            // If it is being moved to an empty stack, set as new bottom card
            else
            {
                // Set bottom card
                bottomCard = card;
            }
        }
        else
        {
            // Get reference to top card
            topCard = pile.Peek().GetComponent<PlayingCard>();

            // If both suits are even or both odd, then they are the same color and move is invalid
            if (topCard.suit % 2 == cardScript.suit % 2)
            {
                //UnityEngine.Debug.Log("> Invalid Move: Cards must be alternating colors");
                return false; // Return false to indicate that move was invalid
            }

            // If card's rank is anything other than 1 less than the top card's rank, move is invalid
            if (topCard.rank - cardScript.rank != 1 )
            {
                //UnityEngine.Debug.Log("> Invalid Move: Cards must be in descending order");
                return false; // Return false to indicate that move was invalid
            }
            // Otherwise, move is valid
        }

        //UnityEngine.Debug.Log("> Adding card face-up to " + gameObject.name);

        // Get target position
        Vector3 targetPosition = gameObject.transform.position + CardMovement.cardOffsetZ + (CardMovement.cardOffsetY + CardMovement.cardOffsetZ) * (faceDownPile.Count + pile.Count);
        
        cardScript.MoveCardTo(targetPosition, true); // Move card to top of pile and flip face up

        pile.Push(card); // Push card onto face-up stack

        // Set card's pile
        card.GetComponent<PlayingCard>().pile = gameObject;

        return true; // Return true to indicate move was valid
    }
    public void AddToFaceDownPile(GameObject card)
    {
        //UnityEngine.Debug.Log("> Adding card face-down to " + gameObject.name);

        // Get target position
        Vector3 targetPosition = gameObject.transform.position + CardMovement.cardOffsetZ + ((CardMovement.cardOffsetY + CardMovement.cardOffsetZ) * faceDownPile.Count);
        faceDownPile.Push(card); // Push card onto top of stack
        PlayingCard cardScript = card.GetComponent<PlayingCard>(); // get reference to movement script

        card.transform.position += new Vector3(0.0f, 0.0f, -50.0f); // Move card up to avoid collision with other cards
        cardScript.MoveCardTo(targetPosition, false); // Move card to top of pile

        // Set card's pile
        cardScript.pile = gameObject;
        cardScript.previousPile = gameObject;
    }


    // Flip the card on top of Tableau pile
    public IEnumerator FlipTopCard()
    {
        if (faceDownPile.Count > 0)
        {
            // Wait until the last card is in place
            //while (input.lastCard != null && input.lastCard.isMoving)
            //    yield return new WaitForSeconds(0.05f);

            //UnityEngine.Debug.Log("> flipping top card of " + gameObject.name);
            GameObject card = faceDownPile.Pop(); // Pop card from top of stack
            card.transform.position += new Vector3(0.0f, 0.0f, -30.0f); // Move card up to avoid collision with other cards
            
            if (AddToPile(card)) // Add to face up pile
            {
                // Play sound
                card.GetComponent<CardSound>().PlayRandomFlipSound();

                // Check if all face down cards in all tableau piles have been flipped
                Deck.Instance.CheckForAutoWin();
            }
        }
        else
        {
            //UnityEngine.Debug.Log("> No cards to flip " + gameObject.name);
            yield break;
        }
    }

    public IEnumerator AutoPlayPass()
    {
        if (pile.Count == 0)
            yield break;

        if (!LogicManager.isAutoPlayOn)
            yield break;

        isAutoPlaying = true;

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        // Check top card
        bool isValid = false;
        do
        {
            if (!LogicManager.isAutoPlayOn)
                yield break;

            // Wait until the last card is in place
            while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                yield return new WaitForSeconds(0.05f);

            if (pile.Count == 0) // If pile is empty, skip
                break;

            GameObject card = pile.Pop();
            isValid = Deck.Instance.AutoMoveCardToFoundation(card); // Try to move card to foundation
            if (!isValid) // If that move was invalid:
            {
                // Wait until the last card is in place
                while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
                    yield return new WaitForSeconds(0.05f);

                // If card is a king, and there are no face down cards beneath it, 
                // then there is no point in moving it to another empty tableau
                PlayingCard cardScript = card.GetComponent<PlayingCard>();
                if (cardScript.rank == 13 && faceDownPile.Count == 0)
                    break;


                // Since the failure to move to the foundation will return the card to its
                // previous pile, we must pop it again
                if (pile.Peek() == card)
                    pile.Pop();

                // Try to move to another tableau
                isValid = Deck.Instance.AutoMoveCard(card);
                
            }

            yield return new WaitForSeconds(0.05f); // Delay before next iteration
        } while (isValid); // Keep trying to autoMove cards until a move fails


        if (!LogicManager.isAutoPlayOn)
        {
            isAutoPlaying = false;
            yield break;
        }

        // Check bottom card and move stack if possible
        PlayingCard bottom = bottomCard.GetComponent<PlayingCard>();
        Stack<GameObject> stack = new Stack<GameObject>();
        while (pile.Count > 0)
        {
            // If bottom card is a king and there are no face-down cards beneath it, dont move it
            if (bottom.rank == 13 && faceDownPile.Count == 0)
                break;
            GameObject card = pile.Pop(); // Pop cards one-by-one from pile
            card.transform.position += new Vector3(0.0f, 0.0f, -50.0f); // Move card up to avoid collisions
            PlayingCard cardScript = card.GetComponent<PlayingCard>();
            cardScript.previousPile = cardScript.pile; // Set previous pile
            stack.Push(card); // Push cards onto temp stack
        }
        if (stack.Count > 0) // Only attempt if stack is not empty
            StartCoroutine(Deck.Instance.AutoMoveStack(stack)); // Attempt to auto move the stack

        yield return new WaitForSeconds(0.05f); // Delay

        // Wait until the last card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);

        isAutoPlaying = false;
        yield break;
    }

}

