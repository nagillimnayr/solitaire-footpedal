using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSound : MonoBehaviour
{
    // Dictionary to store the different card sound effects
    public static Dictionary<string, AudioClip> cardSounds = new Dictionary<string, AudioClip>();
    public static bool isDictionaryInitialized = false;

    // Reference to AudioSource component
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to AudioSource component
        audioSource = gameObject.GetComponent<AudioSource>();

        if (isDictionaryInitialized)
            return; // If dictionary has already been initialized, end function

        string key, path, n;

        // Add sound effects to Dictionary
        for (int i = 1; i <= 4; i++)
        {
            n = i.ToString();
            path = "cardPlace" + n;
            key = "place" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 3; i++)
        {
            n = i.ToString();
            path = "Card_Game_Movement_Deal_Single_0" + n;
            key = "move" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 3; i++)
        {
            n = i.ToString();
            path = "Card_Game_Movement_Deal_Single_Whoosh_Light_0" + n;
            key = "whoosh" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 8; i++)
        {
            n = i.ToString();
            path = "cardSlide" + n;
            key = "flip" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 2; i++) // Ding sounds
        {
            n = i.ToString();
            path = "Vibrant_Game__Slot_Machine_Ding_" + n;
            key = "ding" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 4; i++) // Win sounds
        {
            n = i.ToString();
            path = "Vibrant_Game__Slot_Machine_Win_" + n;
            key = "win" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 5; i++) // Positive Tap sounds
        {
            n = i.ToString();
            path = "Vibrant_Positive_Tap_" + n;
            key = "tap" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 4; i++) // Achievement sounds
        {
            n = i.ToString();
            path = "Vibrant_Game__Positive_Achievement_" + n;
            key = "achievement" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        for (int i = 1; i <= 4; i++) // Bling sounds
        {
            n = i.ToString();
            path = "Vibrant_Game__Tone_Bling_" + n;
            key = "bling" + n;
            AudioClip clip = Resources.Load<AudioClip>(path); // Load clip
            if (!cardSounds.TryAdd(key, clip))// Try to add clip to Dictionary
                UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        }
        key = "bling5";
        if(!cardSounds.TryAdd(key, Resources.Load<AudioClip>("Vibrant_Game__Bling_1")))
            UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");
        key = "bling6";
        if (!cardSounds.TryAdd(key, Resources.Load<AudioClip>("Vibrant_Game__Positive_Sweep_Bling")))
            UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");

        // Shuffle sounds
        key = "shuffle";
        if (!cardSounds.TryAdd("shuffle", Resources.Load<AudioClip>("Card_Game_Movement_Shuffle_03")))
            UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");

        key = "fan";
        if (!cardSounds.TryAdd("fan", Resources.Load<AudioClip>("cardFan1")))
            UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");

        key = "treasure";
        if (!cardSounds.TryAdd(key, Resources.Load<AudioClip>("Vibrant_Game__Treasure")))
            UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");

        // Negative sound for when no moves are left
        key = "negative";
        if (!cardSounds.TryAdd(key, Resources.Load<AudioClip>("Negative_Alert")))
            UnityEngine.Debug.Log("> Failed to add Key: ( " + key + " ) to the dictionary");

        // Set bool
        isDictionaryInitialized = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    // Play sound
    public bool PlaySound(string key)
    {
        if (audioSource == null)
        {
            UnityEngine.Debug.Log("> Error!: Audio Source uninitialized");
            return false;
        }

        AudioClip clip;
        if (cardSounds.TryGetValue(key, out clip))
        {
            audioSource.clip = clip;
            audioSource.Play();
            return true;
        }
        else
        {
            UnityEngine.Debug.Log("> Error!: Key ( " + key + " ) not found in dictionary");
            return false;
        }
    }

    public void PlayRandomMoveSound()
    {
        string key = "move" + UnityEngine.Random.Range(1, 4).ToString();
        if (cardSounds.ContainsKey(key))
        {
            audioSource.clip = cardSounds[key];
            audioSource.Play();
        }
        else
        {
            UnityEngine.Debug.Log("> Error!: Key ( " + key + " ) not found in dictionary");
        }
    }
    public void PlayRandomFlipSound()
    {
        string key = "flip" + UnityEngine.Random.Range(1, 9).ToString();
        if (cardSounds.ContainsKey(key))
        {
            audioSource.clip = cardSounds[key];
            audioSource.Play();
        }
        else
        {
            UnityEngine.Debug.Log("> Error!: Key ( " + key + " ) not found in dictionary");
        }
    }
}
