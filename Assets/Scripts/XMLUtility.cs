using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class XMLUtility
{
    /// <summary>
    /// Loads an XML file given a path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T LoadFromPath<T>(string path)
    {
        var serializer = new XmlSerializer(typeof(T));

        using (var stream = new FileStream(Path.Combine(Application.dataPath, path), FileMode.Open))
        {
            // not sure if this works
            return (T) serializer.Deserialize(stream);
        }
    }

    /// <summary>
    /// Loads an XML file given an asset.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="asset"></param>
    /// <returns></returns>
    public static T LoadFromText<T>(TextAsset asset)
    {
        var serializer = new XmlSerializer(typeof(T));

        using (var stream = new System.IO.StringReader(asset.text))
        {
            // not sure if this works
            return (T)serializer.Deserialize(stream);
        }
    }

}
