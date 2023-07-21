using GlmNet;

namespace Lib
{
    class Model : Object
    {
        public override string DisplayName { get; set; }
        public List<Triangle> triangles;

        public float size { get; private set; } = 1.0f;
        public vec3 position { get; private set; } = new vec3(0.0f);

        private vec3 previousPosition = new vec3(0.0f);
        private List<Triangle> originalTriangles = new List<Triangle>();

        public Model(string name, List<Triangle> triangles) { 
            DisplayName = name;
            this.triangles = triangles;
            originalTriangles = triangles;
        }

        public Model(Model model) {
            DisplayName = model.DisplayName;
            triangles = model.triangles;
            originalTriangles = triangles;
        }

        public void SetSize(float newSize)
        {
            size = newSize;

        }

        public void SetPosition(vec3 newPosition)
        {
            previousPosition = position;
            position = newPosition;

            newPosition -= previousPosition;

            foreach (Triangle triangle in triangles)
            {
                triangle.v0 += newPosition;
                triangle.v1 += newPosition;
                triangle.v2 += newPosition;
            }
        }
    }
}
