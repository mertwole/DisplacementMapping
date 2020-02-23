#version 440 core

out vec4 color;

in mat3 TBN_mat;
in vec2 Tex;
in vec3 Pos;

layout(binding = 0) uniform sampler2D albedo;
layout(binding = 2) uniform sampler2D normal;

const vec3 LightPos = vec3(2.0, 0.0, 1.0);
const vec3 LightColor = vec3(1.0);

uniform vec3 viewer_pos;

void main()
{
	vec3 normal_world = TBN_mat * (texture(normal, Tex).xyz * 2.0 - vec3(1.0));

	vec3 lightDir = normalize(LightPos - Pos);

	vec3 light = LightColor * 0.1;//ambient

	light += max(dot(normal_world, lightDir), 0.0) * LightColor;//diffuse

	vec3 viewDir = normalize(viewer_pos - Pos);
	vec3 reflectDir = reflect(-lightDir, normal_world);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    light += 0.5 * spec * LightColor;//specular

	color = vec4(light * texture(albedo, Tex).rgb, 1.0);
}