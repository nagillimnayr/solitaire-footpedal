using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardRotation : MonoBehaviour
{
    // Rotation properties
    private bool isRotating = false; // Used to control rotation in Update function
    Vector3 targetRotation; // Target rotation to rotate to
    Vector3 pivotPoint; // Point around which to rotate
    public const float rotateSpeed = 540.0f; // Angle to rotate by per second

    public static Vector3 faceUpRotation = new Vector3(-90.0f, 0.0f, 0.0f); // Face-up rotation
    public static Vector3 faceDownRotation = new Vector3(-90.0f, 0.0f, -180.0f); // Face-down rotation



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
        {
            // Rotate card towards target rotation

            gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.Euler(targetRotation), rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(targetRotation, gameObject.transform.rotation.eulerAngles) < 0.1f)
            {
                // Set to be exactly equal
                gameObject.transform.rotation = Quaternion.Euler(targetRotation);

                // Set isRotating to false
                isRotating = false;
            }
        }
    }


    // Flip card to target rotation
    public void FlipCard(bool faceUp)
    {
        // Set target position
        if (faceUp)
        {
            targetRotation = faceUpRotation;

            transform.Rotate(0.0f, 0.0f, 1.0f); // To make sure it flips in the right direction

        }
        else
        {
            targetRotation = faceDownRotation;
        }

        isRotating = true; // Set isRotating to true
        gameObject.GetComponent<PlayingCard>().isFaceUp = faceUp; // Set isFaceUp bool
    }
}
