#version 330 core
out vec4 FragColor;

in vec3 TexCoords;

uniform sampler2DArray texture_array;

void main()
{
    vec4 texColor = texture(texture_array, TexCoords);
    if (texColor.a != 1) {
        discard;
    }
    FragColor = texColor;
}
