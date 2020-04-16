#version 330 core

uniform sampler2D texture0;

out vec4 FragColor;

in vec3 FragPos;
in vec2 TexCoords;
in vec3 Normal;

void main()
{
    FragColor = vec4(vec3(texture(texture0, TexCoords)), 0.5);
}
