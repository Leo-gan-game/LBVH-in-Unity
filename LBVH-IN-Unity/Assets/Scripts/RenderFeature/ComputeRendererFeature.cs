using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ComputeRendererFeature : ScriptableRendererFeature
{
    public ComputeSettings setting;

    private ComputePass csPass;
    private BlitPass blitPass;

    private LBVH lbvh;
    private ComputeBuffer buffer;

    private static ComputeRendererFeature _instance;
    public static ComputeRendererFeature Instance { get { return _instance; } }
    public ComputeBuffer Buffer { get => buffer; set => buffer = value; }
    public LBVH Lbvh { get => lbvh; set => lbvh = value; }
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
        public bool showConstructProcess;
        [Range(10,1000)]
        public int interval;
        public RenderPassEvent csPassEvent;
        public RenderPassEvent debugPassEvent;
        
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (buffer == null)
        {
            buffer = new ComputeBuffer(lbvh.Size.x * lbvh.Size.y * lbvh.Size.z, Marshal.SizeOf(typeof(AABB)), ComputeBufferType.Structured);
            buffer.SetData(lbvh.Array.ToArray());
        }
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
            lbvh = new LBVH();
            lbvh.CreateAABB(setting.size.x, setting.size.y, setting.size.z);
            

            csPass = new ComputePass(setting.cs, setting.kernelName, setting.size);
            csPass.renderPassEvent = setting.csPassEvent;
            blitPass = new BlitPass(setting.mtlDebugCS, Lbvh.Count, setting.showConstructProcess,setting.interval);
            blitPass.renderPassEvent = setting.debugPassEvent;
        }
        else
        {
            Release();
        }
    }

    private void Release()
    {
        if (buffer != null)
        {
            buffer.Release();
        }
    }

    private void OnDisable()
    {
        Release();

    }
}
