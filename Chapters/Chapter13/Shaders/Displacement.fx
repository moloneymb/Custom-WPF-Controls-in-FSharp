sampler2D input1 : register(S0);
sampler2D input2 : register(S1);

float scaleX : register(C0);
float scaleY : register(C1);

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
	float component = tex2D(input2, uv).r;
	float dispX = ((component - 0.5) * scaleX);
	float dispY = ((component - 0.5) * scaleY);
	
	float4 color = tex2D(input1, float2(uv.x + dispX, uv.y + dispY));
	return color;
}


