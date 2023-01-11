using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentHandler : MonoBehaviour
{
    Session exp;
    ExperimentGenerator experimentGenerator;
    TrialRunner trialRunner;
    InstructionHandler instrHandler;

    bool isDebugging = false;
    bool experimentRunning = false;

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

        if (isDebugging) { experimentGenerator.GenerateExperiment("main", 10); } // Show only subset if in debug mode
        else { experimentGenerator.GenerateExperiment("main"); }

        yield return StartCoroutine(ShowStartScren());
        yield return StartCoroutine(RunTrials());
        yield return StartCoroutine(ShowEndScreen());

        Application.Quit();
    }

    IEnumerator ShowStartScren()
    {
        instrHandler.ShowExpStart();
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
