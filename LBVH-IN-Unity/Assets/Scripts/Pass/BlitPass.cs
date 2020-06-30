
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
public class BlitPass : ScriptableRenderPass
{
    const string profilerTag = "Blit";
    internal RenderTargetIdentifier destination { get; private set; }
    public int DrawAABBCount { get => drawAABBCount; set => drawAABBCount = value; }

    ProfilingSampler profilingSampler = new ProfilingSampler(profilerTag);
    private int drawAABBCount;
    private int currentDrawCount=1;// The count change with time currentDrawCount[1,drawAABBCount]
    private long lastTime = 0;
    private long deltatime=0;
    private bool showConstructProcessing;
    private int interval = 500;
    Material blitMaterial;
    public BlitPass(Material m,int count,bool showProcessing=false, int intervaltime=500)
    {
        blitMaterial = m;
        drawAABBCount = count;
        showConstructProcessing = showProcessing;
        interval = intervaltime;
        if (!showProcessing)
        {
            currentDrawCount = count;
        }
    }
    public void Setup(RenderTargetIdentifier renderTarget)
    {
        destination = renderTarget;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        var camera = renderingData.cameraData.camera;
        if (showConstructProcessing)
        {
            deltatime += DateTime.Now.ToUniversalTime().Ticks / 10000 - lastTime;
            lastTime = DateTime.Now.ToUniversalTime().Ticks / 10000;
            if (deltatime > interval)
            {
                currentDrawCount = Mathf.Max(1, (currentDrawCount + 1) % (drawAABBCount + 1));
                if(currentDrawCount== drawAABBCount)
                {
                    deltatime = -1000;//If all AABBs are displayed, currentDrawCount equal drawAABBCount, wait for 1 second
                }
                else if (currentDrawCount%4==0)
                {
                    deltatime = -500;//If a Z-oroder are displayed, currentDrawCount mod(Modulus Operation) 4 equal 0, wait for 0.5 second
                }
                else
                { 
                    deltatime = 0;
                }
            }
        }
        
        using (new ProfilingScope(cmd, profilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            blitMaterial.SetBuffer("buffer", ComputeRendererFeature.Instance.Buffer);
            blitMaterial.SetMatrix("_CameraWorldMatrix", camera.transform.localToWorldMatrix);
            Matrix4x4 perspectivematrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
            blitMaterial.SetMatrix("_Perspective", perspectivematrix);
            blitMaterial.SetPass(0);
            cmd.DrawProcedural(camera.cameraToWorldMatrix, blitMaterial, 0, MeshTopology.Points, currentDrawCount);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        CommandBufferPool.Release(cmd);
    }
}

