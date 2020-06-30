Shader "Cluster/DebugCluster"
{
	Properties
	{
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha // 传统透明度
		Cull front
		//ZTest off
		Pass
		{
			HLSLPROGRAM

			#pragma vertex main_VS
			#pragma fragment main_PS
			#pragma geometry main_GS
			#pragma target 5.0
			#pragma enable_d3d11_debug_symbols
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct VertexShaderOutput
			{
				float4 Min          : AABB_MIN;  // Min vertex position in view space.
				float4 Max          : AABB_MAX;  // Max vertex position in view space.
				float4 Color        : COLOR;     // Cluster color.
			};

			struct GeometryShaderOutput
			{
				float4 Color        : COLOR;
				float4 Position     : SV_POSITION;          // Clip space position.
				float4 worldSpacePosition : TEXCOORD0;
				float4 dist : TEXCOORD1;
				float4 area : TEXCOORD2;
			};

			struct AABB
			{
				int morton;
				float4 Min;
				float4 Max;
				float4 color;
				float3 index;
			};

			StructuredBuffer<AABB> buffer;
			bool CMin(float3 a, float3 b)
			{
				if (a.x < b.x && a.y < b.y && a.z < b.z)
					return true;
				else
					return false;
			}

			bool CMax(float3 a, float3 b)
			{
				if (a.x > b.x&& a.y > b.y&& a.z > b.z)
				{
					return true;
				}
				else
				{
					return false;
				}
			}

			float4x4 _CameraWorldMatrix;
			float4x4 _Perspective;

			

			float4 WorldToProject(float4 posWorld)
			{
				float4 posVP0 = TransformWorldToHClip(posWorld);
				return posVP0;
			}

			VertexShaderOutput main_VS(uint VertexID : SV_VertexID)
			{
				uint clusterID = VertexID; ;// UniqueClusters[VertexID];// VertexID;

				VertexShaderOutput vsOutput = (VertexShaderOutput)0;

				AABB aabb = buffer[clusterID];// ClusterAABBs[VertexID];

				vsOutput.Min = aabb.Min;
				vsOutput.Max = aabb.Max;
				int x = clusterID % 10;
				int y = clusterID / 10%10;
				int z = clusterID / 100;
				float4 factor = aabb.Max - aabb.Min;
				factor *= 0.25;
				vsOutput.Max = aabb.Min + factor;
				vsOutput.Color = aabb.color;
				return vsOutput;
			}


			// Geometry shader to convert AABB to cube.
			[maxvertexcount(16)]
			void main_GS(point VertexShaderOutput IN[1], inout TriangleStream<GeometryShaderOutput> OutputStream)
			{
				float4 min = IN[0].Min;
				float4 max = IN[0].Max;

				// Clip space position
				GeometryShaderOutput OUT = (GeometryShaderOutput)0;

				// AABB vertices
				const float4 Pos[8] = {
					float4(min.x, min.y, min.z, 1.0f),    // 0
					float4(min.x, min.y, max.z, 1.0f),    // 1
					float4(min.x, max.y, min.z, 1.0f),    // 2

					float4(min.x, max.y, max.z, 1.0f),    // 3
					float4(max.x, min.y, min.z, 1.0f),    // 4
					float4(max.x, min.y, max.z, 1.0f),    // 5
					float4(max.x, max.y, min.z, 1.0f),    // 6
					float4(max.x, max.y, max.z, 1.0f)     // 7
				};

				// Colors (to test correctness of AABB vertices)
				const float4 Col[8] = {
					float4(0.0f, 0.0f, 0.0f, 1.0f),       // Black
					float4(0.0f, 0.0f, 1.0f, 1.0f),       // Blue
					float4(0.0f, 1.0f, 0.0f, 1.0f),       // Green
					float4(0.0f, 1.0f, 1.0f, 1.0f),       // Cyan
					float4(1.0f, 0.0f, 0.0f, 1.0f),       // Red
					float4(1.0f, 0.0f, 1.0f, 1.0f),       // Magenta
					float4(1.0f, 1.0f, 0.0f, 1.0f),       // Yellow
					float4(1.01, 1.0f, 1.0f, 1.0f)        // White
				};

				const uint Index[18] = {
					0, 1, 2,
					3, 6, 7,
					4, 5, -1,
					2, 6, 0,
					4, 1, 5,
					3, 7, -1
				};

				[unroll]
				for (uint i = 0; i < 18; ++i)
				{
					if (Index[i] == (uint) - 1)
					{
						OutputStream.RestartStrip();
					}
					else
					{
						OUT.Position = WorldToProject(Pos[Index[i]]);
						OUT.Color = IN[0].Color;
						OutputStream.Append(OUT);
					}
				}
			}

			float4 main_PS(GeometryShaderOutput IN) : SV_Target
			{
				return IN.Color;
			}
			ENDHLSL
		}
	}
}