using GlmNet;

namespace Lib
{
    class Model : Object
    {
        public List<Triangle> triangles;
        public float size = 1.0f;
        public override string DisplayName { get; set; }

        public Model(string name, List<Triangle> triangles) { 
            DisplayName = name;
            this.triangles = triangles;
        }

        public Model(Model model) {
            DisplayName = model.DisplayName;
            triangles = model.triangles;
        }
    }
}
