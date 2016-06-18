sampler2D implicitInput : register(s0);
float radius : register(c0);
float angle : register(c1);

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
	float2 center = (0.5, 0.5);
	float2 delta = uv - center;
	
	float distance = length(delta);
	float phi = atan2(delta.y, delta.x) + angle * ((radius - distance)/radius);
	
	float2 uv1 = center + float2(distance * cos(phi), distance * sin(phi));
	
	float4 src = tex2D(implicitInput, uv);
	float4 dst = tex2D(implicitInput, uv1);

	return (distance < radius) ? dst : src;
}