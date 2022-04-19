using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BlurredMatProvider : MonoBehaviour
{
    [SerializeField]
    private Material blurMat;

    private void Start()
    {
		CommandBuffer command = GetCommandBuffer();
		GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterSkybox, command);
	}

    private CommandBuffer GetCommandBuffer()
    {
		CommandBuffer ret = new CommandBuffer();
		ret.name = "Grab screen and blur";

		// copy screen into temporary RT
		int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
		ret.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
		ret.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

		// get two smaller RTs
		int blurredID = Shader.PropertyToID("_Temp1");
		int blurredID2 = Shader.PropertyToID("_Temp2");
		ret.GetTemporaryRT(blurredID, -2, -2, 0, FilterMode.Bilinear);
		ret.GetTemporaryRT(blurredID2, -2, -2, 0, FilterMode.Bilinear);

		// downsample screen copy into smaller RT, release screen RT
		ret.Blit(screenCopyID, blurredID);

		// horizontal blur
		ret.SetGlobalVector("offsets", new Vector4(2.0f / Screen.width, 0, 0, 0));
		ret.Blit(blurredID, blurredID2, blurMat);
		// vertical blur
		ret.SetGlobalVector("offsets", new Vector4(0, 2.0f / Screen.height, 0, 0));
		ret.Blit(blurredID2, blurredID, blurMat);
		// horizontal blur
		ret.SetGlobalVector("offsets", new Vector4(4.0f / Screen.width, 0, 0, 0));
		ret.Blit(blurredID, blurredID2, blurMat);
		// vertical blur
		ret.SetGlobalVector("offsets", new Vector4(0, 4.0f / Screen.height, 0, 0));
		ret.Blit(blurredID2, blurredID, blurMat);

		ret.SetGlobalTexture("_GrabBaseTexture", screenCopyID);
		ret.SetGlobalTexture("_GrabBlurTexture", blurredID);
		return ret;
    }
}
