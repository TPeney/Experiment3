using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UXF;

/// <summary>
/// Handles running a single trial and recording data.
/// </summary>
public class TrialRunner : MonoBehaviour
{
    Trial currentTrial;

    [Header("Input Actions for the target response")]
    [SerializeField] InputActionProperty respondedTarg1Action;
    [SerializeField] InputActionProperty respondedTarg2Action;

    [Header("Trial Timings (in seconds)")] // TODO - Change these to settings
    [SerializeField] float displayTime = 0f;
    [SerializeField] float stimuliMovementDuration = .15f;
    float interTrialTime;

    // Stimuli Array Configuration
    StimuliController[] stimuliArrayAll;
    readonly List<StimuliController> selectedStimuliArray = new();
    List<int> arrayConfiguration = new();
    LocationCalculator planePoints;

    // Motion Stimuli
    StimuliController loomingStim;
    StimuliController recedingStim;
    StimuliController targetStim;

    // Trial Paramets
    int distractor;
    int targetType;
    double startTime;
    double endTime;
    int response;

    [HideInInspector] public bool allTrialsComplete = false;
    bool waitingForResponse = false;
    bool trialPassed = true;

    void Start()
    {
        planePoints = FindObjectOfType<LocationCalculator>();
        GameObject arrayHolder = GameObject.FindGameObjectWithTag("Array");
        stimuliArrayAll = arrayHolder.GetComponentsInChildren<StimuliController>();

        DisplayStimuli(false, stimuliArrayAll); // Make sure stimuli start hidden        
    }

    public void BeginTrial()
    {
        if (Session.instance.InTrial) { return; }

        Session.instance.BeginNextTrial();
        currentTrial = Session.instance.CurrentTrial;
        StartCoroutine(TrialLoop());
    }

    /// <summary>
    /// The main trial loop coroutine.
    /// </summary>
    /// <returns></returns>
    public IEnumerator TrialLoop()
    {
        LoadTrialData();
        yield return new WaitForSecondsRealtime(interTrialTime);
        DisplayStimuli(true);
        TogglePreTargets(true);
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(ProcessStimuliMovement());
        TogglePreTargets(false);
        DisplayTargets(true);
        yield return StartCoroutine(AwaitResponse());
        SaveResults();
        CleanUp();
    }

    private void LoadTrialData()
    {
        Settings tinfo = currentTrial.settings;
        interTrialTime = Session.instance.settings.GetFloat("interTrialTimeSec");
        arrayConfiguration = tinfo.GetIntList("arrayConfiguration");
        loomingStim = stimuliArrayAll[tinfo.GetInt("loomingStimIndex")];
        recedingStim = stimuliArrayAll[tinfo.GetInt("recedingStimIndex")];
        targetStim = stimuliArrayAll[tinfo.GetInt("targetLocationIndex")];
        targetType = tinfo.GetInt("targetID");

        selectedStimuliArray.Clear();

        // Set current array size based on the index configuration 
        foreach (int index in arrayConfiguration) { selectedStimuliArray.Add(stimuliArrayAll[index]); }
    }

    private void DisplayStimuli(bool toggle, StimuliController[] array)
    {
        foreach (StimuliController stim in array)
        {
            stim.DisplayMesh(toggle);
        }
    }

    private void DisplayStimuli(bool toggle)
    {
        // Overloaded method as shorthand for displaying just the selected stimuli
        DisplayStimuli(toggle, selectedStimuliArray.ToArray());
    }

    /// <summary>
    /// Coroutine to display the stimuli looming / receding movement.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ProcessStimuliMovement()
    {
        int recedingIndex = Array.IndexOf(stimuliArrayAll, recedingStim);
        int loomingIndex = Array.IndexOf(stimuliArrayAll, loomingStim);
    
        Vector3 recedeStart = planePoints.NearPlanePoints[recedingIndex];
        Vector3 recedeEnd = planePoints.MidPlanePoints[recedingIndex];

        Vector3 loomStart = planePoints.FarPlanePoints[loomingIndex];
        Vector3 loomEnd = planePoints.MidPlanePoints[loomingIndex];

        StartCoroutine(recedingStim.MoveTo(displayTime, recedeStart, recedeEnd, stimuliMovementDuration));
        StartCoroutine(loomingStim.MoveTo(displayTime, loomStart, loomEnd, stimuliMovementDuration));

        while(recedingStim.IsMoving || loomingStim.IsMoving)
        {
            yield return null;
        }
    }

    private void DisplayTargets(bool toggle)
    {
        // Select a random distractor from the two options
        if (toggle == true){distractor = Mathf.RoundToInt(UnityEngine.Random.Range(1, 2));}

        foreach (StimuliController stim in selectedStimuliArray)
        {
            if (stim != targetStim)
            {
                stim.DisplayTarget(toggle, distractor);
            }
            else
            {
                stim.DisplayTarget(toggle, targetType, isTarget: true);
            }
        }

        startTime = Time.unscaledTimeAsDouble;
    }

    private void TogglePreTargets(bool toggle)
    {
        foreach (StimuliController stim in selectedStimuliArray)
        {
            stim.DisplayTarget(toggle, targetType: 0);
        }
    }

    private IEnumerator AwaitResponse()
    {
        waitingForResponse = true; // Set to false in CheckInput()
        while (waitingForResponse)
        {
            yield return null;
        }
    }

    // Check current response values and save a response if given
    // Called as Unity Event via the Player Input inspector element
    public void CheckInput(InputAction.CallbackContext context)
    {
        if (!context.performed) { return ; }

        if (context.action.name == "Respond Target 1 Present")
        {
            response = 1;
        }
        else if (context.action.name == "Respond Target 2 Present")
        {
            response = 2;
        }

        endTime = context.startTime;
        trialPassed = response == targetType;
        waitingForResponse = false;
    }

    private void SaveResults()
    {
        currentTrial.result["RT"] = endTime - startTime;

        string trialType;
        if (targetStim == loomingStim) { trialType = "looming"; }
        else if (targetStim == recedingStim) { trialType = "receding"; }
        else { trialType = "static"; }
        currentTrial.result["trialType"] = trialType;

        currentTrial.result["arraySize"] = currentTrial.settings.GetIntList("arrayConfiguration").Count;
        currentTrial.result["targetID"] = targetType == 1 ? "S" : "H";
        currentTrial.result["response"] = response == 1 ? "S" : "H";
        currentTrial.result["trialPassed"] = trialPassed;
        currentTrial.result["condition"] = Session.instance.participantDetails["condition"];
        currentTrial.result["blockType"] = Session.instance.CurrentBlock.settings.GetString("blockName");
    }

    // Resets all objects and values for the next trial
    private void CleanUp()
    {
        DisplayStimuli(false);
        DisplayTargets(false);
        allTrialsComplete = currentTrial == Session.instance.CurrentBlock.lastTrial;
        Session.instance.EndCurrentTrial();
    }
}
