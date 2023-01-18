using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ExperimentGenerator : MonoBehaviour
{
    // Definitions of the Array Size parameter
    // ints are indices of the stimuli in the 'stimuliArray' GameObject
    readonly List<int> array3Up = new() { 0, 2, 4 }; // array size 3 (forming a triangle)
    readonly List<int> array3Down = new() { 1, 3, 5 }; // array size 3 (forming a inverted triangle)
    readonly List<int> array6 = new() { 0, 1, 2, 3, 4, 5 }; // array size 6 - all stimuli in a hexagon

    /// <summary>
    /// Called by the ExperimentHandler to begin trial generation
    /// </summary>
    /// <returns>A block of trials</returns>
    public Block GenerateTrialBlock(string blockName, int? trialN = null)
    {
        Block block = Session.instance.CreateBlock(); // Creating a new Block to store the trials
        block.settings.SetValue("blockName", blockName);
        
        GenerateTrials(block); // Generate trials and assigns the trial parameters 
        block.trials.Shuffle();

        // If only a subset of trials is desired, return that range - otherwise set all trials
        if (trialN != null) { block.trials = block.trials.GetRange(0, (int)trialN); }
        
        AssignRandomTargetType(block);

        return block;
    }
     
    /// <summary>
    /// Run trial generation for each combination of parameters within this experiment
    /// </summary>
    /// <param name="block">The Block to add trials to</param>
    void GenerateTrials(Block block)
    {
        List<int>[] arrayConfigs = { array3Up, array3Down, array6 };

        foreach (List<int> arrayConfig in arrayConfigs) // Trial generation process below is repeated for each array size
        {
            foreach (int stimuliIndex in arrayConfig) 
            {
                // Create a trial for each loom/recede stimulus pairing
                // When one element is looming, two configurations possible for receding location - clockwise or anticlockwise
                int loomIndex = stimuliIndex;
                
                // Generate trials for clockwise orientation
                int recedeIndex = CalculateRecedeIndex(stimuliIndex, clockwise: true);
                foreach (int targIndex in arrayConfig) // Adds copy of each current trial config for each possible target location
                {
                    int reps = arrayConfig == array6 ? 1 : 2; // Add two repeats for the 3-set size arrays
                    for (int i = 0; i < reps; i++)
                    {
                        CreateTrial(block, arrayConfig, loomIndex, recedeIndex, targIndex);
                    }

                }

                // Generate trials for anti-clockwise orientation
                recedeIndex = CalculateRecedeIndex(stimuliIndex, clockwise: false);
                foreach (int targIndex in arrayConfig) // Adds copy of each current trial config for each possible target location
                {
                    int reps = arrayConfig == array6 ? 1 : 2; // Add two repeats for the 3-set size arrays
                    for (int i = 0; i < reps; i++)
                    {
                        CreateTrial(block, arrayConfig, loomIndex, recedeIndex, targIndex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Helper method to quickly generate a trial with given parameters.
    /// </summary>
    /// <param name="block">The Block the trial should be added to.</param>
    /// <param name="arrayConfig">The list of indices representing the array size and orientation</param>
    /// <param name="loomIndex">Which array item will perform a looming motion</param>
    /// <param name="recedeIndex">Which array item will perform a receding motion</param>
    /// <param name="targIndex">Which array item will display the target</param>
    void CreateTrial(Block block, List<int> arrayConfig, int loomIndex, int recedeIndex, int targIndex)
    {
        Trial trial = block.CreateTrial();

        trial.settings.SetValue("arrayConfiguration", arrayConfig);
        trial.settings.SetValue("loomingStimIndex", loomIndex);
        trial.settings.SetValue("recedingStimIndex", recedeIndex);
        trial.settings.SetValue("targetLocationIndex", targIndex);
    }

    /// <summary>
    /// Calculates the (anti-)/clockwise index of the receding array item in relation to the looming one
    /// </summary>
    /// <param name="loomingIndex">The current index of the looming array item</param>
    /// <param name="clockwise">true: Calculate position clockwise of the looming item</param>
    /// <returns></returns>
    private int CalculateRecedeIndex(int loomingIndex, bool clockwise)
    {
        int recedeIndex;
        if (clockwise) { recedeIndex = (loomingIndex + 2) % 6; } // Index + 2 clamped between 0 and 5
        else
        {
            recedeIndex = (loomingIndex - 2) % 6; // Index - 2 clamped between 0 and 5
            // Above can result in a negative, so conditional to clamp back to desired range
            if (recedeIndex < 0) { recedeIndex = 6 + recedeIndex;  } 
        }
        return recedeIndex;
    }

    /// <summary>
    /// Randomly, but evenly, assigns either target option to a trial
    /// </summary>
    /// <param name="block">The Block to assign target info to</param>
    private static void AssignRandomTargetType(Block block)
    {
        // Trials shuffled before/after to avoid repeats each run
        block.trials.Shuffle();

        for (int i = 0; i < block.trials.Count; i++)
        {
            Trial trial = block.trials[i];

            // First half will show target option 1, second half will receive target option 2
            int targType = i < block.trials.Count / 2 ? 1 : 2; 
            trial.settings.SetValue("targetID", targType);
        }
        
        block.trials.Shuffle();
    }
}
