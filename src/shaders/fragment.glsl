#version 450 core

out vec4 FragColor;
in vec2 TexCoord;

// Render uniforms
// ---------------
uniform float time;
uniform int framesFromRenderStart;
uniform vec2 resolution;
uniform float aspectRatio;

uniform bool progressiveRenderingActivated;
uniform sampler2D previousFrameTex;

// Raytracing settings
// -------------------
uniform int maxBounces;
uniform int numRaysPerPixel;
uniform float ambientWeight;

uniform bool raytracingActivated;

// Camera settings
// ---------------
uniform vec3 cameraPosition;
uniform vec3 cameraDirection;
uniform vec3 cameraRight;
uniform float fov;

// Matrices
// --------
uniform mat4 viewMatrixInverse;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 projectionMatrixInverse;

// Constants
// ---------
const float infinity = 340282346638528859811704183484516925440.0;
float PI = 3.141592653589793238462643;

struct Ray {
    vec3 origin;
    vec3 direction;
};

struct Material {
    vec3 color;
    float smoothness;

    vec3 emissionColor;
    float emissionStrength;

    vec3 pad0;
    float glossiness;
};

struct Sphere {
    vec3 position;
    float radius;

    Material material;
};

struct Triangle {
    vec3 v0;
    float pad0;

    vec3 v1;
    float pad1;

    vec3 v2;
    float pad2;

    Material material;
};

struct Hit {
    bool hitRegistered;
    float hitDistance;
    vec3 hitPoint;
    vec3 hitNormal;

    Material material;
};

// scene
    // spheres
    // -------
    uniform int numSpheres;

    layout(std430, binding = 0) buffer SphereBuffer {
        Sphere spheres[];
    };

    uniform int numTriangles;

    layout(std430, binding = 1) buffer TriangleBuffer {
        Triangle triangles[];
    };
 
// From https://stackoverflow.com/a/4275343/
float rand(inout uint seed){
    seed = seed * 747796405 + 2891336453;
    uint result = ((seed >> ((seed >> 28) + 4)) ^ seed) * 277803737;
    result = (result >> 22) ^ result;
    return result / 4294967295.0;
}

float randGaussian(inout uint seed) {
    return sqrt(-2.0 * log(rand(seed))) * cos(2.0 * PI * rand(seed));
}

vec3 randVector(inout uint seed) {
    float x = randGaussian(seed);
    float y = randGaussian(seed);
    float z = randGaussian(seed);

    return normalize(vec3(x, y, z));
} 

/*vec2 randPointInCircle(uint seed) {
    float angle = rand(seed) * 2.0 * PI;
    return vec2(cos(angle), sin(angle)) * sqrt(rand(seed));
}*/

vec3 getEnvironmentLight(Ray ray){
    vec3 skyColorHorizon = vec3(0.9, 1.0, 1.0);
    vec3 skyColorZenith = vec3(0.37, 0.47, 0.61);
    vec3 groundColor = vec3(0.41, 0.39, 0.37);

    float skyGradientT = pow(smoothstep(0.0, 0.7, ray.direction.y), 0.35);
    vec3 skyGradient = mix(skyColorHorizon, skyColorZenith, skyGradientT);

    float groundToSkyT = smoothstep(-0.01, 0.0, ray.direction.y);
    return mix(groundColor, skyGradient, groundToSkyT);
}

// Adapted from https://youtu.be/Qz0KTGYJtUk, some of the things are inspired from that video
Hit sphereIntersection(Ray ray, Sphere sphere) {
    Hit sphereHit;
    sphereHit.hitRegistered = false;

    vec3 rayOrigin = ray.origin - sphere.position;

    float a = dot(ray.direction, ray.direction);
    float b = 2.0 * dot(rayOrigin, ray.direction);
    float c = dot(rayOrigin, rayOrigin) - pow(sphere.radius, 2.0); 
    float discriminant = b * b - 4.0 * a * c;

    if (discriminant >= 0.0) {
        float dst = (-b - sqrt(discriminant)) / (2.0 * a);
        if (dst >= 0.0) {
            sphereHit.hitRegistered = true;
            sphereHit.hitDistance = dst;
            sphereHit.hitPoint = ray.origin + ray.direction * dst;
            sphereHit.hitNormal = normalize(sphereHit.hitPoint - sphere.position);
        }
    }
    return sphereHit;
}

Hit triangleIntersection(Ray ray, Triangle triangle) {
    Hit triangleHit;
    triangleHit.hitRegistered = false;

    const float EPSILON = 0.00001;

    vec3 v0 = triangle.v0;
    vec3 v1 = triangle.v1;
    vec3 v2 = triangle.v2;

    vec3 edge1 = v1 - v0;
    vec3 edge2 = v2 - v0;

    vec3 pvec = cross(ray.direction, edge2);
    float det = dot(edge1, pvec);

    if (abs(det) < EPSILON) {
        triangleHit.hitRegistered = false;
        triangleHit.material.color = vec3(0.0);
        return triangleHit;
    }

    float invDet = 1.0 / det;

    vec3 tvec = ray.origin - v0;
    float u = dot(tvec, pvec) * invDet;

    if (u < 0.0 || u > 1.0) {
        triangleHit.hitRegistered = false;
        triangleHit.material.color = vec3(0.0);
        return triangleHit;
    }

    vec3 qvec = cross(tvec, edge1);
    float v = dot(ray.direction, qvec) * invDet;

    if (v < 0.0 || (u + v) > 1.0) {
        triangleHit.hitRegistered = false;
        triangleHit.material.color = vec3(0.0);
        return triangleHit;
    }

    float t = dot(edge2, qvec) * invDet;
    
    triangleHit.hitRegistered = true;
    triangleHit.hitDistance = t;
    triangleHit.hitPoint = ray.origin + ray.direction * t;
    triangleHit.hitNormal = normalize(cross(edge1, edge2));

    return triangleHit;
}

Hit objectsDepthTest(Ray ray) {
    Hit closestHit;
    closestHit.hitDistance = infinity;
    closestHit.hitRegistered = false;

    for (int i = 0; i < numSpheres; i++) {
        Hit sphereHit = sphereIntersection(ray, spheres[i]);
        if (sphereHit.hitRegistered && sphereHit.hitDistance < closestHit.hitDistance) {
            closestHit = sphereHit;
            closestHit.material = spheres[i].material;
        } 
    }

    for (int i = 0; i < numTriangles; i++) {
        Hit triangleHit = triangleIntersection(ray, triangles[i]);
        if (triangleHit.hitRegistered && triangleHit.hitDistance < closestHit.hitDistance) {
            closestHit = triangleHit;
            closestHit.material = triangles[i].material;
        }
    }
    return closestHit;
}

vec3 traceRay(Ray ray, inout uint seed) {
    vec3 rayColor = vec3(1.0);
    vec3 final = vec3(0.0);

    for (int i = 0; i < maxBounces; i++) {
        Hit objectHit = objectsDepthTest(ray);

        if (!objectHit.hitRegistered){
            final += getEnvironmentLight(ray) * rayColor * ambientWeight;
            break;
        }

        Material hitMaterial = objectHit.material;

        vec3 diffuse = normalize(objectHit.hitNormal + randVector(seed));
        vec3 specular = reflect(ray.direction, objectHit.hitNormal);

        ray.origin = objectHit.hitPoint;
        ray.direction = mix(diffuse, specular, hitMaterial.smoothness * (hitMaterial.glossiness >= rand(seed) ? 1.0 : 0.0));

        vec3 objectOutLight = hitMaterial.emissionColor * hitMaterial.emissionStrength;
        final += objectOutLight * rayColor;
        rayColor *= hitMaterial.color;
    }
    return final;
}

vec4 pixelResult() {
    //this is some sort of magic seed idk but it works sooo...
    //if u change u will get cool effects, but not raytracing (for example if u base this on only time and pixel.y)
    uint seed = uint(((TexCoord.y * resolution.y) * resolution.x + (TexCoord.x * resolution.x)) + time * 76793); 

    float tanFOV = 1.0 / tan(fov * 0.5 * PI / 180.0);
    vec2 pixelNDC = (2.0 * gl_FragCoord.xy - resolution) / resolution.y;

    vec3 rayTarget = (viewMatrixInverse * vec4(pixelNDC, tanFOV, 1.0)).xyz;
    vec3 rayDirection = normalize(rayTarget - cameraPosition);

    Ray ray;
    ray.origin = cameraPosition;
    ray.direction = rayDirection;

    vec4 final;
    
    if (raytracingActivated) {
        vec3 totalInLight = vec3(0.0);

        for (int i = 0; i < numRaysPerPixel; i++) {
            totalInLight += traceRay(ray, seed);

            ray.origin = cameraPosition;
            ray.direction = rayDirection;
        }

        final = vec4(totalInLight / float(numRaysPerPixel), 1.0);
    }else {
        Hit objectHit = objectsDepthTest(ray);
        final = objectHit.hitRegistered ? vec4(objectHit.material.color, 1.0) : vec4(getEnvironmentLight(ray), 1.0);
    } 
    return final;
}

void main() {
    vec4 currentResult = pixelResult();

    if (progressiveRenderingActivated) {
        float weigth = 1.0 / (framesFromRenderStart + 1.0);
        FragColor = texture(previousFrameTex, TexCoord) * (1.0 - weigth) + currentResult * weigth;
    }else {
        FragColor = currentResult;
    }    
}