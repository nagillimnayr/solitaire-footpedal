using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalibrationState : State
{
    private UIManager UIManager;
    private FootPedalReader footPedalReader;
    
    private GameObject calibrationScreen; // Calibration Screen
    private GameObject textGettingBaseline;
    private GameObject textInstructions;
    
    public override void Enter()
    {
        UIManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
        footPedalReader = GameObject.Find("Foot Pedal Reader").GetComponent<FootPedalReader>();
        
        calibrationScreen = GameObject.Find("Calibration Screen");
        textGettingBaseline = GameObject.Find("Text_Getting_Baseline (TMP)");
        
        
        // Show the calibration screen
        calibrationScreen.SetActive(true);
        textGettingBaseline.SetActive(true);
        
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void Exit()
    {
        // Hide the calibration screen
        calibrationScreen.SetActive(false);
    }
}
