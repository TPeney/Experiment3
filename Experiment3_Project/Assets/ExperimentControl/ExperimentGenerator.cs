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
        Trial newTrial3 = testBlock.CreateTrial();
        Trial newTrial4 = testBlock.CreateTrial();
        Trial newTrial5 = testBlock.CreateTrial();
        Trial newTrial6 = testBlock.CreateTrial();

        SetTrialParameters(newTrial, 1, 3, 3, 1);
        SetTrialParameters(newTrial2, 2, 4, 4, 2);
        SetTrialParameters(newTrial3, 3, 5, 3, 1);
        SetTrialParameters(newTrial4, 4, 0, 4, 2);
        SetTrialParameters(newTrial5, 5, 1, 3, 1);
        SetTrialParameters(newTrial6, 0, 2, 4, 2);
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
