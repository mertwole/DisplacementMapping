#version 440 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 tex;
layout(location = 2) in vec3 norm;
layout(location = 3) in vec3 tangent;

out mat3 _TBN_mat;
out vec2 _Tex;
out vec3 _normal;

void main()
{
	vec3 bitangent = normalize(cross(tangent, norm));

	_TBN_mat = mat3(
	tangent.x, bitangent.x, norm.x,
	tangent.y, bitangent.y, norm.y,
	tangent.z, bitangent.z, norm.z);

	_Tex = tex;
	_normal = norm;

	gl_Position = vec4(pos, 1.0);
}