using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessingCanvasController : MonoBehaviour
{
    public Transform Canvas;

    public float HeadingRotateSensitivity = 1;
    public float PitchRotateSensitivity = 1;

    private bool startDrag;
    private Vector3 startDragPosition;
    private float startingPitch;
    private float startingHeading;

    void Update ()
    {
		if(Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;

            if(!startDrag)
            {
                startDragPosition = mousePos;
                startingPitch = Canvas.rotation.eulerAngles.x - 360;
                startingHeading = Canvas.rotation.eulerAngles.y;
                startDrag = true;
                return;
            }

            Vector3 diff = mousePos - startDragPosition;
            float newHeading = -diff.x * HeadingRotateSensitivity + startingHeading;
            newHeading = Mathf.Clamp(newHeading, -50, 50);
            float newPitch = diff.y * PitchRotateSensitivity + startingPitch;
            newPitch = Mathf.Clamp(newPitch, - 170, -40);
            Canvas.rotation = Quaternion.Euler(newPitch, newHeading, 0);
        }
        else
        {
            startDrag = false;
        }
	}
}
