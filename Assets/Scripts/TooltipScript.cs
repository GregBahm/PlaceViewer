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

    private TextMesh _text;
    private Color _litTextColor;
    private Color _unlitTextColor;
    private Color _buttonColor;

	void Start ()
    {
        _text = GetComponent<TextMesh>();
        _litTextColor = _text.color;
        _unlitTextColor = new Color(_litTextColor.r, _litTextColor.g, _litTextColor.b, 0);
	}
	
	void Update ()
    {
        Vector3 toTooltip = Camera.main.transform.position - transform.position;
        float angleToCamera = -Vector3.Dot(toTooltip.normalized, Camera.main.transform.forward);
        bool showTooltip = angleToCamera > ShowTooltipAngle;
        Color textTarget = showTooltip ? _litTextColor : _unlitTextColor;
        Color buttonTarget = showTooltip ? _litTextColor : Color.black;

        _text.color = Color.Lerp(_text.color, textTarget, FadeSpeed);
        _buttonColor = Color.Lerp(_buttonColor, buttonTarget, FadeSpeed);
        ButtonMaterial.SetColor("_EmissionColor", _buttonColor);
	}
}
