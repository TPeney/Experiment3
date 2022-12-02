using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    bool isDebugging;

    List<int> array3Up = new List<int>() { 0, 2, 4 };
    List<int> array3Down = new List<int>() { 1, 3, 5 };
    List<int> array6 = new List<int>() { 0, 1, 2, 3, 4, 5 };

    public void GenerateExperiment(Session session)
    {
        isDebugging = Session.instance.settings.GetBool("isDebugging");

        if (isDebugging)
        {
            GenerateDebugExperiment(session);
            return;
        }


    }

    void SetTrialParameters(Trial trial, int loomingStimIndex, int recedingStimIndex, 
                            int targetLocationIndex, int targetType, List<int> arrayConfig)
    {
        trial.settings.SetValue("loomingStimIndex", loomingStimIndex);
        trial.settings.SetValue("recedingStimIndex", recedingStimIndex);
        trial.settings.SetValue("targetLocationIndex", targetLocationIndex);
        trial.settings.SetValue("targetType", targetType);
        trial.settings.SetValue("arrayConfiguration", arrayConfig);
    }

    void GenerateTrials()
    {

    }

    void GenerateDebugExperiment(Session session)
    {
        Block testBlock = session.CreateBlock();

        for (int i = 0; i < 6; i++)
        {
            testBlock.CreateTrial();
        }

        SetTrialParameters(testBlock.trials[0], 0, 2, 4, 1, array3Up);
        SetTrialParameters(testBlock.trials[1], 2, 4, 4, 2, array3Up);
        SetTrialParameters(testBlock.trials[2], 3, 5, 3, 1, array3Down);
        SetTrialParameters(testBlock.trials[3], 4, 2, 4, 2, array3Down);
        SetTrialParameters(testBlock.trials[4], 5, 1, 3, 1, array6);
        SetTrialParameters(testBlock.trials[5], 0, 2, 4, 2, array6);
    }
}
