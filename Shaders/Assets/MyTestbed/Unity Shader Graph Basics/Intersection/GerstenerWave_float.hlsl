void GerstnerWave_float(
    float3 posOS,
    float time,
    float2 dir,
    float amplitude,
    float frequency,
    float speed,
    out float3 offset)
{
    float2 d = normalize(dir);
    float k = frequency;
    float w = speed;
    float f = dot(d, posOS.xz) * k - time * w;

    offset.y = amplitude * sin(f);
    // xz 수평 이동
    float2 xz = d * amplitude * cos(f);
    offset.x = xz.x;
    offset.z = xz.y;
}