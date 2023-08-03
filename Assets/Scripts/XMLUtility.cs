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

/// <summary>
/// Generic collection for everything that's just a collection of strings
/// </summary>
[XmlRoot("Main")]
public class IdCollection
{
    [XmlArray("ids"), XmlArrayItem("id")]
    public string[] allIds;

    /// <summary>
    /// Loads the string array, and converts it to a dictionary.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Dictionary<int, string> LoadArrayAsDict(string path)
    {
        if (path == "")
        {
            Debug.LogWarning("empty path");
            // return an empty dict
            return new Dictionary<int, string>();
        }

        TextAsset toDeserialize = (TextAsset) Resources.Load(path);

        Dictionary<int, string> returnDict = new Dictionary<int, string>();
        IdCollection ids = XMLUtility.LoadFromText<IdCollection>(toDeserialize);

        Debug.Log("success");

        for(int i = 0; i < ids.allIds.Length; i++)
        {
            // add to a dictionary
            returnDict.Add(i, ids.allIds[i]);
        }

        return returnDict;
    }
}
