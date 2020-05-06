#version 330 core
struct Material
{
	sampler2D diffuse;
	sampler2D specular;
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
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[4];

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

layout(pixel_center_integer) in vec4 gl_FragCoord;

uniform Material material;

uniform vec3 viewPos;

float dither = 0;
float ditherCutoff = .85;

vec3 CalcLight(vec3 lightDir, LightColor color, vec3 normal, vec3 viewDir, float attenuation, float intensity);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir);
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);  

void main()
{
	vec3 normal = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);

	vec3 Result = vec3(0.0);

	vec2 crushedCoord = floor(gl_FragCoord.xy / (12 * gl_FragCoord.w));
	float evenLine = mod(crushedCoord.x, 2);

	if(mod(crushedCoord.y, 2) == evenLine)
		dither = 1;

	//Result = CalcDirLight(dirLight, normal, viewDir);
	
	for(int i = 0; i < NR_POINT_LIGHTS; i++)
		Result += CalcPointLight(pointLights[i], normal, viewDir);

//	Result += CalcSpotLight(spotLight, normal, viewDir

	vec3 dV = vec3(dither);

	FragColor = vec4(Result + dV, 1);

}

vec3 CalcLight(vec3 lightDir, LightColor color, vec3 normal, vec3 viewDir, float attenuation, float intensity)
{
	float diff = max(dot(normal, lightDir), 0.0);
	float spec = pow(max(dot(viewDir, reflect(-lightDir, normal)), 0.0), material.shininess);

	dither = clamp(dither * (ditherCutoff - diff), 0, 1);

	vec3 texel = vec3(texture(material.diffuse, TexCoords));

	vec3 ambient = color.ambient * attenuation * texel;
	vec3 diffuse = color.diffuse * intensity * attenuation * (diff * texel);
//vec3 specular = color.specular * spec * vec3(texture(material.specular, TexCoords)) * intensity * attenuation;
	return vec3(ambient + diffuse);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir)
{
	float distance = length(light.position - FragPos);
	float attenuation = 1 / (light.constant + (light.linear * distance) + (light.quadratic * pow(distance, 2)));

	return CalcLight(normalize(light.position - FragPos), light.color, normal, viewDir, attenuation, 1.0);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir)
{
	float distance = length(light.position - FragPos);
	float attenuation = 1 / (light.constant + (light.linear * distance) + (light.quadratic * pow(distance, 2)));

	vec3 lightDir = normalize(light.position - FragPos);
	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutoff - light.outerCutoff;
	float intensity = clamp((theta - light.outerCutoff) / epsilon, 0.0, 1.0);

	return CalcLight(lightDir, light.color, normal, viewDir, attenuation, intensity);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
	return CalcLight(normalize(-light.direction), light.color, normal, viewDir, 1.0, 1.0);
}