using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlighterScript : MonoBehaviour
{
    public bool ShowHighlight;
    [Range(0,1)]
    public float TargetDrag;

    private Vector3 _target;

    public GameObject HighlighterMesh;
    public Transform MainGrid;

    private bool _newHighlight;

    private void Update()
    {
        if (ShowHighlight)
        {
            HighlighterMesh.SetActive(true);
            Vector3 intersection;
            bool doesIntersect = LinePlaneIntersection(out intersection, 
                transform.position, 
                transform.parent.forward, 
                MainGrid.up, 
                MainGrid.position);
            if(doesIntersect)
            {
                if (_newHighlight)
                {
                    _newHighlight = false;
                    _target = intersection;
                }
                else
                {
                    _target = Vector3.Lerp(_target, intersection, TargetDrag);
                }
            }
            LookAtTarget();
        }
        else
        {
            _newHighlight = true;
            HighlighterMesh.SetActive(false);
        }
    }

    private void LookAtTarget()
    {
        float length = (transform.position - _target).magnitude;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, length / 2);
        transform.LookAt(_target);
    }

    private static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
    {
        float dotNumerator;
        float dotDenominator;
        float length;
        Vector3 vector;
        intersection = Vector3.zero;
        dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
        dotDenominator = Vector3.Dot(lineVec, planeNormal);
        
        if (dotDenominator != 0.0f)
        {
            length = dotNumerator / dotDenominator;
            vector = lineVec.normalized * length;
            intersection = linePoint + vector;
            return true;
        }
        return false;    
    }

}
