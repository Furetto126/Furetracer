using GlmNet;
using System.Runtime.InteropServices;

namespace Lib 
{
    class Sphere
    {
        public vec3 position;
        public float radius;
        public vec3 color;
        public vec3 emissionColor;
        public float emissionStrength;
        public float smoothness;
        public float glossiness;

        public Sphere(vec3 position, float radius, vec3 color, vec3 emissionColor, float emissionStrength, float smoothness, float glossiness) 
        { 
            this.position = position;   
            this.radius = radius;
            this.color = color;
            this.emissionColor = emissionColor;
            this.emissionStrength = emissionStrength;
            this.smoothness = smoothness;
            this.glossiness = glossiness;
        }
    }
}
