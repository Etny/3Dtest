#version 330 core
struct Material
{
	sampler2D texture_diffuse1;
	sampler2D texture_diffuse2;
	sampler2D texture_diffuse3;
	sampler2D texture_diffuse4;
	sampler2D texture_specular1;
	sampler2D texture_specular2;
	sampler2D texture_specular3;
	sampler2D texture_specular4;

	float shininess;
};

struct LightColor
{
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
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
uniform SpotLight spotLight;

struct DirLight
{
	vec3 direction;

	LightColor color;
};
uniform DirLight dirLight;

struct PointLight {    
    vec3 position;
    
    float constant;
    float linear;
    float quadratic;  

    LightColor color;
};  
#define NR_POINT_LIGHTS 1
uniform PointLight pointLights[1];

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;
in vec4 FragPosLightSpace;

uniform Material material;

uniform samplerCube shadowMap;
uniform float farPlane;

uniform vec3 viewPos;

vec3 CalcLight(vec3 lightDir, LightColor color, vec3 normal, vec3 viewDir, float attenuation, float intensity, bool useShadows);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir);
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);  
float CalcShadow(vec4 fragPosLightSpace, vec3 lightDir, vec3 normal);

float near = 0.1;
float far = 100;

void main()
{
	vec3 normal = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);

	vec3 Result = vec3(0.0);
	
	for(int i = 0; i < NR_POINT_LIGHTS; i++)
		Result += CalcPointLight(pointLights[i], normal, viewDir);

	//Result += CalcDirLight(dirLight, normal, viewDir);

	//Result += CalcSpotLight(spotLight, normal, viewDir);

	FragColor = vec4(Result, 1.0);

	//FragColor = vec4(vec3(1 - CalcShadow(FragPosLightSpace)), 1.0);
}

float CalcShadow(vec4 fragPosLightSpace, vec3 lightDir, vec3 normal)
{
	float shadow = 0;

	vec3 lightSpaceNDC = fragPosLightSpace.xyz / fragPosLightSpace.w;

	if(lightSpaceNDC.z > 1)
		return 0;

	vec3 lightMapCoords = lightSpaceNDC * .5 + .5;

	float closest = texture(shadowMap, lightMapCoords.xy).r;
	float current = lightMapCoords.z;

	float bias = max(0.01 * (dot(normal, lightDir)), 0.005);

//	if(closest < current - bias) shadow = 1;

	vec2 texelSize = 1.0 / textureSize(shadowMap, 0);

	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float surroundDepth = texture(shadowMap, lightMapCoords.xy + vec2(x, y) * texelSize).r;
			if(current - bias > surroundDepth) shadow += 1.0;
		}
	}

	shadow /= 9.0;



	return shadow;
}

vec3 CalcLight(vec3 lightDir, LightColor color, vec3 normal, vec3 viewDir, float attenuation, float intensity, bool useShadows)
{
	float shadow = 1;
	if(useShadows) shadow -= CalcShadow(FragPosLightSpace, lightDir, normal);

	float diff = max(dot(normal, lightDir), 0.0);
	float spec = pow(max(dot(normal, normalize(lightDir + viewDir)), 0.0), material.shininess * 3);

	vec3 texel = vec3(texture(material.texture_diffuse1, TexCoords));

	vec3 ambient = color.ambient * attenuation * texel;
	vec3 diffuse = color.diffuse * intensity * attenuation * diff * texel;
	vec3 specular = color.specular * spec * vec3(texture(material.texture_specular1, TexCoords)) * intensity * attenuation;
	return vec3(ambient + (shadow * diffuse) + (shadow * specular));
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir)
{
	float distance = length(light.position - FragPos);
	float attenuation = 1 / (light.constant + (light.linear * distance) + (light.quadratic * pow(distance, 2)));

	return CalcLight(normalize(light.position - FragPos), light.color, normal, viewDir, attenuation, 1.0, false);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir)
{
	float distance = length(light.position - FragPos);
	float attenuation = 1 / (light.constant + (light.linear * distance) + (light.quadratic * pow(distance, 2)));

	vec3 lightDir = normalize(light.position - FragPos);
	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutoff - light.outerCutoff;
	float intensity = clamp((theta - light.outerCutoff) / epsilon, 0.0, 1.0);

	return CalcLight(lightDir, light.color, normal, viewDir, attenuation, intensity, false);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
	return CalcLight(normalize(-light.direction), light.color, normal, viewDir, 1.0, 1.0, true);
}