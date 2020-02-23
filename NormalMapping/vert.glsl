#version 440 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 tex;
layout(location = 2) in vec3 norm;
layout(location = 3) in vec3 tangent;

uniform mat4 transform_mat;

out mat3 TBN_mat;
out vec2 Tex;
out vec3 Pos;

void main()
{
	vec3 bitangent = normalize(cross(tangent, norm));
	TBN_mat = mat3(
	tangent.x, bitangent.x, norm.x,
	tangent.y, bitangent.y, norm.y,
	tangent.z, bitangent.z, norm.z);

	Tex = tex;
	Pos = pos;

	gl_Position = transform_mat * vec4(pos, 1.0);
}