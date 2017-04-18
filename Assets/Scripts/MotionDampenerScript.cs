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

    private Vector3 _lastPositionTarget;
    private Quaternion _lastRotationTarget;

    private Transform _lastTarget;
    private Transform _theGrid;

    void Start()
    {
        _theGrid = transform.GetChild(0);
        _lastPositionTarget = transform.position;
        _lastRotationTarget = transform.rotation;
    }

	void Update ()
    {
        if(Target == null)
        {
            transform.position = Vector3.Lerp(transform.position, _lastPositionTarget, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, _lastRotationTarget, RotationLerp);
        }
        if (Target != null)
        {
            if (Target != _lastTarget)
            {
                _theGrid.parent = null;
                transform.position = Target.position;
                transform.rotation = Target.rotation;
                _theGrid.parent = transform;
            }
            transform.position = Vector3.Lerp(transform.position, Target.position, PositionLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, Target.rotation, RotationLerp);
            _lastPositionTarget = Target.position;
            _lastRotationTarget = Target.rotation;
        }
        _lastTarget = Target;
	}
}
