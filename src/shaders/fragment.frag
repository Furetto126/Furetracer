#version 450 core

out vec4 FragColor;
in vec2 TexCoord;

// matrices
uniform mat4 viewMatrixInverse;

// camera
uniform vec3 cameraPosition;
uniform vec2 resolution;

// raytracing settings
uniform int maxBounces;
uniform int numRaysPerPixel;
uniform float ambientWeight;

// other
uniform float time;

struct Ray {
    vec3 origin;
    vec3 direction;
};

struct RayTracingMaterial {
    vec3 color;
    float smoothness;

    vec3 emissionColor;
    float emissionStrength;

    vec3 _pad0;
    float glossiness;
};

struct Sphere {
    vec3 position;
    float radius;

    RayTracingMaterial material;
};

struct HitInfo {
    bool didHit;
    float dst;
    vec3 hitPoint;
    vec3 normal;
    RayTracingMaterial material;
};


// scene
    // spheres
    // -------
    uniform int numSpheres;

    layout(std430, binding = 0) buffer SphereBuffer {
        Sphere spheres[];
    };


// Gold Noise ©2015 dcerisano@standard3d.com
const float infinity = 340282346638528859811704183484516925440.0;
float PHI = 1.61803398874989484820459;   
float PI = 3.141592653589793238462643;

float goldNoise(inout float seed) {
    seed = fract(tan(distance(TexCoord * resolution * PHI, TexCoord * resolution) * seed) * resolution.x * time);
    return seed;
}
vec2 randomCirclePoint(inout float seed) {
    float angle = goldNoise(seed) * 2.0 * PI;
    vec2 pointOnCircle = vec2(cos(angle), sin(angle));
    return pointOnCircle * sqrt(goldNoise(seed));
}
float randomGaussianDistribution(inout float seed) {
        float theta = 2.0 * PI * goldNoise(seed);
        float rho = sqrt(-2.0 * log(goldNoise(seed)));
        return rho * cos(theta);
}
vec3 randomDirection(inout float seed){
    float x = randomGaussianDistribution(seed);
    float y = randomGaussianDistribution(seed);
    float z = randomGaussianDistribution(seed);
    return normalize(vec3(x, y, z));
}
vec3 RandomHemisphereDirection(vec3 normal, inout float seed){
    vec3 dir = randomDirection(seed);
    return dir * sign(dot(normal, dir));
}
vec3 getEnvironmentLight(Ray ray){
    vec3 skyColorHorizon = vec3(0.9, 1.0, 1.0);
    vec3 skyColorZenith = vec3(0.4, 0.6, 1.0);
    vec3 groundColor = vec3(0.4, 0.4, 0.4);

    float skyGradientT = pow(smoothstep(0.0, 0.4, ray.direction.y), 0.35);
    vec3 skyGradient = mix(skyColorHorizon, skyColorZenith, skyGradientT);

    float groundToSkyT = smoothstep(-0.01, 0.0, ray.direction.y);
    return mix(groundColor, skyGradient, groundToSkyT);
}

HitInfo raySphere(Ray ray, vec3 sphereCenter, float sphereRadius) {
    HitInfo hitInfo;
    vec3 offsetRayOrigin = ray.origin - sphereCenter;

    float a = dot(ray.direction, ray.direction);
    float b = 2.0 * dot(offsetRayOrigin, ray.direction);
    float c = dot(offsetRayOrigin, offsetRayOrigin) - pow(sphereRadius, 2.0);
    float discriminant = b * b - 4.0 * a * c;

    if (discriminant >= 0.0) {
        float dst = (-b - sqrt(discriminant)) / (2.0 * a);
        if (dst >= 0.0) {
            hitInfo.didHit = true;
            hitInfo.dst = dst;
            hitInfo.hitPoint = ray.origin + ray.direction * dst;
            hitInfo.normal = normalize(hitInfo.hitPoint - sphereCenter);
        }
    }
    return hitInfo;
}

HitInfo calculateRayCollision(Ray ray) {
    HitInfo closestHit;
    closestHit.didHit = false;
    closestHit.dst = infinity;

    for (int i = 0; i < numSpheres; i++) {
        HitInfo hitInfo = raySphere(ray, spheres[i].position, spheres[i].radius); 
        if (hitInfo.didHit && hitInfo.dst < closestHit.dst) {
            RayTracingMaterial sphereMaterial;
            sphereMaterial.color = spheres[i].material.color;
            sphereMaterial.emissionColor = spheres[i].material.emissionColor;
            sphereMaterial.emissionStrength = spheres[i].material.emissionStrength;
            sphereMaterial.smoothness = spheres[i].material.smoothness;
            sphereMaterial.glossiness = spheres[i].material.glossiness;

            closestHit = hitInfo;
            closestHit.material = sphereMaterial; 
        } 
    }
    return closestHit;
}

vec3 traceRay(Ray ray, inout float seed) {
    vec3 rayColor = vec3(1.0);
    vec3 incomingLight = vec3(0.0);

    for (int i = 0; i <= maxBounces; i++) {
        HitInfo hitInfo = calculateRayCollision(ray);

        if (hitInfo.didHit) {
            RayTracingMaterial material = hitInfo.material;
            ray.origin = hitInfo.hitPoint;

            vec3 diffuseReflection = normalize(hitInfo.normal + randomDirection(seed));
            vec3 specularReflection = reflect(ray.direction, hitInfo.normal);
            float isGlossy = material.glossiness >= goldNoise(seed) ? 1.0 : 0.0;

            ray.direction = mix(diffuseReflection, specularReflection, material.smoothness * isGlossy);

            vec3 emittedLight = material.emissionColor * material.emissionStrength;
            incomingLight += emittedLight * rayColor;
            //incomingLight = hitInfo.normal;

            rayColor *= material.color; 
        }else {
            incomingLight += getEnvironmentLight(ray) * rayColor * ambientWeight;
            break;
        }
    }
    return incomingLight;
}

vec4 pixelResult() {
    float seed = fract(time);

    vec3 rayTarget = vec3(TexCoord * 2.0 - 1.0, 1.0);
    rayTarget = (viewMatrixInverse * vec4(rayTarget, 1.0)).xyz;
    vec3 rayDirection = normalize(rayTarget - cameraPosition);

    vec3 totalIncomingLight = vec3(0.0);

    for (int rayIndex = 0; rayIndex < numRaysPerPixel; rayIndex++) {
        Ray ray;
        ray.origin = cameraPosition;
        ray.direction = rayDirection;

        totalIncomingLight += traceRay(ray, seed);
    } 
    return vec4(totalIncomingLight / float(numRaysPerPixel), 1.0);
}

void main() {
    FragColor = pixelResult();
}
