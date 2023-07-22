using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayingCard : MonoBehaviour
{
    // References to gameObject's components
    public AudioSource audioData; // Reference to AudioSource component
    public MeshCollider meshCollider; // Reference to MeshCollider component
    public MeshRenderer meshRenderer; // Reference to Card object's Mesh Renderer component
    // so we can set the material based on the suit and rank

    // Other references
    public GameObject pile; // Reference to game object of whichever pile the card is currently in
    public GameObject previousPile; // Reference to game object of whichever pile the card was previously in


    public static string[] suitNames = new string[4] { "Diamond", "Club", "Heart", "Spade" };
  
    // Properties
    public int suit;
    public int rank;
    public bool isFaceUp = false; // Whether card is facing up or not
    public bool isPickedUp = false; // Set to true when card is picked up by mouse


    // Start is called before the first frame update
    void Start()
    {
        // Set references
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        audioData = gameObject.GetComponent<AudioSource>();
        meshCollider = gameObject.GetComponent<MeshCollider>();

       
    }

    // Update is called once per frame
    void Update()
    {


    }

    // Set rank and suit, then set material based on rank and suit
    public void InitializeCard(int suit, int rank)
    {
        this.suit = suit;
        this.rank = rank;

        string rankStr = rank.ToString();
        if (rank < 10)
        {
            rankStr = "0" + rankStr;
        }
        string path = "Mat/Blue/Blue_PlayingCards_" + suitNames[suit] + rankStr + "_00";
        // Set material based on suit and rank
        gameObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>(path);

        // Set name of card based on suit and rank
        SetName();
    }

    // Move card to target position and flip if necessary
    public void MoveCardTo(Vector3 targetPosition, bool faceUp)
    {
        // Move and rotate card
        gameObject.GetComponent<CardMovement>().MoveCardTo(targetPosition);
        gameObject.GetComponent<CardRotation>().FlipCard(faceUp);
        
    }

    // Sets the name of the card (i.e: "{Rank} of {Suit}s)
    private void SetName()
    {
        string nameStr = "";
        // If Rank is 1, it is an ace
        if (rank == 1)
        {
            nameStr = "Ace of " + suitNames[suit] + "s";
        }
        // If rank is greater than 10, it is a face card
        else if (rank > 10)
        {
            string face = "";
            switch (rank)
            {
                case 11:
                    face = "Jack";
                    break;
                case 12:
                    face = "Queen";
                    break;
                case 13:
                    face = "king";
                    break;

            }
            nameStr = face + " of " + suitNames[suit] + "s";
        }
        else
        {
            nameStr = rank.ToString() + " of " + suitNames[suit] + "s";
        }

        // Set gameObject's name
        gameObject.name = nameStr;
    }


    private void PlayCardPlaceSound()
    {

        audioData.Play();
    }

    // Pickup card
    public void Pickup(GameObject cursor)
    {
        if (isPickedUp)
        {
            return;
        }

        
        isPickedUp = true; // Set bool
        previousPile = pile; // Set reference to previous pile
        meshCollider.enabled = false; // Disable collision
        gameObject.transform.position += gameObject.transform.up * 75.0f; // Move up to prevent collision with other cards
        gameObject.transform.SetParent(cursor.transform, true); // Attach card to mouse cursor
        
        // Remove card from whichever pile it was in
        if (pile.CompareTag("Tableau")) // Card was in a tableau pile
        {
            //UnityEngine.Debug.Log("> Card picked up from Tableau pile");

            // If card was in tableau pile, we must take all of the cards on top with it
            Tableau tableau = pile.GetComponent<Tableau>(); // Get reference to tableau script

            // If top card is the one being picked up, proceed to pick up card
            // otherwise, there are cards above it that need to be pushed
            // onto the stack first
            while (tableau.pile.Count > 0 && tableau.pile.Peek() != gameObject)
            {
                tableau.pile.Peek().GetComponent<PlayingCard>().Pickup(cursor);
            }

            GameObject card = tableau.pile.Pop(); // Pop card from tableau
            InputManager.carryStack.Push(card); // Push card onto carryStack

        }
        else if (pile.CompareTag("Waste"))  // Card was in waste pile
        {
            //UnityEngine.Debug.Log("> Card picked up from Waste pile");
            if (pile.GetComponent<Waste>().pile.Peek() == gameObject)
            {
                pile.GetComponent<Waste>().pile.Pop(); // Pop from stack
                InputManager.carryStack.Push(gameObject); // Push card onto carryStack
            }
        }
        else if (pile.CompareTag("Foundation")) // Card was in a foundation pile
        {
            //UnityEngine.Debug.Log("> Card picked up from Foundation pile");
            if (pile.GetComponent<Foundation>().pile.Peek() == gameObject)
            {
                pile.GetComponent<Foundation>().pile.Pop(); // Pop from stack
                InputManager.carryStack.Push(gameObject); // Push card onto carryStack
            }
        }
        else
        {
            Drop();
        }
    }

    // Place card, return true if move was valid, false if move was invalid
    public bool Place(GameObject pile)
    {
        meshCollider.enabled = true; // Re-enable collision
        gameObject.transform.SetParent(null); // Detach from parent

        bool isValid = false;
        if (pile.CompareTag("Tableau"))
        {
            isValid = pile.GetComponent<Tableau>().AddToPile(gameObject);
        }
        if (pile.CompareTag("Foundation"))
        {
            isValid = pile.GetComponent<Foundation>().AddToPile(gameObject);
        }
        if(isValid)
        {
            // Play sound
            GetComponent<CardSound>().PlayRandomMoveSound();
        }
        return isValid;

    }

    // Drop card
    public void Drop()
    {
        meshCollider.enabled = true; // Re-enable collision
        gameObject.transform.SetParent(null); // Detach from parent

        // Move down to avoid collision with other cards
        gameObject.transform.position += new Vector3(0.0f, 0.0f, 10.0f);

        // Play sound
        GetComponent<CardSound>().PlayRandomMoveSound();

        // Return card to its previous pile
        if (previousPile.CompareTag("Tableau")) // Card was in a tableau pile
        {
            // Add back to tableau pile
            previousPile.GetComponent<Tableau>().AddToPile(gameObject);
        }
        else if (previousPile.CompareTag("Waste"))  // Card was in waste pile
        {
            // Add back to waste pile
            previousPile.GetComponent<Waste>().AddToPile(gameObject);
        }
        else if (previousPile.CompareTag("Foundation")) // Card was in a foundation pile
        {
            // Add back to foundation pile
            previousPile.GetComponent<Foundation>().AddToPile(gameObject);
        }
    }

    
}
