using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentHandler : MonoBehaviour
{
    [Header("Condition Camera's")]
    [SerializeField] GameObject camera2D;
    [SerializeField] GameObject VRHcamera;
    [SerializeField] GameObject VRGcamera;

    Session exp;
    ExperimentGenerator experimentGenerator;
    TrialRunner trialRunner;
    InstructionHandler instrHandler;

    bool isDebugging = false;
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
       
        yield return StartCoroutine(ShowInstructions());

        yield return StartCoroutine(RunPractice());

        if (isDebugging) { 
            Block mainBlock = experimentGenerator.GenerateTrialBlock("main", 10); } // Show only subset if in debug mode
        else { Block mainBlock = experimentGenerator.GenerateTrialBlock("main"); }

        yield return StartCoroutine(ShowStartScren());
        yield return StartCoroutine(RunTrials());
        yield return StartCoroutine(ShowEndScreen());

        Application.Quit();
    }

    private void RunConditionSettup()
    {
        string condition = (string)exp.participantDetails["condition"];
        
        switch (condition)
        {
            case "2D":
                break;
            case "VRH":
                VRHcamera.SetActive(true);
                break;
            case "VRG":
                VRGcamera.SetActive(true);
                break;
            default:
                Debug.Log("Error loading condition");
                break;
        }
    }

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
    IEnumerator ShowEndScreen()
    {
        instrHandler.ShowExpEnd();
        while (instrHandler.IsShowingInstruction) { yield return null; }
    }

    IEnumerator RunPractice()
    {
        do
        {
            int pracTrialCount = exp.settings.GetInt("practiceBlockN");
            Block pracBlock = experimentGenerator.GenerateTrialBlock("practice", pracTrialCount);
            yield return StartCoroutine(RunTrials());
            practicePassed = CheckAccuracy(pracBlock);
            if (!practicePassed) { yield return StartCoroutine(ShowPracFailWarning()); }
        } while (!practicePassed);
    }
    IEnumerator RunTrials()
    {
        trialRunner.allTrialsComplete = false;
        while (!trialRunner.allTrialsComplete)
        {
            bool runBreak = BreakCheck();
            if (runBreak && !exp.InTrial) { yield return StartCoroutine(ShowBreakScren()); }
           
            if (!exp.InTrial) { trialRunner.RunTrial(); }

            yield return null;
        }
    }

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
}
