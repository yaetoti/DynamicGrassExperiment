#include "ShaderApiReflectionSupport.hlsl"

struct SurfaceSample {
  float3 position;
  float3 normal;
};

StructuredBuffer<SurfaceSample> samples;

float3x3 GetWorldRotation(float3 normal) {
  float3 up = abs(normal.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
  float3 tangent = normalize(cross(up, normal));
  float3 bitangent = normalize(cross(normal, tangent));
  return transpose(float3x3(tangent, normal, bitangent));
}

UNITY_EXPORT_REFLECTION void Grass(float instanceId, float3 position, float3 normal, float3 tangent, out float3 outPosition, out float3 outNormal, out float3 outTangent) {
  SurfaceSample sample = samples[(int)instanceId];
  
  float3x3 transform = GetWorldRotation(sample.normal);
  outPosition = mul(transform, position) + sample.position;
  outNormal = normalize(mul(transform, normal));
  outTangent = normalize(mul(transform, tangent));
  
  // Don't work either, not rendering at world origin as expected
  //outPosition = position;
  //outNormal = normal;
}
