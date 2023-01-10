using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    readonly List<int> array3Up = new() { 0, 2, 4 };
    readonly List<int> array3Down = new() { 1, 3, 5 };
    readonly List<int> array6 = new() { 0, 1, 2, 3, 4, 5 };

    bool isDebugging;

    private void Start()
    {
        isDebugging = Session.instance.settings.GetBool("isDebugging");
    }

    public void GenerateExperiment(Session session)
    {
        Block mainBlock = Session.instance.CreateBlock();
        GenerateTrials(mainBlock);

        if (isDebugging)
        {
            mainBlock.trials = mainBlock.trials.GetRange(0, 10);
        }
    }

    void GenerateTrials(Block block)
    {
        List<int>[] arrayConfigs = { array3Up, array3Down, array6 };

        foreach (List<int> arrayConfig in arrayConfigs)
        {
            foreach (int stimuliIndex in arrayConfig)
            {
                int loomIndex = stimuliIndex;

                int receedeIndex = CalculateReceedeIndex(stimuliIndex, clockwise: true);
                foreach (int targIndex in arrayConfig) // Adds copy of each current trial config for each possible target location
                {
                    CreateTrial(block, arrayConfig, loomIndex, receedeIndex, targIndex);
                }

                receedeIndex = CalculateReceedeIndex(stimuliIndex, clockwise: false);
                foreach (int targIndex in arrayConfig) // Adds copy of each current trial config for each possible target location
                {
                    CreateTrial(block, arrayConfig, loomIndex, receedeIndex, targIndex);
                }
            }
        }
        
        AssignRandomTargetType(block);
        block.trials.Shuffle();
    }


    void CreateTrial(Block block, List<int> arrayConfig, int loomIndex, int recedeIndex, int targIndex)
    {
        Trial trial = block.CreateTrial();

        trial.settings.SetValue("arrayConfiguration", arrayConfig);
        trial.settings.SetValue("loomingStimIndex", loomIndex);
        trial.settings.SetValue("recedingStimIndex", recedeIndex);
        trial.settings.SetValue("targetLocationIndex", targIndex);
    }

    private int CalculateReceedeIndex(int stimuliIndex, bool clockwise)
    {
        int receedeIndex = clockwise ? (stimuliIndex + 2) % 6 : (stimuliIndex - 2) % 6;
        return receedeIndex;
    }

    private static void AssignRandomTargetType(Block block)
    {
        block.trials.Shuffle();

        for (int i = 0; i < block.trials.Count; i++)
        {
            Trial trial = block.trials[i];

            int targType = i < block.trials.Count / 2 ? 1 : 2;
            trial.settings.SetValue("targetType", targType);
        }
    }

    //void GenerateDebugExperiment(Session session)
    //{
    //    if (testBlock == null) {  testBlock = session.CreateBlock(); }

    //    for (int i = 0; i < 6; i++)
    //    {
    //        Trial trial = testBlock.CreateTrial();

    //        switch (i)
    //        {
    //            case 0:
    //                CreateTrial(trial, 0, 2, 4, 1, array3Up);
    //                break;
    //            case 1:
    //                CreateTrial(trial, 2, 4, 4, 2, array3Up);
    //                break;
    //            case 2:
    //                CreateTrial(trial, 3, 5, 3, 1, array3Down);
    //                break;
    //            case 3:
    //                CreateTrial(trial, 5, 3, 1, 2, array3Down);
    //                break;
    //            case 4:
    //                CreateTrial(trial, 5, 1, 3, 1, array6);
    //                break;
    //            case 5:
    //                CreateTrial(trial, 0, 2, 4, 2, array6);
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //}
}
