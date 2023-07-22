using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Cursor : MonoBehaviour
{
    // Singleton
    public static Cursor Instance { get; private set; }
    
    // References
    public PointerControls pointerControls = null;

    void Awake()
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
        if (pointerControls == null) return;
        if (InputManager.lastCard != null && InputManager.lastCard.isMoving)
            return;

        //Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);// Follow mouse

        Vector2 pointerPos = pointerControls.Pointer.PointerPosition.ReadValue<Vector2>();
        Vector3 pos = Camera.main.ScreenToWorldPoint(pointerPos);// Follow pointer
        gameObject.transform.position = new Vector3(pos.x, pos.y, -50); // Maintain Z-position
    }
}
