using UnityEngine;

namespace VertigoDemo.UI
{
    public static class TransformExtensions
    {
        #region Public API

        public static Transform FindDeep(this Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name)
                {
                    return child;
                }
                Transform found = child.FindDeep(name);
                if (found)
                {
                    return found;
                }
            }
            return null;
        }

        public static T FindDeepComponent<T>(this Transform root, string name) where T : Component
        {
            Transform child = root.FindDeep(name);
            if (!child)
            {
                Debug.LogWarning($"{root.name}: child '{name}' not found.", root);
                return null;
            }
            return child.GetComponent<T>();
        }

        #endregion
    }
}