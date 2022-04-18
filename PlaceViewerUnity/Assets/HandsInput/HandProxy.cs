using System;
using UnityEngine;

[Serializable]
public class HandProxy
{
    public Transform Palm => this.palm;
    public Transform Wrist => this.wrist;
    public Transform ThumbMetacarpal => this.thumbMetacarpal;
    public Transform ThumbProximal => this.thumbProximal;
    public Transform ThumbDistal => this.thumbDistal;
    public Transform ThumbTip => this.thumbTip;
    public Transform IndexMetacarpal => this.indexMetacarpal;
    public Transform IndexProximal => this.indexProximal;
    public Transform IndexIntermediate => this.indexIntermediate;
    public Transform IndexDistal => this.indexDistal;
    public Transform IndexTip => this.indexTip;
    public Transform MiddleMetacarpal => this.middleMetacarpal;
    public Transform MiddleProximal => this.middleProximal;
    public Transform MiddleIntermediate => this.middleIntermediate;
    public Transform MiddleDistal => this.middleDistal;
    public Transform MiddleTip => this.middleTip;
    public Transform RingMetacarpal => this.ringMetacarpal;
    public Transform RingProximal => this.ringProximal;
    public Transform RingIntermediate => this.ringIntermediate;
    public Transform RingDistal => this.ringDistal;
    public Transform RingTip => this.ringTip;
    public Transform LittleMetacarpal => this.littleMetacarpal;
    public Transform LittleProximal => this.littleProximal;
    public Transform LittleIntermediate => this.littleIntermediate;
    public Transform LittleDistal => this.littleDistal;
    public Transform LittleTip => this.littleTip;

    [SerializeField]
    private Transform palm;
    [SerializeField]
    private Transform wrist;
    [SerializeField]
    private Transform thumbMetacarpal;
    [SerializeField]
    private Transform thumbProximal;
    [SerializeField]
    private Transform thumbDistal;
    [SerializeField]
    private Transform thumbTip;
    [SerializeField]
    private Transform indexMetacarpal;
    [SerializeField]
    private Transform indexProximal;
    [SerializeField]
    private Transform indexIntermediate;
    [SerializeField]
    private Transform indexDistal;
    [SerializeField]
    private Transform indexTip;
    [SerializeField]
    private Transform middleMetacarpal;
    [SerializeField]
    private Transform middleProximal;
    [SerializeField]
    private Transform middleIntermediate;
    [SerializeField]
    private Transform middleDistal;
    [SerializeField]
    private Transform middleTip;
    [SerializeField]
    private Transform ringMetacarpal;
    [SerializeField]
    private Transform ringProximal;
    [SerializeField]
    private Transform ringIntermediate;
    [SerializeField]
    private Transform ringDistal;
    [SerializeField]
    private Transform ringTip;
    [SerializeField]
    private Transform littleMetacarpal;
    [SerializeField]
    private Transform littleProximal;
    [SerializeField]
    private Transform littleIntermediate;
    [SerializeField]
    private Transform littleDistal;
    [SerializeField]
    private Transform littleTip;

    public Transform[] AllJoints
    {
        get
        {
            if (allJoints == null)
            {
                allJoints = GetAllJoints();
            }
            return allJoints;
        }
    }

    public Vector3 ModelFingerPointing = new Vector3(0, 0, 0);
    public Vector3 ModelPalmFacing = new Vector3(0, 0, 0);


    private Transform[] allJoints;
    private Transform[] GetAllJoints()
    {
        return new Transform[]
        {
            Palm,
            Wrist,
            ThumbMetacarpal,
            ThumbProximal,
            ThumbDistal,
            ThumbTip,
            IndexMetacarpal,
            IndexProximal,
            IndexIntermediate,
            IndexDistal,
            IndexTip,
            MiddleMetacarpal,
            MiddleProximal,
            MiddleIntermediate,
            MiddleDistal,
            MiddleTip,
            RingMetacarpal,
            RingProximal,
            RingIntermediate,
            RingDistal,
            RingTip,
            LittleMetacarpal,
            LittleProximal,
            LittleIntermediate,
            LittleDistal,
            LittleTip
        };
    }
}