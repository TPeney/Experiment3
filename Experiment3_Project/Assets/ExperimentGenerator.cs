using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    public void GenerateExperiment(Session session)
    {
        Block testBlock = session.CreateBlock();

        Trial newTrial = testBlock.CreateTrial();
        Trial newTrial2 = testBlock.CreateTrial();

        SetTrialParameters(newTrial, 1, 2, 3, 1);
        SetTrialParameters(newTrial2, 1, 2, 4, 2);
    }

    void SetTrialParameters(Trial trial, int loomingStimIndex, int recedingStimIndex, 
                            int targetLocationIndex, int targetType)
    {
        trial.settings.SetValue("loomingStimIndex", loomingStimIndex);
        trial.settings.SetValue("recedingStimIndex", recedingStimIndex);
        trial.settings.SetValue("targetLocationIndex", targetLocationIndex);
        trial.settings.SetValue("targetType", targetType);
    }
}
