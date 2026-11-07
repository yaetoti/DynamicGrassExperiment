#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "ShaderApiReflectionSupport.hlsl"
#include "../Math.hlsl"

struct SurfaceSample {
  float3 position;
  float3 normal;
  float3 tangent;
};

TEXTURE2D(bendTexture);
SAMPLER(sampler_bendTexture);
StructuredBuffer<SurfaceSample> samples;
float4 worldBounds;

float3x3 GetWorldRotation(float3 normal, float3 tangent) {
  float3 bitangent = normalize(cross(normal, tangent));
  return transpose(float3x3(tangent, normal, bitangent));
}

UNITY_EXPORT_REFLECTION void Grass(float instanceId, float time, float3 position, float3 normal, float3 tangent, float2 uv, out float3 outPosition, out float3 outNormal, out float3 outTangent) {
  SurfaceSample sample = samples[(int)instanceId];
  float3x3 transform = GetWorldRotation(sample.normal, sample.tangent);

  // TODO controlled by wind projection onto local grass forward?
  // TODO (direction / wind strength) controlled by perlin noise?
  
  // Making curve based on v coordinate
  float bendModifier = uv.y * uv.y * 0.25 + 0.75;

  // Sample bend texture
  float2 texCoords = float2((sample.position.x - worldBounds.x) / worldBounds.z, (sample.position.z - worldBounds.y) / worldBounds.w);
  float bend = SAMPLE_TEXTURE2D_LOD(bendTexture, sampler_bendTexture, texCoords, 0);
  //float bend = 0.0f;

  // Add wind sway
  float sway = 0.1f * sin(time + sample.position.x * 7 + sample.position.z * 13);
  bend += sway;
  bend = clamp(bend, -1.0f, 1.0f);

  // Apply bend
  float3 p1 = RotateAround(position, float3(1, 0, 0), bendModifier * bend * PI * 0.5);
  // Mesh space to world space
  outPosition = mul(transform, p1) + sample.position;
  outNormal = normalize(mul(transform, normal));
  outTangent = normalize(mul(transform, tangent));
}
