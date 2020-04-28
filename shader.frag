#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;
uniform sampler2D texture2;

void main()
{
	FragColor = mix(texture(texture1, TexCoords), texture(texture2, TexCoords), 0.5);
	//FragColor = vec4(vec3(gl_FragCoord.z), 1.0);
}