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

        StartCoroutine(RunExperiment());
    }

    IEnumerator RunExperiment()
    {
        experimentRunning = true;

        experimentGenerator.GenerateExperiment();

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
            if (!exp.InTrial) { trialRunner.RunTrial(); }

            yield return null;
        }
    }
}
