using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuPositioning : MonoBehaviour
{
    [SerializeField]
    private Transform leftPalm;

    [SerializeField]
    private float palmActivateThreshold;

    [SerializeField]
    private float palmDeactivateThreshold;

    [SerializeField]
    private GameObject handMenu;

    private bool handActive;

    private void Update()
    {
        handActive = GetIsHandActive();
        handMenu.SetActive(handActive);
        PlaceHandMenu();
    }

    private void PlaceHandMenu()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        //hmm
    }

    private bool GetIsHandActive()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        float palmLook = Vector3.Dot(cameraForward, leftPalm.forward);
        if (handActive)
        {
            return palmLook < palmActivateThreshold;
        }
        else
        {
            return palmLook < palmDeactivateThreshold;
        }
    }
}
