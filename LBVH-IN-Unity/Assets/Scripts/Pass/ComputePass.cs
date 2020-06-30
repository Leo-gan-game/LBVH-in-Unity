using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ComputePass : ScriptableRenderPass
{
    private ComputeShader cs;
    private int kernel;
    private LBVH lbvh;
    private ComputeBuffer buffer;

    const string profilerTag = "Compute";

    ProfilingSampler profilingSampler = new ProfilingSampler(profilerTag);
    private RenderTargetHandle handle;

    public RenderTargetHandle Handle { get { return handle; } }

    public ComputeBuffer Buffer { get => buffer; set => buffer = value; }
    public LBVH Lbvh { get => lbvh; set => lbvh = value; }

    public ComputePass(ComputeShader compute,string kernelName,Vector3Int size)
    {
        cs = compute;
        kernel =cs.FindKernel(kernelName);
        lbvh = new LBVH();
        lbvh.CreateAABB(size.x, size.y, size.z);
        buffer = new ComputeBuffer(lbvh.Size.x * lbvh.Size.y * lbvh.Size.z, Marshal.SizeOf(typeof(AABB)), ComputeBufferType.Structured);
        buffer.SetData(lbvh.Array.ToArray());
        handle.Init("_Texture");
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
    {
        desc.enableRandomWrite = true;
        cmd.GetTemporaryRT(handle.id, desc);
        base.Configure(cmd, desc);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        using (new ProfilingScope(cmd, profilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.SetComputeTextureParam(cs, kernel, "Result", handle.Identifier());
            cmd.SetComputeBufferParam(cs, kernel, "buffer", buffer);
            cmd.DispatchCompute(cs,kernel, lbvh.Size.x, lbvh.Size.y, lbvh.Size.z);
            
            context.ExecuteCommandBuffer(cmd);
        }
        CommandBufferPool.Release(cmd);
    }
    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
    }
}

