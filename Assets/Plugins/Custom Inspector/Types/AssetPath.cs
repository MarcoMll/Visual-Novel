using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector
{
    /// <summary>
    /// Only valid for FolderPath or FilePath! Used to fix overriding of other attributes
    /// </summary>
    public class AssetPathAttribute : PropertyAttribute { }

    /// <summary>
    /// Used for Folders
    /// </summary>
    [System.Serializable]
    public class FolderPath : AssetPath
    {
        public FolderPath(string defaultPath = "<invalid>")
        : base(defaultPath, typeof(Folder)) { }


        public void CreateAsset(Object asset, string assetName)
        {
#if UNITY_EDITOR
            //check
            if (asset is null)
                throw new System.NullReferenceException("asset is null");
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");
            //Check extension
            {
                int ind = assetName.LastIndexOf('.');
                if (ind == -1)
                    throw new ArgumentException($"{assetName} is missing an extension like '.asset'");
                else if(ind >= assetName.Length - 1)
                    throw new ArgumentException($"extension on {assetName} is empty");
            }

            if (HasPath())
                AssetDatabase.CreateAsset(asset, path + "/" + assetName);
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
#else
            throw new NotSupportedException("Creating assets in build is not allowed");
#endif
        }
        public void DeleteAsset(string assetName)
            => DeleteAsset(assetName, typeof(Object));
        public void DeleteAsset(string assetName, Type assetType)
        {
#if UNITY_EDITOR
            //check
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");

            if (HasPath())
            {
                string filePath = path + "/" + assetName;
                if (AssetDatabase.LoadAssetAtPath(filePath, assetType) != null)
                    AssetDatabase.DeleteAsset(filePath);
                else
                {
                    if(assetType == typeof(Object))
                        throw new ArgumentException($"Asset at {filePath} not found");
                    else
                        throw new ArgumentException($"Asset at {filePath} of type {assetType} not found");
                }
            }
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
#else
            throw new NotSupportedException("Deleting assets in build is not allowed");
#endif
        }
        public T LoadAsset<T>(string assetName) where T : Object
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");
            if (HasPath())
                return AssetDatabase.LoadAssetAtPath<T>(path + "/" + assetName);
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
        public Object LoadAsset(string assetName, Type type)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetName))
                throw new System.ArgumentException("assetName empty");
            if (HasPath())
                return AssetDatabase.LoadAssetAtPath(path + "/" + assetName, type);
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
    }

    /// <summary>
    /// Used for 'real' Objects (that are not Folders)
    /// <para>Use the AssetPathTypeAttribute to specify the objectType</para>
    /// </summary>
    [System.Serializable]
    public class FilePath : AssetPath
    {
        public FilePath(string defaultPath = "<invalid>")
        : base(defaultPath, typeof(Object)) { }
        public FilePath(string defaultPath, Type fileType)
        : base(defaultPath, fileType) { }
        public FilePath(Type fileType, string defaultPath = "<invalid>")
        : base(defaultPath, fileType) { }

        /// <summary>
        /// string behind the last slash of the path
        /// </summary>
        /// <exception cref="System.NullReferenceException">If no valid path was entered</exception>
        public string GetFileName()
        {
            if (HasPath())
            {
                Debug.Assert(path[^1] != '/' && path[^1] != '\\', "Path has to name");
                for (int i = path.Length - 2; i >= 0; i--)
                {
                    if(path[i] == '/' || path[i] == '\\')
                    {
                        return path[(i+1)..];
                    }
                }
                throw new ArgumentException("Path has to start with 'Assets/'");
            }
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
        }

        /// <summary>
        /// Replace current asset on path with new one
        /// </summary>
        /// <param name="asset">the new asset</param>
        /// <exception cref="System.NullReferenceException">If the path or asset is null</exception>
        public void OverrideAsset(Object asset)
        {
#if UNITY_EDITOR
            //check
            if (asset is null)
                throw new System.NullReferenceException("asset is null");

            if (HasPath())
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
#else
            throw new NotSupportedException("Creating assets in build is not allowed");
#endif
        }
        public T LoadAsset<T>() where T : Object
        {
#if UNITY_EDITOR
            if (HasPath())
                return (T)AssetDatabase.LoadAssetAtPath(path, fileType);
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
#else
            throw new NotSupportedException("Loading assets in build is not allowed");
#endif
        }
    }
    public class Folder { }
    /// <summary>
    /// Used for any Object whether its a folder or a file
    /// </summary>
    [System.Serializable]
    public abstract class AssetPath
    {
#if UNITY_EDITOR
        [MessageBox("You are overriding the default PropertyDrawer of AssetPath. Use the [AssetPath] attribute to fix overriding", MessageBoxType.Error)]
#endif

        [SerializeField]
        protected string path;

        protected readonly Type fileType = typeof(Object);

#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField, HideInInspector]
        private Object assetReference = null;
#pragma warning restore CS0414
#endif
        protected AssetPath(string defaultPath = "<invalid>")
        {
            this.path = defaultPath;
            this.fileType = typeof(Object);
        }
        protected AssetPath(string defaultPath, Type fileType)
        {
            this.path = defaultPath;
            this.fileType = fileType;
        }

        /// <summary> </summary>
        /// <exception cref="System.NullReferenceException">If no valid path was entered</exception>
        public string GetPath()
        {
            if (HasPath())
                return path;
            else
                throw new System.NullReferenceException("No valid path entered. Fill it in the Inspector!");
        }

        /// <summary>
        /// Change the current path
        /// </summary>
        /// <returns>True, if current path is valid</returns>
        public bool SetPath(string path)
        {
            this.path = path;
            return HasPath();
        }

        /// <summary>
        /// If a path is filled
        /// </summary>
        /// <returns></returns>
        public bool HasPath()
            => IsValidPath(path, fileType);


        public static bool IsValidPath(string path, Type fileType)
        {
#if UNITY_EDITOR
            if(fileType == typeof(Folder))
            {
                return AssetDatabase.IsValidFolder(path);
            }
            else
            {
                return AssetDatabase.LoadAssetAtPath<Object>(path) != null && !AssetDatabase.IsValidFolder(path);
            }
#else
            throw new NotSupportedException("AssetDatabase is not available in build");
#endif
        }

        public override string ToString()
        {
            if (HasPath())
            {
                return $"AssetPath({path})";
            }
            else
            {
                return "AssetPath(empty)";
            }
        }
    }
}
