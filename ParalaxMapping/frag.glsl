#version 440 core

out vec4 color;

in mat3 TBN_mat;
in vec2 Tex;
in vec3 Pos;

layout(binding = 0) uniform sampler2D albedo;
layout(binding = 1) uniform sampler2D depth_map;
layout(binding = 2) uniform sampler2D normal_tangent;

const vec3 LightPos = vec3(1.0, 0.0, 1.0);
const vec3 LightColor = vec3(1.0);

uniform vec3 viewer_pos;

void Lighting(vec2 tex_coords)
{
	bvec4 out_of_tex = bvec4(lessThan(tex_coords, vec2(0.0)), greaterThan(tex_coords, vec2(1.0)));
	if(any(out_of_tex))
		discard;

	vec3 normal = TBN_mat * (texture(normal_tangent, tex_coords).xyz * 2.0 - vec3(1.0));	
	vec3 lightDir = TBN_mat * normalize(LightPos - Pos);

	vec3 light = LightColor * 0.1;//ambient
	light += max(dot(normal, lightDir), 0.0) * LightColor;//diffuse
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(normalize(Pos - viewer_pos), reflectDir), 0.0), 32);
    light += 0.5 * spec * LightColor;//specular

	color = vec4(light * texture(albedo, tex_coords).rgb, 1.0);
}

#define STRENGTH 0.7

vec2 ParalaxMapping() 
{
	float depth = texture(depth_map, Tex).x * STRENGTH;
	vec3 viewDir = TBN_mat * normalize(Pos - viewer_pos);
	return Tex - (viewDir.yx / viewDir.z) * depth * STRENGTH; 
}

vec2 SteepParalaxMapping()
{
	#define STEP 0.1
	#define STEP_COUNT 10
	vec3 viewDir = TBN_mat * normalize(Pos - viewer_pos);
	vec2 tex_coords = Tex;

	for(int i = 0; i < STEP_COUNT; i++)
	{
		float depth = texture(depth_map, tex_coords).x;
		if(depth <= STEP * i) return tex_coords;
		tex_coords -= viewDir.yx / viewDir.z * (depth * STRENGTH) * STEP;
	}

	return tex_coords;
	#undef STEP
	#undef STEP_COUNT
}

vec2 ParalaxOcclusionMapping()
{
	#define STEP 0.1
	#define STEP_COUNT 10
	vec3 viewDir = TBN_mat * normalize(Pos - viewer_pos);
	vec2 tex_coords = Tex;

	for(int i = 0; i < STEP_COUNT; i++)
	{
		float depth = texture(depth_map, tex_coords).x;
		if(depth < STEP * i)
		{
			vec2 tex_coords_under = tex_coords;
			vec2 tex_coords_above = tex_coords - viewDir.yx / viewDir.z * (depth * STRENGTH) * STEP;
			float under_k = STEP * i - depth;
			float above_k = (STEP - 1) * i - texture(depth_map, tex_coords_above).x;
			return (tex_coords_above * under_k + tex_coords_under * above_k) / (under_k + above_k);
		}
		tex_coords -= viewDir.yx / viewDir.z * (depth * STRENGTH) * STEP;
	}
	return tex_coords;
	#undef STEP
	#undef STEP_COUNT
}

vec2 ReliefParalaxMapping()
{
	#define STEP 0.1
	#define STEP_COUNT 10
	#define CLARIFY_STEPS 32
	vec3 viewDir = TBN_mat * normalize(Pos - viewer_pos);
	vec2 tex_coords = Tex;

	for(int i = 0; i < STEP_COUNT; i++)
	{
		float depth = texture(depth_map, tex_coords).x;
		if(depth < STEP * i)
		{
			vec2 tex_coords_under = tex_coords;
			vec2 tex_coords_above = tex_coords - viewDir.yx / viewDir.z * (depth * STRENGTH) * STEP;
			float guess_depth_under = STEP * i;
			float guess_depth_above = STEP * (i + 1); 
			float guess_depth;

			for(int j = 0; j < CLARIFY_STEPS; j++) // binary search between under- and above- lying layers
			{
				tex_coords = (tex_coords_above + tex_coords_under) * 0.5;
				guess_depth = (guess_depth_above + guess_depth_under) * 0.5;
				if(texture(depth_map, tex_coords).x > guess_depth)
				{
					tex_coords_above = tex_coords;
					guess_depth_above = guess_depth;
				}
				else
				{
					tex_coords_under = tex_coords;
					guess_depth_under = guess_depth;
				}
			}

			return tex_coords;
		}
		tex_coords -= viewDir.yx / viewDir.z * (depth * STRENGTH) * STEP;
	}
	return tex_coords;
	#undef STEP
	#undef STEP_COUNT
	#undef CLARIFY_STEPS
}

void main()
{
	//vec2 tex_coords = ParalaxMapping();
	//vec2 tex_coords = SteepParalaxMapping();
	//vec2 tex_coords = ParalaxOcclusionMapping();
	vec2 tex_coords = ReliefParalaxMapping();

	Lighting(tex_coords);
}