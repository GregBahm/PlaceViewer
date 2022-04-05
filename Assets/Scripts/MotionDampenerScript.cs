using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionDampenerScript : MonoBehaviour
{
    public Transform Target;
    [Range(0, 1)]
    public float PositionLerp;
    [Range(0, 1)]
    public float RotationLerp;

    private Vector3 lastPositionTarget;
    private Quaternion lastRotationTarget;

    private Transform lastTarget;
    private Transform theGrid;

    void Start()
    {
        theGrid = transform.GetChild(0);
        lastPositionTarget = transform.position;
        lastRotationTarget = transform.rotation;
    }

	void Update ()
    {
        if(Target == null)
        {
            transform.position = Vector3.Lerp(transform.position, lastPositionTarget, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, lastRotationTarget, RotationLerp);
        }
        if (Target != null)
        {
            if (Target != lastTarget)
            {
                theGrid.parent = null;
                transform.position = Target.position;
                transform.rotation = Target.rotation;
                theGrid.parent = transform;
            }
            transform.position = Vector3.Lerp(transform.position, Target.position, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, Target.rotation, RotationLerp);
            lastPositionTarget = Target.position;
            lastRotationTarget = Target.rotation;
        }
        lastTarget = Target;
	}
}
