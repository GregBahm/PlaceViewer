using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HandRefaction : MonoBehaviour
{
    private Camera theCamera;
    private CommandBuffer command;

    [SerializeField]
    private Renderer[] blurryObjects;

    [SerializeField]
    private Material fresnelSourceMat;
    [SerializeField]
    public Material blurMat;

    private void Start()
    {
        theCamera = GetComponent<Camera>();
        command = new CommandBuffer();
        SetUpCommandBuffer();
    }

    private void SetUpCommandBuffer()
    {

        int fresnelSourceId = Shader.PropertyToID("_FresnelSource");

        command.GetTemporaryRT(fresnelSourceId, -1, -1, 0, FilterMode.Bilinear);

        RenderTargetIdentifier identifier = new RenderTargetIdentifier(fresnelSourceId);
        command.SetRenderTarget(identifier);

        command.ClearRenderTarget(false, true, Color.black);
        foreach (Renderer renderer in blurryObjects)
        {
            command.DrawRenderer(renderer, fresnelSourceMat);
        }
        command.ReleaseTemporaryRT(fresnelSourceId);

        theCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, command);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, blurMat);
    }
}
