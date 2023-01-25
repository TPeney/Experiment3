using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the flow and execution of each element of the experiment.
/// </summary>
public class ExperimentHandler : MonoBehaviour
{
    [Header("Condition Camera's")]
    [SerializeField] GameObject camera2D;
    [SerializeField] GameObject VRHcamera;
    [SerializeField] GameObject VRGcamera;
    public GameObject ActiveCamera { private set; get; }

    [Header("References to the input actions to respond to each target")]
    [SerializeField] InputActionReference target1Response;
    [SerializeField] InputActionReference target2Response;

    Session exp;
    ExperimentGenerator experimentGenerator;
    TrialRunner trialRunner;
    InstructionHandler instrHandler;

    bool isDebugging = false;
    bool pauseRequested = false;
    bool experimentRunning = false;
    bool practicePassed = false;

    void Start()
    {
        exp = Session.instance;
        trialRunner = FindObjectOfType<TrialRunner>();
        instrHandler = FindObjectOfType<InstructionHandler>();
        experimentGenerator = GetComponent<ExperimentGenerator>();
    }

    void Update()
    {
        if (!exp.hasInitialised || experimentRunning) { return; }

        isDebugging = exp.settings.GetBool("isDebugging");
        StartCoroutine(RunExperiment());
    }

    IEnumerator RunExperiment()
    {
        experimentRunning = true;
        RunConditionSettup();

        if (ActiveCamera != camera2D) { yield return StartCoroutine(ShowCameraResetInstructions()); }

        yield return StartCoroutine(ShowInstructions());

        yield return StartCoroutine(RunPractice());

        if (isDebugging)
        {
            Block mainBlock = experimentGenerator.GenerateTrialBlock(blockName: "main", trialN: 10); ;
        } // Show only subset if in debug mode
        else { Block mainBlock = experimentGenerator.GenerateTrialBlock("main"); }

        yield return StartCoroutine(ShowStartScren());
        yield return StartCoroutine(RunTrials());
        yield return StartCoroutine(ShowEndScreen());

        Application.Quit();
    }

    // Activates the camera associated with the given condition. 
    private void RunConditionSettup()
    {
        string condition = (string)exp.participantDetails["condition"];

        switch (condition)
        {
            case "2D":
                ActiveCamera = camera2D;
                Cursor.visible = false;
                break;
            case "VRH":
                camera2D.SetActive(false);
                VRHcamera.SetActive(true);
                ActiveCamera = VRHcamera;
                break;
            case "VRG":
                camera2D.SetActive(false);
                VRGcamera.SetActive(true);
                ActiveCamera = VRGcamera;
                break;
            default:
                Debug.Log("Error loading condition");
                break;
        }

        SetInputBindings();
    }
    
    // Swaps which 'hand' is used to respond to each target     
    private void SetInputBindings()
    {
        int controlScheme = Session.instance.settings.GetInt("controlScheme");

        if (controlScheme == 1) { return; } // Target 1 as left hand response is set by default 
        if (controlScheme == 2)
        {
            target1Response.action.ApplyBindingOverride("<ValveIndexController>{RightHand}/primaryButton"); 
            target1Response.action.AddBinding("<Keyboard>/p");
            target1Response.action.AddBinding("<OculusTouchController>{RightHand}/primaryButton");

            target2Response.action.ApplyBindingOverride("<ValveIndexController>{LeftHand}/primaryButton");
            target2Response.action.AddBinding("<Keyboard>/q");
            target2Response.action.AddBinding("<OculusTouchController>{LeftHand}/primaryButton");
        }
    }

    // Helper method to check a Practice block was completed with minimal errors
    private bool CheckAccuracy(Block pracBlock)
    {
        int nCorrect = 0;
        foreach (Trial trial in pracBlock.trials)
        {
            if ((bool)trial.result["trialPassed"]) { nCorrect++; }
        }

        int passTarget = exp.settings.GetInt("practicePassPercent");
        int passRate = Mathf.RoundToInt((nCorrect / pracBlock.trials.Count) * 100);

        return passRate >= passTarget;
    }

    // Info screen methods
    IEnumerator ShowCameraResetInstructions()
    {
        instrHandler.ShowCameraResetInstructions();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }

    IEnumerator ShowInstructions()
    {
        instrHandler.ShowExpInstructions();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }
    IEnumerator ShowStartScren()
    {
        instrHandler.ShowExpStart();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }
    IEnumerator ShowPracFailWarning()
    {
        instrHandler.ShowPracFailWarning();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }
    IEnumerator ShowBreakScren()
    {
        instrHandler.ShowBreak();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }
    IEnumerator ShowPauseScren()
    {
        instrHandler.ShowPause();
        while (instrHandler.IsShowingInstruction) { yield return null; }
        pauseRequested = false;
    }

    IEnumerator ShowEndScreen()
    {
        instrHandler.ShowExpEnd();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }

    IEnumerator RunPractice()
    {
        do
        {
            Block pracBlock;
            int session = Session.instance.number;
            if (session == 1)
            {
                int pracTrialCount = exp.settings.GetInt("practiceBlockN");
                pracBlock = experimentGenerator.GenerateTrialBlock(blockName: "practice", trialN: pracTrialCount);
            }
            else
            {
                int pracTrialCount = exp.settings.GetInt("warmUpBlockN");
                pracBlock = experimentGenerator.GenerateTrialBlock(blockName: "practice", trialN: pracTrialCount);
            }

            yield return StartCoroutine(RunTrials());
            practicePassed = CheckAccuracy(pracBlock);
            if (!practicePassed) { yield return StartCoroutine(ShowPracFailWarning()); }
        } while (!practicePassed); // Keep running practice trials until passed with sufficent accuracy 
    }

    /// <summary>
    /// Coroutine to initalize trials and breaks.
    /// </summary>
    /// <returns></returns>
    IEnumerator RunTrials()
    {
        trialRunner.allTrialsComplete = false;
        while (!trialRunner.allTrialsComplete)
        {
            bool runBreak = BreakCheck();

            if (pauseRequested && !exp.InTrial) { yield return StartCoroutine(ShowPauseScren()); }

            if (runBreak && !exp.InTrial) { yield return StartCoroutine(ShowBreakScren()); }
           
            if (!exp.InTrial) { trialRunner.BeginTrial(); }

            yield return null;
        }
    }

    // Helper method for RunTrials() to check if a break is needed 
    bool BreakCheck()
    {
        // # of breaks are evenly spaced between first and last trial
        if (exp.currentBlockNum == 0) { return false; }
        if (exp.CurrentBlock.settings.GetString("blockName") != "main") { return false; }
        if (exp.CurrentTrial == exp.LastTrial) { return false; }

        int breakN = exp.settings.GetInt("numberOfBreaks");
        int totalTrialN = exp.CurrentBlock.trials.Count;
        int breakIndex = Mathf.FloorToInt(totalTrialN / breakN);
        int thisTrialIndex = exp.CurrentTrial.numberInBlock;

        // Returns true if the current trial index falls on a break index
        return (thisTrialIndex % breakIndex == 0);
    }

    public void PausePressed(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        pauseRequested = true;
    }
}
