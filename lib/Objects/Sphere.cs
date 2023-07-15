using GlmNet;

namespace Lib 
{
    class Sphere : Object
    {
        public vec3 position;
        public float radius;
        public Material material;

        public override string DisplayName { get; set; }

        public Sphere(string name, vec3 position, float radius, Material material) 
        { 
            DisplayName = name;
            this.position = position;   
            this.radius = radius;
            this.material = material;
        }

        public Sphere(string name, vec3 position, float radius, vec3 color, vec3 emissionColor, float emissionStrength, float smoothness, float glossiness)
        {
            DisplayName = name;
            this.position = position;
            this.radius = radius;
            material = new Material(
                color,
                emissionColor,
                emissionStrength,
                smoothness,
                glossiness
            );
        }

        public Sphere(Sphere sphere)
        {
            DisplayName = sphere.DisplayName;
            position = sphere.position;
            radius = sphere.radius;
            material = sphere.material;
        }

        public Sphere(string name)
        {
            DisplayName = name;
            position = new vec3(0.0f);
            radius = 1.0f;
            material = new Material(new vec3(1.0f), new vec3(1.0f), 1.0f, 0.5f, 0.5f);
        }
    }
}
