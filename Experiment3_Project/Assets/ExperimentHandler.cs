using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentHandler : MonoBehaviour
{
    TrialRunner trialRunner;

    private void Awake()
    {
        trialRunner = FindObjectOfType<TrialRunner>();    
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Session.instance.hasInitialised && !Session.instance.InTrial)
        {
            trialRunner.RunTrial();
        }
    }
}
