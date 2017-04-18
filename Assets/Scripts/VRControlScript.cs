using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRControlScript : MonoBehaviour
{   
    private MainViewerScript.DisplayMode[] modes;

    public MainViewerScript.DisplayMode CurrentDisplayMode;
    private int _modeIndex;

    public MainViewerScript MainScript;
    public LegendVisualScript LegendScript;
    public TimelineVisualScript TimelineScript;
    public HighlighterScript HighlighterScript;

    public Animator LeftMovementAnimator;
    public Animator RightMovementAnimator;

    public Transform MainHand;
    public Transform OffHand;
    public Transform Control;
    public Transform HandAverage;
    private MotionDampenerScript _dampener;

    public bool MoveMode;
    public bool ScaleMode;
    
    private bool _scaling;
    private float _initialScale;
    private float _initialHandDistance;

    private bool _lastLeftHand;
    private bool _lastRightHand;

    void Start ()
    {
        _dampener = Control.GetComponent<MotionDampenerScript>();
        modes = new MainViewerScript.DisplayMode[] { MainViewerScript.DisplayMode.ColorAndHeat, MainViewerScript.DisplayMode.FullHeat, MainViewerScript.DisplayMode.FullLongevity, MainViewerScript.DisplayMode.FlatColor };
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

        _dampener.Target = GetDampenerTarget(leftHand, rightHand);

        ScaleMode = leftHand && rightHand;

        UpdateDisplayMode();

        UpdateScaleMode();
        UpdateTimeline();

        HighlighterScript.DoMark = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

    }

    private void SetMovementIndicators(bool leftHand, bool rightHand)
    {
        bool newBothHands = leftHand && rightHand && !(_lastLeftHand && _lastRightHand);
        if (leftHand && !_lastLeftHand || newBothHands)
        {
            LeftMovementAnimator.SetTrigger("Bump");
        }
        if (rightHand && !_lastRightHand || newBothHands)
        {
            RightMovementAnimator.SetTrigger("Bump");
        }
        _lastLeftHand = leftHand;
        _lastRightHand = rightHand;
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
            _modeIndex = (_modeIndex + 1) % modes.Length;
            ApplyDisplayMode();
        }
        if (cycleDown)
        {
            _modeIndex = _modeIndex - 1;
            if (_modeIndex == -1)
            {
                _modeIndex = modes.Length - 1;
            }
            ApplyDisplayMode();
        }
    }

    private void UpdateScaleMode()
    {
        if (ScaleMode)
        {
            float dist = (MainHand.position - OffHand.position).magnitude;
            if (!_scaling)
            {
                _scaling = true;
                _initialScale = Control.transform.localScale.x;
                _initialHandDistance = dist;
            }
            else
            {
                float newScale = _initialScale * (dist / _initialHandDistance);
                Control.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        else
        {
            _scaling = false;
        }
    }

    private void ApplyDisplayMode()
    {
        MainScript.SetDisplayMode(modes[_modeIndex]);
        LegendScript.SetDisplayMode(modes[_modeIndex]);
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
