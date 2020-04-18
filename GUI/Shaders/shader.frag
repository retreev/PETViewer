#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture_array;

void main()
{
    vec4 texColor = texture(texture_array, TexCoords);
//    if (texColor.a < 0.1) { // discard fully transparent fragments
//        discard;
//    }
    FragColor = texColor;
}
