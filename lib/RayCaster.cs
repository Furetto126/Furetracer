using GlmNet;

namespace Lib
{
    class RayCaster
    {
        const float PI = 3.141592653589793238462643f;

        struct Hit
        {
            public bool hitRegistered;
            public float hitDistance;

            public Hit (bool hitRegistered, float hitDistance)
            {
                this.hitRegistered = hitRegistered;
                this.hitDistance = hitDistance;
            }
        }

        private static Hit SphereIntersection(vec3 rayOrigin, vec3 rayDirection, vec3 spherePosition, float sphereRadius)
        {
            Hit sphereHit = new Hit(false, 0.0f);

            vec3 rayOriginOffsetted = rayOrigin - spherePosition;

            float a = glm.dot(rayDirection, rayDirection);
            float b = 2.0f * glm.dot(rayOriginOffsetted, rayDirection);
            float c = glm.dot(rayOriginOffsetted, rayOriginOffsetted) - (sphereRadius * sphereRadius);
            float discriminant = b * b - 4.0f * a * c;

            if (discriminant >= 0.0)
            {
                float dst = (-b - MathF.Sqrt(discriminant)) / (2.0f * a);
                if (dst >= 0.0)
                {
                    sphereHit.hitRegistered = true;
                    sphereHit.hitDistance = dst;
                }
            }
            return sphereHit;
        }

        public static int CheckSphereIntersectionCoord(vec2 cursorCoord, vec3 rayOrigin, Shader shader, vec2 resolution, float fov, mat4 viewMatrixInverse)
        {
            // Initializes a ray just like in the shader
            float tanFOV = 1.0f / MathF.Tan(fov * 0.5f * PI / 180.0f);
            vec2 pixelNDC = (2.0f * cursorCoord - resolution) / resolution.y;

            Console.WriteLine(pixelNDC);

            vec3 rayTarget = new vec3(viewMatrixInverse * new vec4(pixelNDC.x, pixelNDC.y, tanFOV, 1.0f));
            vec3 rayDirection = glm.normalize(rayTarget - rayOrigin);

            vec3[] spheresPosition = shader.GetSpheresList().Select(s => s.position).ToArray();
            float[] spheresRadius = shader.GetSpheresList().Select(s => s.radius).ToArray();

            Hit closestHit = new Hit(false, float.MaxValue);
            int sphereIndex = int.MinValue;

            for (int i = 0; i < spheresPosition.Length; i++)
            {
                Hit sphereHit = SphereIntersection(rayOrigin, rayDirection, spheresPosition[i], spheresRadius[i]);
                if (sphereHit.hitRegistered && sphereHit.hitDistance < closestHit.hitDistance)
                {
                    closestHit = sphereHit;
                    sphereIndex = i;
                }
            }

            return sphereIndex;
        }
    }
}
