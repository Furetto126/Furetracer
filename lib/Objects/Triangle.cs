using GlmNet;

namespace Lib
{
    class Triangle
    {
        public vec3 v0, v1, v2;
        public Material material;

        public Triangle(vec3 v0, vec3 v1, vec3 v2, vec3 color, vec3 emissionColor, float emissionStrength, float smoothness, float glossiness)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            Material material = new Material(
                color,
                emissionColor,
                emissionStrength,
                smoothness,
                glossiness
            );
        }

        public Triangle(vec3 v0, vec3 v1, vec3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }
    }
}
