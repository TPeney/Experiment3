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
    bool respondedTarg1; bool respondedTarg2;
    int response;
    bool trialPassed = true;

    StimuliController[] stimuliArrayAll;
    [SerializeField] int[] arrayConfiguration;
    List<StimuliController> selectedStimuliArray = new();


    StimuliController loomingStim;
    StimuliController recedingStim;
    StimuliController targetStim;
    int targetType;

    Trial currentTrial;

    void Start()
    {
        GameObject arrayHolder = GameObject.FindGameObjectWithTag("Array");
        stimuliArrayAll = arrayHolder.GetComponentsInChildren<StimuliController>();

        DisplayStimuli(false, stimuliArrayAll);
    }

    void Update()
    {
        ReadInput();
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
        Debug.Break();
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
        selectedStimuliArray.Clear();
        foreach (int index in arrayConfiguration)
        {
            selectedStimuliArray.Add(stimuliArrayAll[index]);
        }

        Settings tinfo = currentTrial.settings;
        loomingStim = stimuliArrayAll[tinfo.GetInt("loomingStimIndex")];
        recedingStim = stimuliArrayAll[tinfo.GetInt("recedingStimIndex")];
        targetStim = stimuliArrayAll[tinfo.GetInt("targetLocationIndex")];
        targetType = tinfo.GetInt("targetType");
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
        loomingStim.PlayLooming();
        recedingStim.PlayReceding();
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
    }

    // Coroutine to loop until response is given
    private IEnumerator AwaitResponse()
    {
        bool responseReceived = false;

        while (!responseReceived)
        {
            responseReceived = CheckInput();
            yield return null;
        }
    }

    // Check current response values and save a response if given
    private bool CheckInput()
    {
        if (!respondedTarg1 && !respondedTarg2) { return false; }

        if (respondedTarg1)
        {
            response = 1;
        }
        else if (respondedTarg2)
        {
            response = 2;
        }

        trialPassed = response == targetType;

        return true;
    }

    // Reads the current status of the controllers at each update
    private void ReadInput()
    {
        respondedTarg1 = respondedTarg1Action.action.triggered;

        respondedTarg2 = respondedTarg2Action.action.triggered;
    }

    // Saves results data to trial object
    private void SaveResults()
    {
        //TODO - Add Time data

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
