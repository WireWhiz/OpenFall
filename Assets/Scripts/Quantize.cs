using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Quantizer
{
    public static float3 Quantize(this float3 value, float quantization)
    {
        return new float3(Quantize(value.x, quantization), Quantize(value.y, quantization), Quantize(value.z, quantization));
    }
    public static float2 Quantize(this float2 value, float quantization)
    {
        return new float2(Quantize(value.x, quantization), Quantize(value.y, quantization));
    }
    public static quaternion Quantize(this quaternion value, float quantization)
    {
        return new quaternion(Quantize(value.value.x, quantization), Quantize(value.value.y, quantization), Quantize(value.value.z, quantization), Quantize(value.value.w, quantization));
    }

    public static float Quantize(float value, float quantization)
    {
        //UnityEngine.Debug.Log(string.Format("Quantized {0} to {1}", value, (int)(value * quantization) / quantization));
        return (int)(value * quantization) / quantization;
    }
}
