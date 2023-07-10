using GlmNet;

namespace Lib
{
    class Material
    {
        public vec3 color;
        public vec3 emissionColor;
        public float emissionStrength;
        public float smoothness;
        public float glossiness;

        public Material(vec3 color, vec3 emissionColor, float emissionStrength, float smoothness, float glossiness) {
            this.color = color;
            this.emissionColor = emissionColor;
            this.emissionStrength = emissionStrength;
            this.smoothness = smoothness;
            this.glossiness = glossiness;
        }
    }
}
