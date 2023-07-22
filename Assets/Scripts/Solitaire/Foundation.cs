using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foundation : MonoBehaviour
{
    public Stack<GameObject> pile = new Stack<GameObject>(); // Stack data structure because only the top card should be accessible

    [SerializeField] int suit; // Only one suit per foundation pile

    // References
    static InputManager _inputManager = null; // Reference to Process Input Script

    // Start is called before the first frame update
    void Start()
    {
        // Get references
        if (_inputManager == null)
        {
            _inputManager = GameObject.Find("Input Manager").GetComponent<InputManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Adds card to pile, returns false if move is invalid, returns true if valid
    public bool AddToPile(GameObject card)
    {
        // Get reference to card's script
        PlayingCard cardScript = card.GetComponent<PlayingCard>();

        // Validate move:

        // Suits must match, otherwise move is invalid
        if (cardScript.suit != this.suit)
        {
            //UnityEngine.Debug.Log("> Invalid Move: Suits must match");
            return false; // Return false to indicate that move was invalid
        }
        if (pile.Count == 0)
        {
            // If pile is empty, card must be an ace, otherwise move is invalid
            if (cardScript.rank != 1)
            {
                //UnityEngine.Debug.Log("> Invalid Move: Card must be an ace");
                return false; // Return false to indicate that move was invalid
            }
        }
        else
        {
            // Get reference to top card
            PlayingCard topCard = pile.Peek().GetComponent<PlayingCard>();

            // Top card must be one less than the card being placed, otherwise move is invalid
            if (cardScript.rank - topCard.rank != 1)
            {
                //UnityEngine.Debug.Log("> Invalid Move: Cards must be in ascending order");
                return false;
            }
        }
        // Otherwise move is valid

        //UnityEngine.Debug.Log("> Adding card: " + gameObject.name + " to foundation pile");

        // Move card to foundation pile position
        Vector3 targetPosition = transform.position + CardMovement.cardOffsetZ * (pile.Count + 1);
        //card.transform.position += new Vector3(0.0f, 0.0f, -25.0f); // Move card up to avoid collision with other cards
        cardScript.MoveCardTo(targetPosition, true);
        pile.Push(card); // Push card onto stack

        // Set card's pile
        cardScript.pile = gameObject;

        // Play positive notification sound
        if (cardScript.rank == 13) // If card was a king, foundation is full
            StartCoroutine(PlayFoundationFullSound());
        else
            StartCoroutine(PlaySuccessSound());

        return true; // Return true to indicate move was valid
    }

    IEnumerator PlaySuccessSound()
    {
        // Wait until card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        GetComponent<CardSound>().PlaySound("achievement4");
    }
    IEnumerator PlayFoundationFullSound()
    {
        // Wait until card is in place
        while (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            yield return new WaitForSeconds(0.05f);
        GetComponent<CardSound>().PlaySound("achievement1");
    }

}
