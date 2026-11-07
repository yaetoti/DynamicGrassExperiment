#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "ShaderApiReflectionSupport.hlsl"
#include "../Math.hlsl"

struct SurfaceSample {
  float3 position;
  float3 normal;
  float3 tangent;
};

StructuredBuffer<SurfaceSample> samples;

float3x3 GetWorldRotation(float3 normal, float3 tangent) {
  float3 bitangent = normalize(cross(normal, tangent));
  return transpose(float3x3(tangent, normal, bitangent));
}

UNITY_EXPORT_REFLECTION void Grass(float instanceId, float3 position, float3 normal, float3 tangent, float2 uv, out float3 outPosition, out float3 outNormal, out float3 outTangent) {
  // TODO controlled by projection texture
  float bend = uv.y * uv.y * 0.25 + 0.75;
  // TODO controlled by noise
  float sway = 0;
  
  SurfaceSample sample = samples[(int)instanceId];
  float3x3 transform = GetWorldRotation(sample.normal, sample.tangent);
  
  float3 p1 = RotateAround(position, float3(1, 0, 0), bend * PI * 0.5);
  
  //outPosition = mul(transform, position) + sample.position;
  outPosition = mul(transform, p1) + sample.position;
  outNormal = normalize(mul(transform, normal));
  outTangent = normalize(mul(transform, tangent));
}
