using GlmNet;

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

        public Sphere()
        {
            position = new vec3(0.0f);
            radius = 1.0f;
            color = new vec3(0.7f);
            emissionColor = new vec3(0.0f);
            emissionStrength = 0.0f;
            smoothness = 1.0f;
            glossiness = 1.0f;
        }
    }
}
