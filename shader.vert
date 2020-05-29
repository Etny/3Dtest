#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;


layout (std140) uniform Matrices
{
	mat4 projection;
	mat4 view;
};

uniform int pointLightCount;
uniform int spotLightCount;
uniform int dirLightCount;

const int MaxLights = 4;

struct LightColor
{
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight {    
    vec3 position;
    
    float constant;
    float linear;
    float quadratic;  

    LightColor color;
};  

struct SpotLight
{
	vec3 position;
	vec3 direction;
	float cutoff;
	float outerCutoff;

	float constant;
    float linear;
    float quadratic;  

	LightColor color;
};

struct DirLight
{
	vec3 direction;

	LightColor color;
};

uniform PointLight pointLights[MaxLights];
uniform SpotLight spotLights[MaxLights];
uniform DirLight dirLights[MaxLights];

out VS_TAGENTLIGHTS{
	vec3 pointLightPos[MaxLights];
	vec3 spotLightPos[MaxLights];
	vec3 spotLightDir[MaxLights];
	vec3 dirLightDir[MaxLights];
} tangentLights;

//uniform mat4 lightSpace;
uniform mat4 model;
uniform vec3 viewPos;

out VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} vs_out;

void main()
{
	gl_Position = projection * view * model * vec4(aPos, 1.0);
	vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
	//vs_out.Normal = mat3(transpose(inverse(model))) * aNormal;
	vs_out.TexCoords = vec2(aTexCoords.x, aTexCoords.y);
	//FragPosLightSpace = lightSpace * vec4(FragPos, 1);

	vec3 T = normalize(vec3(model * vec4(aTangent, 0.0)));
	vec3 B = normalize(vec3(model * vec4(aBitangent, 0.0)));
	vec3 N = normalize(vec3(model * vec4(aNormal, 0.0)));
	mat3 TBN = transpose(mat3(T, B, N));

	vs_out.TBN = TBN;

	vs_out.TangentFragPos = TBN * vs_out.FragPos;
	vs_out.TangentViewPos = TBN * viewPos;

	for(int i = 0; i < pointLightCount; ++i)
		tangentLights.pointLightPos[i] = TBN * pointLights[i].position;
	

	for(int i = 0; i < spotLightCount; ++i)
	{
		tangentLights.spotLightPos[i] = TBN * spotLights[i].position;
		tangentLights.spotLightDir[i] = TBN * spotLights[i].direction;
	}

	for(int i = 0; i < dirLightCount; ++i)
		tangentLights.dirLightDir[i] = TBN * dirLights[i].direction;
	
}
