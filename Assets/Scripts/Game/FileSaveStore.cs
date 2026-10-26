using System.IO;
using UnityEngine;
using VertigoDemo.Core;

namespace VertigoDemo.Game
{
    public class FileSaveStore : ISaveStore
    {
        private const string FILE_NAME = "save.json";

        private readonly string _filePath;

        public FileSaveStore()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        #region ISaveStore

        public string Load()
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(_filePath);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Could not read the save at {_filePath}: {exception.Message}");
                return null;
            }
        }

        public void Save(string json)
        {
            try
            {
                File.WriteAllText(_filePath, json);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Could not write the save at {_filePath}: {exception.Message}");
            }
        }

        #endregion
    }
}
