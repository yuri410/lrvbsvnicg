#include "waterDepth.vsh"

float4x4 mvp : register(c0);
float4x4 world : register(c4);
float3 viewPos : register(c8);

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    
};
struct VSOutput
{
    float4 Position : POSITION0;
    float4 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

VSOutput main(VSInput ip)
{
    VSOutput o;

    o.Position = mul(ip.Position, mvp);
    o.TexCoord.xy = ip.TexCoord;
    o.TexCoord.z = abs(ip.Position.x) - 35;

	o.TexCoord.z = o.TexCoord.z > 0 ? (1 - (o.TexCoord.z/15)) : 1;
	
	o.Normal = normalize((float3)mul(float4(0, 1, 0, 0), world));
    
    float3 wpos = mul(ip.Position, world).xyz;
    float d =distance(wpos, viewPos);
    
    o.TexCoord.w = saturate((15000-d) / 1500);
    
    return o;
}
