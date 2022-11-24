using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrialRunner : MonoBehaviour
{
    [SerializeField] InputActionProperty respondedTarg1Action;
    [SerializeField] InputActionProperty respondedTarg2Action;
    bool respondedTarg1;
    bool respondedTarg2;
    int response;
    bool trialPassed = true;

    StimuliController[] stimuliArray;


    //TODO - change from serialized to set from a script
    [SerializeField] int loomingStimIndex;
    [SerializeField] int recedingStimIndex;
    [SerializeField] int targetLocationIndex;

    StimuliController loomingStim;
    StimuliController recedingStim;

    StimuliController targetStim;
    [SerializeField] int targetType;



    void Start()
    {
        GameObject arrayHolder = GameObject.FindGameObjectWithTag("Array");
        stimuliArray = arrayHolder.GetComponentsInChildren<StimuliController>();
        StartCoroutine(RunTrial());
    }

    void Update()
    {
        ReadInput();
    }

    // Main Trial Loop
    public IEnumerator RunTrial()
    {
        loomingStim = stimuliArray[loomingStimIndex];
        recedingStim = stimuliArray[recedingStimIndex];
        DisplayTargets(true);
        yield return StartCoroutine(AwaitResponse());
        CleanUp();
    }

    // Shows / hides target and all distractors
    private void DisplayTargets(bool toggle)
    {
        targetStim = stimuliArray[targetLocationIndex];
        targetType = targetType;
        foreach (StimuliController stim in stimuliArray)
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

        return true;
    }

    // Reads the current status of the controllers at each update
    private void ReadInput()
    {
        respondedTarg1 = respondedTarg1Action.action.triggered;

        respondedTarg2 = respondedTarg2Action.action.triggered;
    }

    // Resets all objects and values for the next trial
    private void CleanUp()
    {
        DisplayTargets(false);
    }

}
