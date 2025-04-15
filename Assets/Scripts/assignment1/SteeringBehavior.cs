using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    // you can use this label to show debug information,
    // like the distance to the (next) target
    public TextMeshProUGUI label;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

void Update()
{
    float angleDelta = 0f;  // Declare it here so it's visible throughout the method

    // If there's a path, follow it
    if (path != null && path.Count > 0)
    {
        Vector3 currentWaypoint = path[0];
        target = currentWaypoint;

        Vector3 currentPosition = transform.position;
        Vector3 directionToTarget = target - currentPosition;
        float distanceToTarget = directionToTarget.magnitude;

        // Check if we're close enough to move on to the next waypoint
        float baseArrivalRadius = 1.5f;

        // Estimate next angle change (only if we have a "next" waypoint)
        if (path.Count > 1)
        {
            Vector3 dirToCurrent = (path[0] - currentPosition).normalized;
            Vector3 dirToNext = (path[1] - path[0]).normalized;
            angleDelta = Vector3.Angle(dirToCurrent, dirToNext);
        }

        // Increase the arrival threshold if turning sharply
        float dynamicArrivalRadius = baseArrivalRadius + angleDelta / 30f;

        if (distanceToTarget < dynamicArrivalRadius)
        {
            path.RemoveAt(0);
            if (path.Count == 0)
            {
                kinematic.SetDesiredSpeed(0f);
                kinematic.SetDesiredRotationalVelocity(0f);
                return;
            }

            // Recalculate for the new target
            target = path[0];
            directionToTarget = target - currentPosition;
            distanceToTarget = directionToTarget.magnitude;
        }
    }

    // Default to single target if no path
    Vector3 toTarget = target - transform.position;
    float distance = toTarget.magnitude;

    if (label != null)
    {
        label.text = $"Dist: {distance:F2}";
    }

    if (distance < 1.5f && (path == null || path.Count == 0))
    {
        kinematic.SetDesiredSpeed(0f);
        kinematic.SetDesiredRotationalVelocity(0f);
        return;
    }

    // Rotation handling
    Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
    Vector3 forward = transform.forward;
    float angle = Vector3.SignedAngle(forward, flatDir, Vector3.up);

    float maxRotVel = 90f;
    float rotVel = Mathf.Clamp(angle, -maxRotVel, maxRotVel);
    kinematic.SetDesiredRotationalVelocity(rotVel);

    // Speed handling
    float maxSpeed = 10f;
    float slowdownFactor = 1f;

    // Slow down if turning sharply (based on angle to target)
    float turnSlowdown = Mathf.Clamp01(1f - Mathf.Abs(angle) / 90f);
    slowdownFactor *= turnSlowdown;

    // Slow down further if sharp corner is ahead (based on path angle delta)
    if (angleDelta > 0f)
    {
        float cornerSlowdown = Mathf.Clamp01(1f - angleDelta / 90f);
        slowdownFactor *= cornerSlowdown;
    }

    float speed = Mathf.Clamp(distance, 0f, maxSpeed * slowdownFactor);
    kinematic.SetDesiredSpeed(speed);
}



    public void SetTarget(Vector3 target)
    {
        this.target = target;
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }
}
