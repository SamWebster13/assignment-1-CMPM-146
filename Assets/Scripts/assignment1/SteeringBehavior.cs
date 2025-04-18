using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    public TextMeshProUGUI label;


    public float cruiseSpeed = 15f;        // Max speed while cruising
    public float accelerationTime = 3f;    // Time (in seconds) to reach cruiseSpeed
    private float trueAcceleration;        // Calculated acceleration value


    public float maxSpeed = 30f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float turnSpeed = 180f; // degrees per second
    public float arrivalThreshold = 1f;
    public float minSpeedToTurn = 10f;

    private float currentSpeed = 0f;

    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;

        trueAcceleration = cruiseSpeed / accelerationTime;
    }

    void Update()
    {
        if (path != null && path.Count > 0)
        {
            FollowPath();
        }
        else
        {
            MoveToTarget(target, true);
        }
    }

    void FollowPath()
    {
        if (path.Count == 0)
            return;

        Vector3 currentTarget = path[0];

        // If close to current waypoint, move to next
        if (Vector3.Distance(transform.position, currentTarget) < arrivalThreshold)
        {
            if (path.Count > 1)
            {
                path.RemoveAt(0); // Smooth transition
                currentTarget = path[0];
            }
            else
            {
                MoveToTarget(currentTarget, true); // Last point 
                return;
            }
        }

        MoveToTarget(currentTarget, false); // keep going
    }

    void MoveToTarget(Vector3 targetPos, bool finalTarget)
{
    Vector3 dir = targetPos - transform.position;
    Vector3 flatDir = new Vector3(dir.x, 0, dir.z);
    float distance = flatDir.magnitude;

    if (distance < arrivalThreshold && finalTarget)
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        if (label) label.text = "Arrived!";
        return;
    }

    flatDir.Normalize();
    float angleToTarget = Vector3.SignedAngle(transform.forward, flatDir, Vector3.up);
    float alignment = Mathf.Cos(Mathf.Deg2Rad * angleToTarget);
	// Scale turning with speed 
	float speedFactor = Mathf.Clamp01(currentSpeed / cruiseSpeed); 
	float effectiveTurnSpeed = Mathf.Lerp(turnSpeed * 1.5f, turnSpeed * 1f, speedFactor); // slower turn at high speed

	float steeringStrength = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
	float turnAmount = steeringStrength * effectiveTurnSpeed * Time.deltaTime;

    // Apply rotation only if moving
    if (currentSpeed > 0.1f)
    {
        float rotationFactor = Mathf.InverseLerp(0.5f, 5f, currentSpeed);
        transform.Rotate(0, turnAmount * rotationFactor, 0);
    }

    // Set speed cap based on whether we're approaching the final target
    float speedCap = finalTarget && distance < 5f ? cruiseSpeed * (distance / 5f) : cruiseSpeed;
    if (currentSpeed < speedCap)
    {
        currentSpeed += trueAcceleration * Time.deltaTime;
    }
    else if (currentSpeed > speedCap)
    {
        currentSpeed -= deceleration * Time.deltaTime;
    }

    currentSpeed = Mathf.Clamp(currentSpeed, 0f, cruiseSpeed);
    // Move forward in the direction the car is currently facing
    transform.position += transform.forward * currentSpeed * Time.deltaTime;

    // Debug info
    // if (label)
    // {
        // label.text = $"Dist: {distance:F2}\nTurn: {angleToTarget:F1}°\nSpeed: {currentSpeed:F1}";
    // }
}

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        this.path = null;
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        if (path == null || path.Count == 0) return;
        this.path = new List<Vector3>(path);
        this.target = path[path.Count - 1];
        EventBus.ShowTarget(target);
    }
    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }
    void OnDestroy()
    {
        EventBus.OnSetMap -= SetMap;
    }
}
