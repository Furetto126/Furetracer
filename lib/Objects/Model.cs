namespace Lib
{
    class Model : Object
    {
        public List<Triangle> Triangles { get; set; }
        public override string DisplayName { get; protected set; }

        public Model(string name, List<Triangle> triangles) { 
            DisplayName = name;
            Triangles = triangles;
        }
    }
}
