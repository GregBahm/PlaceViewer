using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class DepthOfField : PostEffectsBase
    {
         
        public bool visualizeFocus = false;
        public float focalLength = 10.0f;
        public float focalSize = 0.05f;
        public float aperture = 0.5f;
        public Transform focalTransform = null;
        public float maxBlurSize = 2.0f;
        public bool highResolution = false;

        public enum BlurType
        {
            DiscBlur = 0,
            DX11 = 1,
        }

        public enum BlurSampleCount
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        public BlurType blurType = BlurType.DiscBlur;
        public BlurSampleCount blurSampleCount = BlurSampleCount.High;

        public bool nearBlur = false;
        public float foregroundOverlap = 1.0f;

        public Shader dofHdrShader;
        private Material dofHdrMaterial = null;

        private float focalDistance01 = 10.0f;
        private ComputeBuffer cbDrawArgs;
        private ComputeBuffer cbPoints;
        private float internalBlurWidth = 1.0f;

        private Camera cachedCamera;

        public override bool CheckResources()
        {
            CheckSupport(true); // only requires depth, not HDR

            dofHdrMaterial = CheckShaderAndCreateMaterial(dofHdrShader, dofHdrMaterial);

            if (!isSupported)
                ReportAutoDisable();

            return isSupported;
        }

        void OnEnable()
        {
            cachedCamera = GetComponent<Camera>();
            cachedCamera.depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnDisable()
        {
            ReleaseComputeResources();

            if (dofHdrMaterial) DestroyImmediate(dofHdrMaterial);
            dofHdrMaterial = null;
        }

        void ReleaseComputeResources()
        {
            if (cbDrawArgs != null) cbDrawArgs.Release();
            cbDrawArgs = null;
            if (cbPoints != null) cbPoints.Release();
            cbPoints = null;
        }

        void CreateComputeResources()
        {
            if (cbDrawArgs == null)
            {
                cbDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
                var args = new int[4];
                args[0] = 0; args[1] = 1; args[2] = 0; args[3] = 0;
                cbDrawArgs.SetData(args);
            }
            if (cbPoints == null)
            {
                cbPoints = new ComputeBuffer(90000, 12 + 16, ComputeBufferType.Append);
            }
        }

        float FocalDistance01(float worldDist)
        {
            return cachedCamera.WorldToViewportPoint((worldDist - cachedCamera.nearClipPlane) * cachedCamera.transform.forward + cachedCamera.transform.position).z / (cachedCamera.farClipPlane - cachedCamera.nearClipPlane);
        }

        private void WriteCoc(RenderTexture fromTo, bool fgDilate)
        {
            dofHdrMaterial.SetTexture("_FgOverlap", null);

            if (nearBlur && fgDilate)
            {

                int rtW = fromTo.width / 2;
                int rtH = fromTo.height / 2;

                // capture fg coc
                RenderTexture temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(fromTo, temp2, dofHdrMaterial, 4);

                // special blur
                float fgAdjustment = internalBlurWidth * foregroundOverlap;

                dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, fgAdjustment, 0.0f, fgAdjustment));
                RenderTexture temp1 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(temp2, temp1, dofHdrMaterial, 2);
                RenderTexture.ReleaseTemporary(temp2);

                dofHdrMaterial.SetVector("_Offsets", new Vector4(fgAdjustment, 0.0f, 0.0f, fgAdjustment));
                temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(temp1, temp2, dofHdrMaterial, 2);
                RenderTexture.ReleaseTemporary(temp1);

                // "merge up" with background COC
                dofHdrMaterial.SetTexture("_FgOverlap", temp2);
                fromTo.MarkRestoreExpected(); // only touching alpha channel, RT restore expected
                Graphics.Blit(fromTo, fromTo, dofHdrMaterial, 13);
                RenderTexture.ReleaseTemporary(temp2);
            }
            else
            {
                // capture full coc in alpha channel (fromTo is not read, but bound to detect screen flip)
                fromTo.MarkRestoreExpected(); // only touching alpha channel, RT restore expected
                Graphics.Blit(fromTo, fromTo, dofHdrMaterial, 0);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!CheckResources())
            {
                Graphics.Blit(source, destination);
                return;
            }

            // clamp & prepare values so they make sense

            if (aperture < 0.0f) aperture = 0.0f;
            if (maxBlurSize < 0.1f) maxBlurSize = 0.1f;
            focalSize = Mathf.Clamp(focalSize, 0.0f, 2.0f);
            internalBlurWidth = Mathf.Max(maxBlurSize, 0.0f);

            // focal & coc calculations

            focalDistance01 = (focalTransform) ? (cachedCamera.WorldToViewportPoint(focalTransform.position)).z / (cachedCamera.farClipPlane) : FocalDistance01(focalLength);
            dofHdrMaterial.SetVector("_CurveParams", new Vector4(1.0f, focalSize, (1.0f / (1.0f - aperture) - 1.0f), focalDistance01));

            // possible render texture helpers

            RenderTexture rtLow = null;
            RenderTexture rtLow2 = null;


            source.filterMode = FilterMode.Bilinear;

            if (highResolution) internalBlurWidth *= 2.0f;

            WriteCoc(source, true);

            rtLow = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
            rtLow2 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);

            int blurPass = (blurSampleCount == BlurSampleCount.High || blurSampleCount == BlurSampleCount.Medium) ? 17 : 11;

            if (highResolution)
            {
                dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, internalBlurWidth, 0.025f, internalBlurWidth));
                Graphics.Blit(source, destination, dofHdrMaterial, blurPass);
            }
            else
            {
                dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, internalBlurWidth, 0.1f, internalBlurWidth));

                // blur
                Graphics.Blit(source, rtLow, dofHdrMaterial, 6);
                Graphics.Blit(rtLow, rtLow2, dofHdrMaterial, blurPass);

                // cheaper blur in high resolution, upsample and combine
                dofHdrMaterial.SetTexture("_LowRez", rtLow2);
                dofHdrMaterial.SetTexture("_FgOverlap", null);
                dofHdrMaterial.SetVector("_Offsets", Vector4.one * ((1.0f * source.width) / (1.0f * rtLow2.width)) * internalBlurWidth);
                Graphics.Blit(source, destination, dofHdrMaterial, blurSampleCount == BlurSampleCount.High ? 18 : 12);
            }
            if (rtLow) RenderTexture.ReleaseTemporary(rtLow);
            if (rtLow2) RenderTexture.ReleaseTemporary(rtLow2);
        }
    }
}
