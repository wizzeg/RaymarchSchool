#ifndef CustomFunc
#define CustomFunc
#endif  
struct Shape
{
    float3 position;
    float3 scale;
    float4 color;
    uint type;
};

StructuredBuffer<Shape> ShapesBuffer;
int ShapesCount;
float SmoothFactor;

float maxcomp(float2 vec)
{
    return max(vec.x, vec.y);
}

float maxcomp(float3 vec)
{
    float comp = max(vec.x, vec.y);
    return max(comp, vec.z);
}

float smin(float a, float b, float k)
{
    k *= 8.0 / 1;
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - h * h * h * (4.0 - h) * k * (1.0 / 16.0);    
}

float CubeSDF(float3 RayPosition, float3 CubePosition, float3 Scale)
{
    // to rotate, we'd rotate the ray position around the cube
    float3 dist = abs(RayPosition - CubePosition) - Scale;
    return length(max(dist, 0.0)) + min(maxcomp(dist), 0.0);
}

float RoundedCubeSDF(float3 RayPosition, float3 CubePosition, float3 Scale, float Rounding )
{
  float3 dist = abs(RayPosition - CubePosition) - Scale + Rounding;
  return length(max(dist,0.0)) + min(max(dist.x,max(dist.y,dist.z)),0.0) - Rounding;
}

float TorusSDF(float3 RayPosition, float3 TorusPosition, float2 Scale )
{
    float3 torusDist = RayPosition - TorusPosition;
    float2 dist = float2(length(torusDist.xz)-Scale.x,torusDist.y);
    return length(dist)-Scale.y;
}

float SphereSDF(float3 RayPosition, float3 SpherePos, float Radius)
{
    return distance(SpherePos, RayPosition) - Radius;
}

float SDFbyType(float3 position, float3 scale, uint type, float3 ray)
{
    if (type == (uint)0)
    {
        return SphereSDF(ray, position, scale.x);
    }
    else if (type == (uint)1)
    {
        return RoundedCubeSDF(ray, position, scale, 0.075);
    }
    else if (type == (uint)2)
    {
        return TorusSDF(ray, position, scale.xy);
    }
    return 999999;
}

float SDF(float3 RayPosition)
{
    float distance = 99999;
    if ( ShapesCount > 0)
    {
        float minDistance = SDFbyType(ShapesBuffer[0].position, ShapesBuffer[0].scale, ShapesBuffer[0].type, RayPosition);
        for (int i = 1; i < ShapesCount; i++)
        {
            minDistance = smin(minDistance, SDFbyType(ShapesBuffer[i].position, ShapesBuffer[i].scale, ShapesBuffer[i].type, RayPosition), SmoothFactor);
        }
        distance = minDistance;
    }
    return distance;
}

void GetNormal_float(float run, float closest, float3 WorldPos, out float3 Normal)
{
    if (run > 0.5)
    {
        float2 delta = float2(2e-2, 0);
        float distance = SDF(WorldPos);
        Normal = distance - float3(SDF(WorldPos - delta.xyy),
                                SDF(WorldPos - delta.yxy),
                                SDF(WorldPos - delta.yyx));
        Normal = normalize(Normal);
    }
    else 
    {
        Normal = float3(1.0, 1.0, 1.0);
    }
}


void GetLight_float(float3 LightDirection, float3 Normal, out float Light)
{
    Light = dot(LightDirection, Normal);
    Light = saturate(Light);
}

void RayMarch_float(float minStep, float minStepThreshold, float maxIterations, float cutoffDistance, float3 rayDir, float3 fragPos, out float3 collision, out float alpha, out float closest, out float stepsLeft)
{
    float threshold = 0.001f;
    float maxDist = 99999;
    closest = maxDist;
    collision = fragPos;
    alpha = 0;
    float fractoin = 1/minStepThreshold;
    stepsLeft = -1;
    for (int i = 0; i < maxIterations; i++)
    {
        float dist = SDF(collision);
        closest = min(dist, closest);
        if (dist < threshold || dist > cutoffDistance)
        {
            alpha = 1 * (dist < threshold);
            stepsLeft = (maxIterations - i)/maxIterations;
            break;
        }
        collision += rayDir * max(dist, min(((maxIterations - i)*1.25) * minStep, minStep) * (1 - step( minStepThreshold, maxIterations - i)));
    }
    closest *= (1 - alpha);
}


