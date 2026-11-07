float3 RotateAround(float3 v, float3 axis, float angle) {
  // https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
  float s = sin(angle);
  float c = cos(angle);
  return v * c + cross(axis, v) * s + axis * (dot(axis, v) * (1 - c));
}