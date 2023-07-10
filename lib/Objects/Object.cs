namespace Lib
{
    abstract class Object
    {
        public abstract string DisplayName { get; protected set; }

        public static List<Object> SceneObjects = new List<Object>();
    }
}
