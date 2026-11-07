uint Rand(uint s) {
  s ^= 2747636419u;
  s *= 2654435769u;
  s ^= s >> 16;
  s *= 2654435769u;
  s ^= s >> 16;
  s *= 2654435769u;
  return s;
}

float Rand01(uint s) {
  return Rand(s) / 4294967296.0f;
}

float3 RandomBarycentric(uint r1, uint r2) {
  float u = Rand01(r1);
  float v = Rand01(r2);
  
  if (u + v > 1.0) {
    u = 1.0 - u;
    v = 1.0 - v;
  }
  
  return float3(u, v, 1.0 - u - v);
}

float3 RotateAround(float3 v, float3 axis, float angle) {
  // https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
  float s = sin(angle);
  float c = cos(angle);
  return v * c + cross(axis, v) * s + axis * (dot(axis, v) * (1 - c));
}