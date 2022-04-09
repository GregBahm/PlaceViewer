using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipScript : MonoBehaviour
{
    [Range(0, 1)]
    public float ShowTooltipAngle;
    
    [Range(0, 1)]
    public float FadeSpeed = .01f;

    public Material ButtonMaterial;

    private TextMesh text;
    private Color litTextColor;
    private Color unlitTextColor;
    private Color buttonColor;

	void Start ()
    {
        text = GetComponent<TextMesh>();
        litTextColor = text.color;
        unlitTextColor = new Color(litTextColor.r, litTextColor.g, litTextColor.b, 0);
	}
	
	void Update ()
    {
        Vector3 toTooltip = Camera.main.transform.position - transform.position;
        float angleToCamera = -Vector3.Dot(toTooltip.normalized, Camera.main.transform.forward);
        bool showTooltip = angleToCamera > ShowTooltipAngle;
        Color textTarget = showTooltip ? litTextColor : unlitTextColor;
        Color buttonTarget = showTooltip ? litTextColor : Color.black;

        text.color = Color.Lerp(text.color, textTarget, FadeSpeed);
        buttonColor = Color.Lerp(buttonColor, buttonTarget, FadeSpeed);
        ButtonMaterial.SetColor("_FocusColor", buttonColor);
	}
}
