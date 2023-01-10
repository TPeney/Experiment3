using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentHandler : MonoBehaviour
{
    Session exp;
    TrialRunner trialRunner;

    ExperimentGenerator experimentGenerator;

    private void Awake()
    {
        trialRunner = FindObjectOfType<TrialRunner>();    
    }

    void Start()
    {
        exp = Session.instance;
        experimentGenerator = GetComponent<ExperimentGenerator>();
    }

    void Update()
    {
        if (!exp.hasInitialised || exp.InTrial) { return; }
        
        trialRunner.RunTrial();

        if (exp.CurrentTrial == exp.CurrentBlock.lastTrial)
        {
            if (exp.settings.GetBool("isDebugging"))
            {
                experimentGenerator.GenerateExperiment();
            }
        }
    }
}
