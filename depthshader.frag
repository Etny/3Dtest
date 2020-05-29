#version 330 core

in vec4 FragPos;

uniform vec3 lightPos;
uniform float farPlane;

void main()
{
		float l = length(FragPos.xyz - lightPos);

		float depth = l / farPlane;

		gl_FragDepth = depth;
}