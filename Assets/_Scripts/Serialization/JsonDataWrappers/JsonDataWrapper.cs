#region Serialization Helper Classes

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
    public class JsonDataWrapper
    {
        [SerializeField] private string key;
        [SerializeField] private SerializationDataType dataType;
        [SerializeField] private string value;

        public string Key => key;
        public SerializationDataType DataType => dataType;
        public string Value => value;

        public JsonDataWrapper(string key, SerializationDataType dataType, string value)
        {
            this.key = key;
            this.dataType = dataType;
            this.value = value;
        }
    }

    [Serializable]
    public class JsonDataObjectWrapper
    {
        [SerializeField] protected string uniqueId;
        [SerializeField] protected JsonDataWrapper[] data;

        public string UniqueId => uniqueId;
        public IReadOnlyList<JsonDataWrapper> Data => data;

        public JsonDataObjectWrapper(string uniqueId, IEnumerable<JsonDataWrapper> data)
        {
            this.uniqueId = uniqueId;

            // Convert the list of JsonDataWrappers to a JSON string
            this.data = data.ToArray();
        }

        // public void Add<TOtherType>(JsonDataWrapper<TOtherType> jsonDataWrapper) =>
        //     jsonDataWrappers.Add((JsonDataWrapper<object>)jsonDataWrapper);
    }

    [Serializable]
    public class SceneJsonData
    {
        [SerializeField] private string sceneName;
        [SerializeField] private JsonDataObjectWrapper[] data;

        public string SceneName => sceneName;
        public IReadOnlyList<JsonDataObjectWrapper> Data => data;

        public SceneJsonData(string sceneName, IEnumerable<JsonDataObjectWrapper> jsonDataObjectWrappers)
        {
            this.sceneName = sceneName;
            data = jsonDataObjectWrappers.ToArray();
        }
    }

    [Serializable]
    public class SceneJsonDataCollection
    {
        [SerializeField] private SceneJsonData[] data;

        public IReadOnlyList<SceneJsonData> Data => data;

        public SceneJsonDataCollection(IEnumerable<SceneJsonData> jsonDataObjects)
        {
            data = jsonDataObjects.ToArray();
        }
    }

    #endregion