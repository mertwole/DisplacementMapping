#version 440 core
layout(quads, equal_spacing, ccw) in;

uniform mat4 transform_mat;

in mat3[] _TBN_mat;
in vec2[] _Tex;
in vec3[] _normal;

out mat3 TBN_mat;
out vec2 Tex;
out vec3 Pos;

layout(binding = 1) uniform sampler2D depthmap;

vec3 MixQuad(vec3 lb, vec3 lt, vec3 rt, vec3 rb)
{
	vec3 up = mix(lt, rt, gl_TessCoord.x);
	vec3 down = mix(lb, rb, gl_TessCoord.x);
	return mix(down, up, gl_TessCoord.y);
}

vec2 MixQuad(vec2 lb, vec2 lt, vec2 rt, vec2 rb)
{
	vec2 up = mix(lt, rt, gl_TessCoord.x);
	vec2 down = mix(lb, rb, gl_TessCoord.x);
	return mix(down, up, gl_TessCoord.y);
}

void main()
{
	TBN_mat = _TBN_mat[0];
	Tex = MixQuad(_Tex[0], _Tex[1], _Tex[2], _Tex[3]);

	Pos = MixQuad(gl_in[0].gl_Position.xyz, gl_in[1].gl_Position.xyz, gl_in[2].gl_Position.xyz, gl_in[3].gl_Position.xyz);
	Pos -= _normal[0] * texture(depthmap, Tex).x * 0.2;
	vec4 pos = transform_mat * vec4(Pos, 1.0);
	Pos = pos.xyz / pos.w;

	gl_Position = vec4(Pos, 1.0);
}