using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UXF;

public class TrialRunner : MonoBehaviour
{
    [SerializeField] InputActionProperty respondedTarg1Action;
    [SerializeField] InputActionProperty respondedTarg2Action;
    [SerializeField] float stimuliMovementDuration = .15f;
    int response;
    bool trialPassed = true;

    LocationCalculator planePoints;
    StimuliController[] stimuliArrayAll;
    List<int> arrayConfiguration = new();
    List<StimuliController> selectedStimuliArray = new();

    StimuliController loomingStim;
    StimuliController recedingStim;
    StimuliController targetStim;
    int targetType;

    bool waitingForResponse = false;
    double startTime;
    double endTime;

    Trial currentTrial;

    void Start()
    {
        planePoints = FindObjectOfType<LocationCalculator>();
        GameObject arrayHolder = GameObject.FindGameObjectWithTag("Array");
        stimuliArrayAll = arrayHolder.GetComponentsInChildren<StimuliController>();

        for (int i = 0; i < stimuliArrayAll.Length; i++)
        {
            stimuliArrayAll[i].MoveTo(planePoints.midPlanePoints[i]);
        }

        DisplayStimuli(false, stimuliArrayAll);        
    }

    public void RunTrial()
    {
        if (Session.instance.InTrial) { return; }

        Session.instance.BeginNextTrial();
        currentTrial = Session.instance.CurrentTrial;
        StartCoroutine(BeginTrialLoop());
    }

    // Main Trial Loop
    public IEnumerator BeginTrialLoop()
    {
        LoadTrialData();
        DisplayStimuli(true);
        yield return new WaitForEndOfFrame();
        ProcessStimuliMovement();
        yield return new WaitForSeconds(0.5f);
        DisplayStimuli(false);
        DisplayTargets(true);
        yield return StartCoroutine(AwaitResponse());
        SaveResults();
        CleanUp();
    }

    private void LoadTrialData()
    {
        Settings tinfo = currentTrial.settings;
        loomingStim = stimuliArrayAll[tinfo.GetInt("loomingStimIndex")];
        recedingStim = stimuliArrayAll[tinfo.GetInt("recedingStimIndex")];
        targetStim = stimuliArrayAll[tinfo.GetInt("targetLocationIndex")];
        targetType = tinfo.GetInt("targetType");
        arrayConfiguration = tinfo.GetIntList("arrayConfiguration");

        selectedStimuliArray.Clear();
        foreach (int index in arrayConfiguration)
        {
            selectedStimuliArray.Add(stimuliArrayAll[index]);
        }
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
        DisplayStimuli(toggle, selectedStimuliArray.ToArray());
    }

    private void ProcessStimuliMovement()
    {
        //loomingStim.PlayLooming();
        Vector3 start = planePoints.nearPlanePoints[0];
        Vector3 end = planePoints.midPlanePoints[0];
        StartCoroutine(recedingStim.MoveTo(start, end, stimuliMovementDuration));
    }

    // Shows / hides target and all distractors
    private void DisplayTargets(bool toggle)
    {
        foreach (StimuliController stim in selectedStimuliArray)
        {
            if (stim != targetStim)
            {
                stim.DisplayTarget(toggle);
            }
            else
            {
                stim.DisplayTarget(toggle, targetType);
            }
        }

        startTime = Time.unscaledTimeAsDouble;
    }

    // Coroutine to loop until response is given
    private IEnumerator AwaitResponse()
    {
        waitingForResponse = true;

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

    // Saves results data to trial object
    private void SaveResults()
    {
        currentTrial.result["RT"] = endTime - startTime;

        string trialType;
        if (targetStim == loomingStim) { trialType = "looming"; }
        else if (targetStim == recedingStim) { trialType = "receding"; }
        else { trialType = "static"; }

        currentTrial.result["trialType"] = trialType;
        currentTrial.result["response"] = response;
        currentTrial.result["trialPassed"] = trialPassed;
    }

    // Resets all objects and values for the next trial
    private void CleanUp()
    {
        DisplayTargets(false);
        Session.instance.EndCurrentTrial();
    }
}
