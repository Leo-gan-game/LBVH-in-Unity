using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ComputeRendererFeature : ScriptableRendererFeature
{
    public ComputeSettings setting;

    private ComputePass csPass;
    private BlitPass blitPass;

    private static ComputeRendererFeature _instance;
    public static ComputeRendererFeature Instance { get { return _instance; } }

    public ComputePass CsPass { get => csPass; set => csPass = value; }
    public BlitPass BlitPass { get => blitPass; set => blitPass = value; }

    [System.Serializable]
    public class ComputeSettings
    {
        public ComputeShader cs;
        public string kernelName;
        public Vector3Int size=new Vector3Int(3,3,3);
        public Material mtlDebugCS;
        public bool debug;
        public RenderPassEvent csPassEvent;
        public RenderPassEvent debugPassEvent;
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(csPass);
        if (setting.debug)
        {
            blitPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(blitPass);
        }
    }

    public override void Create()
    {
        if (isActive)
        {
            _instance = this;
            csPass = new ComputePass(setting.cs, setting.kernelName, setting.size);
            csPass.renderPassEvent = setting.csPassEvent;
            blitPass = new BlitPass(setting.mtlDebugCS, csPass.Lbvh.Count);
            blitPass.renderPassEvent = setting.debugPassEvent;
        }
    }


}
