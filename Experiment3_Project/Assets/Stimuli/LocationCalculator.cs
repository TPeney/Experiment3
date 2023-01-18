using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculates stimuli positions dynmaically and update in editor.
/// Positions are calculated at three depth planes along a start/end point and
/// the radius of each stimuli circle is based on a reaction radius around the participant. 
/// </summary>
[ExecuteAlways]
public class LocationCalculator : MonoBehaviour
{
    [Header("Stimuli Action Space Settings")]
    [Tooltip("This should be aligned to the Camera's centre.")]
    [SerializeField] Vector3 actionAreaCentre;
    [Tooltip("The bounds of stimuli movement. Controls overall spread of Stimuli in each plane.")]
    [SerializeField] float reactionRadius = 1;
    [Tooltip("The distance between the participant and the plane for looming stimuli, in Unity Units.")]
    [SerializeField] float actionToTerminationDistance = 1f;

    // These Vector3's represent the centre of each depth plane
    Vector3 nearPlaneCentre = new(); // Plane where receding motion starts
    Vector3 midPlaneCentre = new(); // Plane where motion ends / static stimuli
    Vector3 farPlaneCentre = new(); // Plane where looming motion starts

    // The radius of the stimuli circle within each depth plane
    readonly float nearPlaneRadius = 0.3f;
    readonly float midPlaneRadius = 0.5f;
    readonly float farPlaneRadius = 0.9f;

    // How far along the line each depth plane should be
    readonly float nearPlanePercentDistance = 0.25f; // Between ppt and centre
    readonly float midPlanePercentDistance = 0.5f; // Centre point (between start and end)
    readonly float farPlanePercentDistance = 1f; // Twice the distance of near plane, from mid

    // List of vectors to hold the calculated points at each depth plane
    public Vector3[] NearPlanePoints { get; private set; } = new Vector3[6];
    public Vector3[] MidPlanePoints { get; private set; } = new Vector3[6];
    public Vector3[] FarPlanePoints { get; private set; } = new Vector3[6];
    
    StimuliController[] stimuliArray;

    [SerializeField] bool showDebug = true;

    void Start()
    {
        GameObject arrayHolder = GameObject.FindGameObjectWithTag("Array");
        stimuliArray = arrayHolder.GetComponentsInChildren<StimuliController>();
        CalculateCentrePoints();
        CalculateDepthPlanePositions();
    }

    private void Update()
    {
        // Calculate and update positions in editor 
        if (Application.isPlaying) return; 
        CalculateCentrePoints();
        CalculateDepthPlanePositions();
        UpdateStimuliPositions();
    }

    /// <summary>
    /// Finds the centre point of each depth plane along the action to termination point line.
    /// </summary>
    private void CalculateCentrePoints()
    {
        nearPlaneCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance * nearPlanePercentDistance);
        
        midPlaneCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance * midPlanePercentDistance);
        
        farPlaneCentre = new Vector3(actionAreaCentre.x, actionAreaCentre.y, 
            actionAreaCentre.z + actionToTerminationDistance * farPlanePercentDistance);
    }

    private void CalculateDepthPlanePositions()
    {
        CalculatePoints(NearPlanePoints, nearPlaneCentre, nearPlaneRadius);
        CalculatePoints(MidPlanePoints, midPlaneCentre, midPlaneRadius);
        CalculatePoints(FarPlanePoints, farPlaneCentre, farPlaneRadius);
    }

    /// <summary>
    /// Calculates and assigns the location of each 6 stimuli positions in the three depth planes.
    /// </summary>
    /// <param name="planePoints">The list to assign the points to.</param>
    /// <param name="planeCentre">The centre point of the depth plane.</param>
    /// <param name="planeRadius">The radius of the stimuli circle.</param>
    void CalculatePoints(Vector3[] planePoints, Vector3 planeCentre, float planeRadius)
    {
        // Now using a cylinder rather than a cone shape - all planes share radius of middle
        //float adjustedR = reactionRadius * (1 - (Vector3.Distance(actionAreaCentre, planeCentre) / actionToTerminationDistance));
        
        float adjustedR = reactionRadius * planeRadius;

        // Find each of the 6 stimuli points - 0 is the top of the circle, 3 is the bottom
        // 0degrees is 9 on a clock, points go clockwise around the circle
        planePoints[0] = FindPointOnCircle(planeCentre, adjustedR, 90f);
        planePoints[1] = FindPointOnCircle(planeCentre, adjustedR, 35f);
        planePoints[2] = FindPointOnCircle(planeCentre, adjustedR, 325f);
        planePoints[3] = FindPointOnCircle(planeCentre, adjustedR, 270f);
        planePoints[4] = FindPointOnCircle(planeCentre, adjustedR, 215f);
        planePoints[5] = FindPointOnCircle(planeCentre, adjustedR, 145f);
    }

    /// <summary>
    /// Finds a point on the circumference of a circle.
    /// </summary>
    /// <param name="centrePoint">Centre of the circle.</param>
    /// <param name="adjustedR">Radius of the circle.</param>
    /// <param name="angle">Angle along circumference.</param>
    /// <returns>A vector3 of the point on the circle's circumference.</returns>
    Vector3 FindPointOnCircle(Vector3 centrePoint, float adjustedR, float angle)
    {
        // Angle is given in degrees
        float x = centrePoint.x + adjustedR * Mathf.Cos(angle * (Mathf.PI / 180));
        float y = centrePoint.y + adjustedR * Mathf.Sin(angle * (Mathf.PI / 180));

        return new Vector3(x, y, centrePoint.z);
    }

    // Dynamically update the positions of the stimuli so they are correct in the editor
    private void UpdateStimuliPositions()
    {
        for (int i = 0; i < stimuliArray.Length; i++)
        {
            stimuliArray[i].transform.position = MidPlanePoints[i];
        }
    }

    // Visual debugging - shows the depth planes as gizmos
    private void OnDrawGizmos()
    {
        if (!showDebug) { return; }

        UnityEditor.Handles.color = Color.yellow;

        // Action Area
        UnityEditor.Handles.DrawWireDisc(actionAreaCentre, Vector3.forward,
            reactionRadius);

        // Near Plane Radius
        UnityEditor.Handles.DrawWireDisc(nearPlaneCentre, Vector3.forward,
            reactionRadius * nearPlaneRadius);

        // Mid Plane Radius
        UnityEditor.Handles.DrawWireDisc(midPlaneCentre, Vector3.forward,
            reactionRadius * midPlaneRadius);

        // Far Plane Radius
        UnityEditor.Handles.DrawWireDisc(farPlaneCentre, Vector3.forward,
            reactionRadius * farPlaneRadius);
        
        // Draw cubes for each point in the depth planes
        foreach (Vector3[] array in new Vector3[][] {NearPlanePoints, MidPlanePoints, FarPlanePoints})
        {
            foreach (Vector3 point in array)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(point, new Vector3(.05f, .05f, .05f));
            }
        }

        // Action and Termination Centre Points
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(actionAreaCentre, 0.03f);
        Vector3 terminationPoint = actionAreaCentre;
        terminationPoint.z = actionAreaCentre.z + actionToTerminationDistance;
        Gizmos.DrawSphere(terminationPoint, 0.03f);

    }
}
