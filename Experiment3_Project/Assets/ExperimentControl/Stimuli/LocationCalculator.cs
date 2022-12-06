using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LocationCalculator : MonoBehaviour
{
    [SerializeField] bool showDebug = true;

    [SerializeField] float reactionRadius = 1;
    [SerializeField] float actionToTerminationDistance = 1f;

    // These Vector3's represent the centre of a circle at each depth plane
    [SerializeField] Vector3 actionAreaCentre;
    Vector3 nearPlaneCentre = new();
    Vector3 midPlaneCentre = new();
    Vector3 farPlaneCentre = new();
    Vector3 terminationPointCentre = new();

    public Vector3[] actionAreaPoints = new Vector3[6];
    public Vector3[] nearPlanePoints = new Vector3[6];
    public Vector3[] midPlanePoints = new Vector3[6];
    public Vector3[] farPlanePoints = new Vector3[6];
    public Vector3[] terminationPointPoints = new Vector3[6];

    readonly float nearPlanePercentDistance = 0.25f;
    readonly float midPlanePercentDistance = 0.5f;
    readonly float farPlanePercentDistance = 0.75f;

    void Start()
    {
        CalculatedPlaneCentrePoints();
        CalculatePlanePoints();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            CalculatedPlaneCentrePoints();
            CalculatePlanePoints();
        }
    }

    // Find the centre point of each planes circle based on the starting plane (actionArea) and 
    // desired total length between the action plane and termination point
    private void CalculatedPlaneCentrePoints()
    {
        nearPlaneCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance * nearPlanePercentDistance);
        
        midPlaneCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance * midPlanePercentDistance);
        
        farPlaneCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance * farPlanePercentDistance);
        
        terminationPointCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance);
    }

    private void CalculatePlanePoints()
    {
        CalculatePoints(actionAreaPoints, actionAreaCentre);
        CalculatePoints(nearPlanePoints, nearPlaneCentre);
        CalculatePoints(midPlanePoints, midPlaneCentre);
        CalculatePoints(farPlanePoints, farPlaneCentre);
        CalculatePoints(terminationPointPoints, terminationPointCentre);
    }

    void CalculatePoints(Vector3[] planePoints, Vector3 planeCentre)
    {
        if (planePoints == terminationPointPoints)
        {
            for (int i = 0; i < terminationPointPoints.Length; i++)
            {
                terminationPointPoints[i] = terminationPointCentre;
            }
            return;
        }

        // Radius of the circle of points for each plane = Radius of original circle reduced based on distance as a 
        // percentage from starting plane to end plane
        
        float adjustedR = reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, planeCentre) / actionToTerminationDistance));

        // Far plane now set to mirror near plane - to give angle for looming obj
        if (planeCentre == farPlaneCentre)
        {
            adjustedR = reactionRadius * farPlanePercentDistance;
        }

        // Find each of the 6 stimuli points - 0 is the top of the circle, 3 is the bottom
        planePoints[0] = FindPointOnCircle(planeCentre, adjustedR, 90f);
        planePoints[1] = FindPointOnCircle(planeCentre, adjustedR, 35f);
        planePoints[2] = FindPointOnCircle(planeCentre, adjustedR, 325f);
        planePoints[3] = FindPointOnCircle(planeCentre, adjustedR, 270f);
        planePoints[4] = FindPointOnCircle(planeCentre, adjustedR, 215f);
        planePoints[5] = FindPointOnCircle(planeCentre, adjustedR, 145f);

    }

    // Find the point on a circles circumference given the starting centre co-ords and the angle
    Vector3 FindPointOnCircle(Vector3 centrePoint, float adjustedR, float angle)
    {
        // Angle is given in degrees
        float x = centrePoint.x + adjustedR * Mathf.Cos(angle * (Mathf.PI / 180));
        float y = centrePoint.y + adjustedR * Mathf.Sin(angle * (Mathf.PI / 180));

        return new Vector3(x, y, centrePoint.z);
    }

    // Show Debugging Gizmos
    private void OnDrawGizmos()
    {
        if (!showDebug) { return; }

        UnityEditor.Handles.color = Color.yellow;

        // Action Area
        UnityEditor.Handles.DrawWireDisc(actionAreaCentre, Vector3.forward,
            reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, actionAreaCentre) / actionToTerminationDistance)));

        UnityEditor.Handles.DrawWireDisc(nearPlaneCentre, Vector3.forward,
            reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, nearPlaneCentre) / actionToTerminationDistance)));

        UnityEditor.Handles.DrawWireDisc(midPlaneCentre, Vector3.forward,
            reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, midPlaneCentre) / actionToTerminationDistance)));

        // Far Plane is mirror of near plane (for looming objects)
        UnityEditor.Handles.DrawWireDisc(farPlaneCentre, Vector3.forward,
            reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, nearPlaneCentre) / actionToTerminationDistance)));

        UnityEditor.Handles.DrawWireDisc(terminationPointCentre, Vector3.forward,
            reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, terminationPointCentre) / actionToTerminationDistance)));
    
        foreach (Vector3[] array in new Vector3[][] {nearPlanePoints, midPlanePoints, farPlanePoints})
        {
            foreach (Vector3 point in array)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(point, new Vector3(.05f, .05f, .05f));
            }

        }
    }
}
