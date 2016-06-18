
sampler2D implicitInput : register(s0);
float left : register(c0);
float right : register(c1);


float2 GetTextureCoord(float2 uv)
{
		float tx = lerp(0, 1, (uv.x-left)/(right-left));
		float2 pos = float2(tx, uv.y);
		
		return pos;
}

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
	if (uv.x >= left && uv.x <= right)
	{
		float2 pos = GetTextureCoord(uv);
		return tex2D(implicitInput, pos);
	}
	else return float4(0,0,0,0);
}

