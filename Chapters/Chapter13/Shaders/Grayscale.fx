sampler2D implicitInput : register(s0);

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
	float4 src = tex2D(implicitInput, uv);

	float4 dst;
	float average = (src.r + src.g + src.b)/3;
	dst.rgb = average;
	dst.a = src.a;
	
	return dst;
}