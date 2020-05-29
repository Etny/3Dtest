#version 330 core
layout (location = 0) in vec3 aPos;

uniform mat4 lightSpace;
uniform mat4 model;

out vec4 FragPos;

void main()
{
	gl_Position = lightSpace * model * vec4(aPos, 1);
	//gl_Position = model * vec4(aPos, 1);
	FragPos = model * vec4(aPos, 1.0);
}