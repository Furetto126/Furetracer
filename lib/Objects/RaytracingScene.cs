using System.Linq;
using System.Security.Cryptography;

namespace Lib
{
    class RaytracingScene
    {
        public Dictionary<string, Object> objects = new Dictionary<string, Object>();

        public RaytracingScene(Dictionary<string, Object> objects)
        {
            this.objects = objects;
        }

        public void AddObjectInScene(Object obj)
        {
            if (ExistsInScene(obj.DisplayName))
            {
                Console.WriteLine("Could not add object since name " + obj.DisplayName + " was already taken by another one!"); return;
            }

            objects.Add(obj.DisplayName, obj);
        }

        public void RemoveObjectInScene(Object obj) 
        {
            if (!ExistsInScene(obj.DisplayName))
            {
                Console.WriteLine("Did not find a valid object with the specified name (" + obj.DisplayName + ") to remove!"); return;
            }

            objects.Remove(obj.DisplayName);
        }

        public bool ExistsInScene(Object obj)
        {
            if (objects.ContainsKey(obj.DisplayName))
            {
                return true;
            }

            return false;
        }

        public bool ExistsInScene(string name)
        {
            if (objects.ContainsKey(name))
            {
                return true;
            }

            return false;
        }

        public Object GetObjectByName(string name)
        {
            if (!ExistsInScene(name))
            {
                throw new Exception("Tried to get a non-existing object!");
            }

            return objects[name];
        }

        public Dictionary<string, Object> GetObjectsInScene() 
        {
            return objects;
        }

        public List<Object> GetObjectsInSceneList()
        {
            return objects.Values.OfType<Object>().ToList();
        }

        public List<Sphere> GetSpheresInScene()
        {
            return objects.Values.OfType<Sphere>().ToList();
        }

        public List<Model> GetModelsInScene()
        {
            return objects.Values.OfType<Model>().ToList();
        }
    }
}
