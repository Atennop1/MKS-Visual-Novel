// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#ifndef NANINOVEL_CG_INCLUDED
#define NANINOVEL_CG_INCLUDED

static const float PI = 3.14159;

inline float IsPositionOutsideRect(float2 position, float4 rect)
{
    float2 isInside = step(rect.xy, position.xy) * step(position.xy, rect.zw);
    return 1.0 - isInside.x * isInside.y;
}

inline float IsPositionOutsideParallelogram(float2 position, float4 rect, float parallelogramAngle)
{
    float alpha = PI / 2.0 - radians(parallelogramAngle);
    float2 dPoint = rect.zw - rect.xy;
    float2 dPos = position - rect.xy;

    float4 rectModified = float4(0, 0, dPoint.x * tan(alpha) - dPoint.y, dPoint.y);
    float2 positionModified = float2(dPos.x * tan(alpha) - dPos.y, dPos.y);

    return IsPositionOutsideRect(positionModified, rectModified);
}

// Performs standard tex2D but returns clip color when UV is out of 0-1 range.
inline fixed4 Tex2DClip01(sampler2D tex, float2 uvCoord, fixed4 clipColor)
{
    const float4 UV_RANGE = float4(0, 0, 1, 1);
    float isUVOutOfRange = IsPositionOutsideRect(uvCoord, UV_RANGE);
    return lerp(tex2D(tex, uvCoord), clipColor, isUVOutOfRange);
}

inline fixed4 PremultiplyAlpha (fixed4 color)
{
    #ifndef PREMULTIPLIED_ALPHA
    color.rgb *= color.a;
    #endif
    return color;
}

float DistanceFromCenterToSquareEdge(float2 dir)
{
    dir = abs(dir);
    float dist = dir.x > dir.y ? dir.x : dir.y;
    return dist;
}

float4 Mod(float4 x, float4 y)
{
    return x - y * floor(x / y);
}

float4 Mod289(float4 x)
{
    return x - floor(x / 289.0) * 289.0;
}

float4 Permute(float4 x)
{
    return Mod289(((x * 34.0) + 1.0) * x);
}

float4 TaylorInvSqrt(float4 r)
{
    return (float4)1.79284291400159 - r * 0.85373472095314;
}

float2 Fade(float2 t) 
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

float PerlinNoise(float2 uv)
{
    float4 pi = floor(uv.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
    float4 pf = frac(uv.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
    pi = Mod289(pi); // To avoid truncation effects in permutation.
    float4 ix = pi.xzxz;
    float4 iy = pi.yyww;
    float4 fx = pf.xzxz;
    float4 fy = pf.yyww;

    float4 i = Permute(Permute(ix) + iy);

    float4 gx = frac(i / 41.0) * 2.0 - 1.0;
    float4 gy = abs(gx) - 0.5;
    float4 tx = floor(gx + 0.5);
    gx = gx - tx;

    float2 g00 = float2(gx.x, gy.x);
    float2 g10 = float2(gx.y, gy.y);
    float2 g01 = float2(gx.z, gy.z);
    float2 g11 = float2(gx.w, gy.w);

    float4 norm = TaylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
    g00 *= norm.x;
    g01 *= norm.y;
    g10 *= norm.z;
    g11 *= norm.w;

    float n00 = dot(g00, float2(fx.x, fy.x));
    float n10 = dot(g10, float2(fx.y, fy.y));
    float n01 = dot(g01, float2(fx.z, fy.z));
    float n11 = dot(g11, float2(fx.w, fy.w));

    float2 fade_xy = Fade(pf.xy);
    float2 nx = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
    float nxy = lerp(nx.x, nx.y, fade_xy.y);
    return 2.3 * nxy;
}

#endif // NANINOVEL_CG_INCLUDED
