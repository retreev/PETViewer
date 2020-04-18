#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_diffuse2;
uniform sampler2D texture_diffuse3;
uniform sampler2D texture_specular1;

void main()
{
    vec4 texColor = texture(texture_diffuse3, TexCoords);
//    if (texColor.a < 0.1) { // discard fully transparent fragments
//        discard;
//    }
    FragColor = texColor;
}
