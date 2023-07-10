# Furetracer
This is a Raytracer written in C# and GLSL, using OpenGL.
All the important ray tracing calculations are happening in the shader code running on the GPU, for decent speed even on not so powerful hardware.
## How to use
It's pretty straightforward: clone, open the project on visual studio and run it!  
While inside of the program you can move the camera using the scroll wheel (both by pressing it and scolling) and using the right mouse button.  
By clicking "V" you can spawn a sphere at your location, with "Ctrl + S" you save your scene and with "Ctrl + L" you load it.  
Click "Esc" to quit the program.  

TODO:
Get triangles from 3D models and send it to the shader (DONE). 
Render polygons from 3D models (WIP).  
Make a proper rendering system (and being able to make animations/videos).  
Get a custom console with commands to have a separate "only keyboard" mode (WIP).  
Polish GUI (WIP).  
Implement an acceleration structure to optimize ray intersection search.  

## Examples

![A simple scene rendered in 3 seconds on my mid-tier PC](https://cdn.discordapp.com/attachments/900407826755772437/1123351978890756228/image.png)
