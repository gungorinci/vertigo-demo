using UnityEngine;

namespace VertigoDemo.UI
{
    public static class TransformExtensions
    {
        public static Transform FindDeep(this Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name) return child;
                Transform found = child.FindDeep(name);
                if (found != null) return found;
            }
            return null;
        }
    }
}