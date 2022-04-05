using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineVisualScript : MonoBehaviour
{
    public bool ActiveTimeline;
    public Transform Needlehead;

    public MainViewerScript MainScript;

    public GameObject MainTimelineControl;
    public Transform CurrentTimeUi;
    public Transform TimeStartUi;
    public Transform TimeEndUi;
    public Transform TimeContainerUi;
    public Transform TimeConnectorUi;
    
    private float timeUiScale;

    void Start()
    {
        timeUiScale = (TimeStartUi.position - TimeEndUi.position).magnitude;
    }
    private void Update()
    {
        UpdateTimeline();
    }

    private void DisplayTimeUi()
    {
        MainTimelineControl.SetActive(true);
        MainTimelineControl.transform.position = Needlehead.position;
        MainTimelineControl.transform.rotation = Needlehead.rotation;
        CurrentTimeUi.localPosition = Vector3.zero;
        float startOffset = timeUiScale * MainScript.Time;
        float endOffset = timeUiScale * (1 - MainScript.Time);
        TimeStartUi.localPosition = new Vector3(-startOffset, 0, 0);
        TimeEndUi.localPosition = new Vector3(endOffset, 0, 0);
        TimeContainerUi.position = (TimeStartUi.position + TimeEndUi.position) / 2;
    }

    private void UpdateTimeline()
    {
        if (ActiveTimeline && !MainTimelineControl.activeSelf)
        {
            DisplayTimeUi();
        }
        if (!ActiveTimeline && MainTimelineControl.activeSelf)
        {
            MainTimelineControl.SetActive(false);
        }
        if (ActiveTimeline)
        {
            float l2 = (TimeStartUi.position - TimeEndUi.position).sqrMagnitude;
            float theDot = Vector3.Dot(Needlehead.position - TimeStartUi.position, TimeEndUi.position - TimeStartUi.position);
            CurrentTimeUi.position = Vector3.Lerp(TimeStartUi.position, TimeEndUi.position, MainScript.Time);
            MainScript.Time = Mathf.Max(0, Mathf.Min(1, theDot / l2));
            
            float connectorLength = (CurrentTimeUi.position - Needlehead.position).magnitude / 2;
            TimeConnectorUi.localScale = new Vector3(TimeConnectorUi.localScale.x, TimeConnectorUi.localScale.y, connectorLength);
            TimeConnectorUi.position = (CurrentTimeUi.position + Needlehead.position) / 2;
            TimeConnectorUi.LookAt(CurrentTimeUi);
        }
    }
}
