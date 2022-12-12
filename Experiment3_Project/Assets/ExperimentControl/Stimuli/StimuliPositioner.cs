using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class StimuliPositioner : MonoBehaviour
{
    LocationCalculator locations;
    StimuliController[] stimuliArray;

    void Start()
    {
        locations = GetComponent<LocationCalculator>();
        GameObject arrayHolder = GameObject.FindGameObjectWithTag("Array");
        stimuliArray = arrayHolder.GetComponentsInChildren<StimuliController>();
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            for (int i = 0; i < stimuliArray.Length; i++)
            {
                stimuliArray[i].transform.position = locations.midPlanePoints[i];
            }
        }

    }
}
