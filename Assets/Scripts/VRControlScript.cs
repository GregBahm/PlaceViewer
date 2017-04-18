using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRControlScript : MonoBehaviour
{   
    private ViewerScript.DisplayMode[] modes;

    public ViewerScript.DisplayMode CurrentDisplayMode;
    private int modeIndex;

    public ViewerScript MainScript;
    public LegendVisualController LegendScript;
    public TimelineVisualController TimelineScript;
    public HighlighterController HighlighterScript;

    public Animator LeftMovementAnimator;
    public Animator RightMovementAnimator;

    public Transform MainHand;
    public Transform OffHand;
    public Transform Control;
    public Transform HandAverage;
    private Dampen dampener;

    public bool MoveMode;
    public bool ScaleMode;
    
    private bool scaling;
    private float initialScale;
    private float initialHandDistance;

    private bool lastLeftHand;
    private bool lastRightHand;

    void Start ()
    {
        dampener = Control.GetComponent<Dampen>();
        modes = new ViewerScript.DisplayMode[] { ViewerScript.DisplayMode.ColorAndHeat, ViewerScript.DisplayMode.FullHeat, ViewerScript.DisplayMode.FullLongevity, ViewerScript.DisplayMode.FlatColor };
        ApplyDisplayMode();
    }

    void Update ()
    {
        //HandAverage is the average point between both hands that controls the board when both grip buttons are pressed
        HandAverage.position = Vector3.Lerp(MainHand.position, OffHand.position, .5f);
        HandAverage.rotation = Quaternion.Lerp(MainHand.rotation, OffHand.rotation, .5f);
        HandAverage.LookAt(MainHand.position, HandAverage.up);

        bool leftHand = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        bool rightHand = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

        SetMovementIndicators(leftHand, rightHand);

        dampener.Target = GetDampenerTarget(leftHand, rightHand);

        ScaleMode = leftHand && rightHand;

        UpdateDisplayMode();

        UpdateScaleMode();
        UpdateTimeline();

        HighlighterScript.DoMark = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

    }

    private void SetMovementIndicators(bool leftHand, bool rightHand)
    {
        bool newBothHands = leftHand && rightHand && !(lastLeftHand && lastRightHand);
        if (leftHand && !lastLeftHand || newBothHands)
        {
            LeftMovementAnimator.SetTrigger("Bump");
        }
        if (rightHand && !lastRightHand || newBothHands)
        {
            RightMovementAnimator.SetTrigger("Bump");
        }
        lastLeftHand = leftHand;
        lastRightHand = rightHand;
    }

    private Transform GetDampenerTarget(bool leftHand, bool rightHand)
    {
        if(leftHand && rightHand)
        {
            return HandAverage;
        }
        if(leftHand)
        {
            return MainHand;
        }
        if(rightHand)
        {
            return OffHand;
        }
        return null;
    }

    private void UpdateDisplayMode()
    {
        bool cycleUp = OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three);
        bool cycleDown = OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.Four);
        if (cycleUp)
        {
            modeIndex = (modeIndex + 1) % modes.Length;
            ApplyDisplayMode();
        }
        if (cycleDown)
        {
            modeIndex = modeIndex - 1;
            if (modeIndex == -1)
            {
                modeIndex = modes.Length - 1;
            }
            ApplyDisplayMode();
        }
    }

    private void UpdateScaleMode()
    {
        if (ScaleMode)
        {
            float dist = (MainHand.position - OffHand.position).magnitude;
            if (!scaling)
            {
                scaling = true;
                initialScale = Control.transform.localScale.x;
                initialHandDistance = dist;
            }
            else
            {
                float newScale = initialScale * (dist / initialHandDistance);
                Control.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        else
        {
            scaling = false;
        }
    }

    private void ApplyDisplayMode()
    {
        MainScript.SetDisplayMode(modes[modeIndex]);
        LegendScript.SetDisplayMode(modes[modeIndex]);
    }

    private void UpdateTimeline()
    {
        bool timelineTrigger = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        TimelineScript.ActiveTimeline = timelineTrigger;

        float leftThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x / 1000;
        float rightThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x / 1000;
        MainScript.Time = Mathf.Clamp01(leftThumbstick + rightThumbstick + MainScript.Time);
    }
}
