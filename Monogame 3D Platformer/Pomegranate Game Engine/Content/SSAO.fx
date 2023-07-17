#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;
texture MainTexture;
float3 PointLightPosition;
float3 PointLightColor;
sampler MainTextureSampler = sampler_state
{
	Texture = (MainTexture);
	MAGFILTER = NONE;
	MINFILTER = NONE;
	MIPFILTER = NONE;
	AddressU = Wrap;
	AddressV = Wrap;
};


struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal: NORMAL0;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD1;
	float2 UV : TEXCOORD0;
	float3 WorldPosition : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = input.Normal;
	output.WorldPosition = input.Position;
	output.Color = input.Color;
	return output;
}
float CalculatePointLightBrightness(VertexShaderOutput input, float3 LightPosition, float Strength)
{
	float brightness = (1 / pow(distance(float3(input.WorldPosition.x, input.WorldPosition.y, input.WorldPosition.z), LightPosition), 2)) * Strength;
	float LightNormal = 1 - dot(normalize(input.WorldPosition - LightPosition), input.Normal);
	if (brightness < 0)
	{
		brightness = 0;
	}
	return LightNormal * brightness;
}
float CalculateSunBrightness(VertexShaderOutput input, float3 LightAngle, float Strength)
{
	float brightness = dot(input.Normal, normalize(LightAngle * -1)) * Strength;
	if (brightness < 0)
		brightness = 0;
	return brightness;
}
float4 MainPS(VertexShaderOutput input) : COLOR
{
	float brightness = 1.0f;
//brightness += CalculateSunBrightness(input, float3(1, -1, 1), 1);
//brightness += CalculatePointLightBrightness(input,float3(0,5,0),25);
float4 color = input.Color;
color *= tex2D(MainTextureSampler, input.UV);
return color * float4(brightness, brightness, brightness,1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};