using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlighterController : MonoBehaviour
{
    public bool DoMark;

    public Transform BeamTip;
    public Transform BeamBody;
    public Transform BeamBase;

    private const int MainPlaneLayer = 8;

    private void Update()
    {
        Debug.DrawLine(BeamBase.position, BeamBase.position + BeamBase.forward * 1000000, Color.red);
        if (DoMark)
        {
            RaycastHit raycast;
            Ray ray = new Ray(BeamBase.position, BeamBase.forward);
            bool markOnBoard = Physics.Raycast(ray, out raycast, 1000000);
            BeamBody.gameObject.SetActive(markOnBoard);
            BeamTip.gameObject.SetActive(markOnBoard);
            if (markOnBoard)
            {
                UpdateBeam(raycast, markOnBoard);
            }
        }
        else
        {
            BeamBody.gameObject.SetActive(false);
            BeamTip.gameObject.SetActive(false);
        }
    }

    private void UpdateBeam(RaycastHit raycast, bool markOnBoard)
    {
        BeamTip.gameObject.SetActive(markOnBoard);
        BeamBody.gameObject.SetActive(markOnBoard);
        BeamTip.position = raycast.point;
        BeamBody.position = (BeamBase.position + BeamTip.position) / 2;
        BeamBody.localScale = new Vector3(0, 0, raycast.distance);
        BeamBody.LookAt(BeamTip);
    }
}
