#version 430 

in vec3 position;
in vec2 textureCoord;

out vec2 pass_textureCoords;

void main()
{
	gl_Position = vec4(position, 1.0);
	pass_textureCoords = textureCoord;
}