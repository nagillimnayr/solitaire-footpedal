using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;

public class CardMovement : MonoBehaviour
{
    // References
    [FormerlySerializedAs("input")] [SerializeField] InputManager inputManager;
    // Movement properties
    public bool isMoving = false; // Used to control movement in Update function
    Vector3 targetPosition; // Target position to move to

    public static float moveSpeed = 150.0f; // Distance to move per second

    public static Vector3 cardOffsetX = new Vector3(1.5f, 0, 0); // Vector3 for card offset in X-Axis (For drawing 3 cards into the waste pile)
    public static Vector3 cardOffsetY = new Vector3(0, -1.5f, 0); // Vector3 for card offset in Y-Axis (For stacking cards in Tableau)
    public static Vector3 cardOffsetZ = new Vector3(0.0f, 0.0f, -1.0f); // Vector3 for card offset in Z-axis (for stacking cards on top of each other)

    // Start is called before the first frame update
    void Start()
    {
        // Get references
        inputManager = GameObject.Find("Input Manager").GetComponent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    { 
        // Move card to target position
        if (isMoving)
        {
            // Move card towards target position
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if currrent position and target position are approximately equal
            if (Vector3.Distance(gameObject.transform.position, targetPosition) < 0.01f)
            {
                // Set to be exactly equal
                gameObject.transform.position = targetPosition;

                // Set isMoving to false
                isMoving = false;
                // Set isPickedUp to false
                gameObject.GetComponent<PlayingCard>().isPickedUp = false;

                //UnityEngine.Debug.Log("> Card in place!");
            }
        }
    }


    // Move card to target position
    public void MoveCardTo(Vector3 targetPosition)
    {
        // Update lastCard to move
        InputManager.lastCard = this;

        // Set target position
        this.targetPosition = targetPosition;

        // Set isMoving to true
        isMoving = true;

        // Move card up in Z-axis to avoid colliding with other cards
        //gameObject.transform.position += new Vector3(0.0f, 0.0f, -3.0f);
    }

}
