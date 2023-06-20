using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Linq;

public class Editor
{

    //[UnityEditor.MenuItem("Tools/ScriptableObjects/Generate NPC Data from SQLite")]
    /*static void GenerateNPCData()
    {
        string dialogueURI = "URI=file:Assets/DialogueDB.db";
        string destPath = "Assets/Scripts/ScriptableObjects/NPCData";

        // connect to speakerDb in sqlite
        IDbConnection dbConnection = new SqliteConnection(dialogueURI);
        dbConnection.Open();

        // command, select speaker archetype and isFiller rows
        IDbCommand readValues = dbConnection.CreateCommand();
        readValues.CommandText = "SELECT speakerArchetype, isFiller FROM Speakers";

        // start data reader
        IDataReader reader = readValues.ExecuteReader();

        // read
        // = index 0 -> id, index 1 -> tag
        while (reader.Read())
        {
            // we create a scriptableObject NPC data from each unique NPC archetype
            // filler characters that have more than 1 occurring character (guards, etc) will be a single scriptableobj as well
            string archetype = reader.GetString(0);
            int isFiller = reader.GetInt32(1);

            if (archetype == "player")
            {
                continue;
            }

            // create an npc from each row
            NPCData newNpc = ScriptableObject.CreateInstance<NPCData>();

            newNpc.speakerArchetype = archetype;
            newNpc.displayNameDefault = archetype.Split('_')[1].ToUpper().ToString();

            // is npc filler char?
            if (isFiller == 0)
            {
                // npc not a filler character; thus the characterarchetype is unique and the npc id is archetype
                newNpc.isFillerCharacter = false;
                newNpc.npcId = archetype;
            }
            else
            {
                // if npc is filler character, filler character = true and npc id will be empty to be generated when the filler npc spawns
                newNpc.isFillerCharacter = true;
                newNpc.npcId = "";
            }

            // asset database
            // get asset with the following filename:
            // npcSprite = tag_sprite           ex. virgil_sprite
            // npcPortrait = tag_portrait       ex. virgil_portrait
            string spriteFname = archetype + "_sprite.png";             // entire sprite
            string portraitFname = archetype + "_portrait.png";

            Debug.Log("sprite fname: " + spriteFname);
            Debug.Log("portrait fname: " + portraitFname);

            // load all assets with the file name (including the sliced sprites)
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/World/" + spriteFname);

            foreach(Object asset in allAssets)
            {
                if(asset is Sprite sprite)
                {
                    newNpc.npcSprite = sprite;
                    // break
                    break;
                }
            }
            
            newNpc.npcPortrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/Portrait/" + portraitFname);

            // create new scriptableobject if its a valid folder
            if (AssetDatabase.IsValidFolder(destPath))
            {
                // create file name.
                string fname = destPath + "/NPC_" + newNpc.speakerArchetype + ".asset";

                var asset = AssetDatabase.LoadAssetAtPath<NPCData>(fname);

                // if the file already exists...
                if (asset != null)
                {
                    Debug.Log("existing asset...");
                    // replace all values of the file
                    asset.npcId = newNpc.npcId;
                    asset.npcPortrait = newNpc.npcPortrait;
                    asset.npcSprite = newNpc.npcSprite;
                    asset.isFillerCharacter = newNpc.isFillerCharacter;
                    asset.speakerArchetype = newNpc.speakerArchetype;
                    asset.displayNameDefault = newNpc.displayNameDefault;
                }
                else
                {
                    // create a new asset if the file exists

                    // filename = NPC_speakerarchetype.asset
                    AssetDatabase.CreateAsset(newNpc, fname);
                }
            }
            else
            {
                string[] sepPath = destPath.Split('/');
                // new folder
                string newFolder = sepPath[sepPath.Length - 1];
                // join path
                string path = string.Join("/", sepPath);
                AssetDatabase.CreateFolder(path, newFolder);

                Debug.Log("Created folder " + newFolder + " at path " + path);
            }

        }
        
    }*/

    /// <summary>
    /// Reads a CSV file given a path, and returns a list of dict where the elements of the list is the 
    /// npc itself, and element of the dictionary represent the columns of the csv.
    /// </summary>
    /// <param name="path">Path of file in assetDB</param>
    /// <returns>List of dictionaries where each element is a row, the dictionary is represented by
    /// COL_NAME for key, and COL_VALUE for value</returns>
    static List<Dictionary<string, string>> ReadCSVFile(string path)
    {
        // check if path is a .csv file
        if(!(path.Split('.')[1] == "csv"))
        {
            Debug.LogError("Not a CSV file");
            return null;
        }

        char[] delimiters = { '\n', '\r' };
        // list of dictionary where key => colName, and value => item or value
        List<Dictionary<string, string>> outData = new List<Dictionary<string, string>>();

        TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

        if (text == null)
        {
            Debug.LogError("asset not found");
            return null;
        }
        
        List<string[]> allData = new List<string[]>();

        Debug.Log(text.text.Split(delimiters));
        foreach(string line in text.text.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries))
        {
            // split each line by comma
            allData.Add(line.Split(','));
        }

        // get the first row => the column names
        string[] colNames = allData[0];
        
        // iterate through the remaining rows
        for(int row = 1; row < allData.Count; row++)
        {
            Dictionary<string, string> current = new Dictionary<string, string>();
            for(int col = 0; col < colNames.Length; col++)
            {
                current.Add(colNames[col], allData[row][col]);
            }

            // add current dictionary to output
            outData.Add(current);
        }

        TestPrint(outData);

        return outData;
    }

    /// <summary>
    /// Creates NPC data from a CSV file. We will get the following data from CSV:
    ///     - fillercharacter?
    ///     - speakerarchetype?
    ///     - traits
    /// </summary>
    [UnityEditor.MenuItem("Tools/ScriptableObjects/Generate NPC Data from CSV")]
    static void CreateNPCData()
    {

        List<Dictionary<string, string>> csvData = ReadCSVFile("Assets/Data/CSV/speakers.csv");
        
        // if it's not a csv file
        if (csvData == null)
        {
            return;
        }

        // iterate through each npc (row)
        foreach (Dictionary<string, string> npc in csvData)
        {
            // create a new NPCData
            NPCData newNPC = ScriptableObject.CreateInstance<NPCData>();

            newNPC.speakerArchetype = npc["speakerArchetype"];
            
            // split the traits.
            foreach(string trait in npc["speakerTraits"].Split('/'))
            {
                // add the trait into the npcdata list of possible traits
                newNPC.speakerTraits.Add(trait);
            }

            // given the speakerArchetype, we find a sprite in the asset database
            // and add it to the npc's portrait list
            newNPC.dialoguePortraits = new List<Sprite>();

            // get all asset at path npc_speakerarchetype
            var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Portrait/" + newNPC.speakerArchetype + ".png");

            if (assets.Length == 0)
            {
                Debug.LogWarning("Was not able to find asset with name " + newNPC.speakerArchetype);
            }
            else
            {
                Debug.Log("Found asset with name " + newNPC.speakerArchetype + ".png");
                // iterate through the found asset and get all the sprites (kasi texture yung image as-is)
                foreach (Object asset in assets)
                {
                    // check if found asset in all assets is a sprite
                    if (asset is Sprite sprite)
                    {
                        newNPC.dialoguePortraits.Add(sprite);
                        Debug.Log("Added " + sprite.name + " into sprite list for " + newNPC.speakerArchetype);
                    }
                }
            }
            
            

            // create a new asset
            string destPath = "Assets/Scripts/ScriptableObjects/NPCData";
            CreateNPCScriptableObject(destPath, newNPC);
            
        }

    }

    static void CreateNPCScriptableObject(string destPath, NPCData npc)
    {

        // create new scriptableobject if its a valid folder
        if (AssetDatabase.IsValidFolder(destPath))
        {
            // create file name.
            string fname = destPath + "/NPC_" + npc.speakerArchetype + ".asset";

            var asset = AssetDatabase.LoadAssetAtPath<NPCData>(fname);

            // if the file already exists...
            if (asset != null)
            {
                Debug.Log("existing asset...");
                // replace all values of the file
                asset.speakerArchetype = npc.speakerArchetype;
                asset.speakerTraits = npc.speakerTraits;
                asset.dialoguePortraits = npc.dialoguePortraits;
            }
            else
            {
                // create a new asset if the file exists

                // filename = NPC_speakerarchetype.asset
                AssetDatabase.CreateAsset(npc, fname);
            }
        }
        else
        {
            string[] sepPath = destPath.Split('/');
            // new folder
            string newFolder = sepPath[sepPath.Length - 1];
            // join path
            string path = string.Join("/", sepPath);
            AssetDatabase.CreateFolder(path, newFolder);

            Debug.Log("Created folder " + newFolder + " at path " + path);
        }
    }

    /// <summary>
    /// Loads the first sprite encountered at the path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static Sprite LoadSpriteAt(string path)
    {
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (Object asset in allAssets)
        {
            if (asset is Sprite sprite)
            {
                // return the first sprite encountered
                return sprite;
            }
        }

        return null;
    }

    #region XML GENERATION

    /// <summary>
    /// Creates multiple separate XML files for all messaes in a single CSV.
    /// </summary>
    [UnityEditor.MenuItem("Tools/XML/Generate interactions XML")]
    static void CreateXMLFile()
    {
        string ID_COLUMN = "interaction_id";
        string CONTENT_COLUMN = "interaction_message";
        string CONTAINER = "AllInteractions";

        // symbol that indicates you must interact w the message to continue to next msg.
        // used to split the contents into parts.
        string NEXT_SYMBOL = "\\n";
        string COMMA_SYMBOL = "{comma}";
        string MESSAGES_PATH = "Assets/Data/CSV/interactionMessages.csv";
        
        List<Dictionary<string, string>> csvData = ReadCSVFile(MESSAGES_PATH);

        /*
         * FORMAT OF CSV
         *      interaction_id, interaction_messages
         * We assume that rows with empty interaction id have the same id as previously saved id.
         */

        if (csvData == null)
        {
            Debug.LogError("is null");
            return;
        }


        // using xmlwriter
        var writerSettings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t"
        };

        // write root stuff
        string outFile = "Assets/Data/XML/messages/interactionMsgs.xml";

        XmlWriter writer = XmlWriter.Create(outFile, writerSettings);

        writer.WriteStartDocument();
        writer.WriteStartElement(CONTAINER);
        writer.WriteStartElement("Interactions");
        

        for(int rowNum = 0; rowNum < csvData.Count; rowNum++)
        {
            Dictionary<string, string> interaction = csvData[rowNum];

            // if we have an interaction id and it's not the first ever interaction, 
            //we create a new interaction and close previous, else continue off sa kanina
            if (interaction[ID_COLUMN] != "")
            {
                // create a new interaction.
                writer.WriteStartElement("Interaction");
                writer.WriteAttributeString(ID_COLUMN, interaction[ID_COLUMN]);

                // create message array
                writer.WriteStartElement("Messages");

            }

            // create a new message
            writer.WriteStartElement("Message");
            writer.WriteStartElement("Contents");
            

            // split the content column
            foreach (string content in interaction[CONTENT_COLUMN].Split(NEXT_SYMBOL))
            {
                writer.WriteStartElement("Content");
                writer.WriteString(content.Replace(COMMA_SYMBOL, ","));
                writer.WriteEndElement();   // content
            }

            writer.WriteEndElement();       // contents
            writer.WriteEndElement();       // message

            // if next element has an id or if last, then we close
            if(rowNum == csvData.Count - 1 || csvData[rowNum+1][ID_COLUMN] != "")
            {
                writer.WriteEndElement();       // messages
                // close the interaction also
                writer.WriteEndElement();  // interaction
            }
        }

        writer.WriteEndElement();               // interactions
        writer.WriteEndElement();               // container
        writer.WriteEndDocument();
        writer.Close();
    }

    [UnityEditor.MenuItem("Tools/XML/Generate Speaker XML from CSV")]
    static void CreateSpeakersFromCSV()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        // if NOT speakers file, invalid
        if (!path.Contains("speaker"))
        {
            Debug.LogError("Incorrect CSV file. Must be a speaker-related file.");
            return;
        }

        List<Dictionary<string, string>> csvData = ReadCSVFile(path);

        if (csvData == null)
        {
            return;
        }

        string outFile = "Assets/Data/XML";

        if (!AssetDatabase.IsValidFolder(outFile))
        {
            AssetDatabase.CreateFolder("Assets/Data", "XML");
        }

        // set outfile if valid
        outFile = outFile + "/Speakers.xml";

        // using xmlwriter
        var writerSettings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t"
        };

        XmlWriter writer = XmlWriter.Create(outFile, writerSettings);
        

        writer.WriteStartDocument();

        // start with the Speaker collection and speakers
        writer.WriteStartElement("SpeakerCollection");
        writer.WriteStartElement("Speakers");

        // access each row
        foreach(Dictionary<string, string> dictionary in csvData)
        {
            // write Speaker element
            writer.WriteStartElement("Speaker");

            // access the column name and value pairs
            foreach(KeyValuePair<string, string> pair in dictionary)
            {
                // skip speaker traits as this isnt needed in the speaker xml
                if(pair.Key == "speakerTraits")
                {
                    continue;
                }
                
                // write element per pair
                writer.WriteStartElement(pair.Key);

                if (pair.Key == "isFillerCharacter")
                {
                    writer.WriteString(pair.Value.ToLower());
                }
                else
                {
                    // write the value as string
                    writer.WriteString(pair.Value);
                }
                
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteEndDocument();
        writer.Close();
    }
    
    /// <summary>
    /// This is for events, traits, topics db.
    /// </summary>
    [UnityEditor.MenuItem("Tools/XML/Generate ids XML from single-column CSV")]
    static void CreateIdXML()
    {
        // READ
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        List<Dictionary<string, string>> csvData = ReadCSVFile(path);

        // get the filename without the csv extension
        string filename = Selection.activeObject.name.Split('/')[Selection.activeObject.name.Split('/').Length - 1].Split('.')[0];

        if (csvData == null)
        {
            return;
        }

        string outFile = "Assets/Data/XML/DB";

        if (!AssetDatabase.IsValidFolder(outFile))
        {
            AssetDatabase.CreateFolder("Assets/Data/XML", "DB");
        }

        // set outfile if valid -- same name as the csv file name
        outFile = outFile + "/" + filename + ".xml";

        // using xmlwriter
        var writerSettings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t"
        };

        XmlWriter writer = XmlWriter.Create(outFile, writerSettings);

        writer.WriteStartDocument();

        // single column
        writer.WriteStartElement("Main");
        writer.WriteStartElement("ids");
        
        // row-based, since single column, we only have a single dict entry per row
        foreach(Dictionary<string, string> row in csvData)
        {
            writer.WriteStartElement("id");
            writer.WriteString(row["ids"]);

            writer.WriteEndElement();
        }

        // dafault <none> option if the file is topics
        if (filename.ToLower().Contains("topic") || filename.ToLower().Contains("trait"))
        {
            writer.WriteStartElement("id");
            writer.WriteString("none");
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteEndDocument();
        writer.Close();
    }
    
    #endregion

    #region LINE XML
    [UnityEditor.MenuItem("Tools/XML/Generate DialogueLine XML from CSV")]
    static void CreateLineXMLFromCSV()
    {
        // READ
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        // if NOT speakers file, invalid
        if (!path.Contains("dialogue"))
        {
            Debug.LogError("Incorrect CSV file. Must be a dialogue-related file.");
            return;
        }
        
        List<Dictionary<string, string>> csvData = ReadCSVFile(path);

        // get the filename without the csv extension
        string filename = Selection.activeObject.name.Split('/')[Selection.activeObject.name.Split('/').Length - 1].Split('.')[0];

        if (csvData == null)
        {
            return;
        }

        string outFile = "Assets/Data/XML/Dialogue";

        if (!AssetDatabase.IsValidFolder(outFile))
        {
            AssetDatabase.CreateFolder("Assets/Data/XML", "Dialogue");
        }
        
        // set outfile if valid -- same name as the csv file name
        outFile = outFile + "/" + filename + ".xml";

        // using xmlwriter
        var writerSettings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t"
        };

        XmlWriter writer = XmlWriter.Create(outFile, writerSettings);

        writer.WriteStartDocument();

        // start with the Speaker collection and speakers
        writer.WriteStartElement("LineCollection");
        writer.WriteStartElement("DialogueLines");

        // access each row
        foreach (Dictionary<string, string> dictionary in csvData)
        {

            // dline
            writer.WriteStartElement("DialogueLine");

            // merge all events in memory and global and map events
            dictionary.Add("relatedEvents", dictionary["eventsInMemory"] + " / " + dictionary["globalAndMapEvents"]);

            // remove events in memory and global
            dictionary.Remove("eventsInMemory");
            dictionary.Remove("globalAndMapEvents");

            // separate the effects
            Dictionary<string, string> effects = new Dictionary<string, string>();

            // get the key-value pairs where the columns include the tag "effect"
            foreach(string effect in dictionary.Keys.Where(x => x.Contains("effect")).ToList())
            {
                effects.Add(effect.Replace("effect_", ""), dictionary[effect]);

                // remove the said effect from dictionary
                dictionary.Remove(effect);
            }
            
            /*
             * WRITING TO XML 
             */

            // DIALOGUE DATA
            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                
                // array or multivalued attribs.
                if (new List<string>() { "relatedEvents", "relatedTopics", "locations" }.Contains(pair.Key))
                {
                    WriteArrayElements(writer, pair.Key, pair.Value);
                }
                // non-array
                else
                {
                    writer.WriteStartElement(pair.Key);

                    // if it's the dialogue, we first convert the {comma} tags into actual commas.
                    if (pair.Key == "dialogue")
                    {
                        string val = pair.Value.Replace("{next}", "\n");
                        writer.WriteString(val.Replace("{comma}", ","));
                    }
                    // EMPTY WEIGHTS ARE 1 BY DEFAULT.
                    else if (pair.Key.Contains("Weight") && pair.Value=="")
                    {
                        writer.WriteString("1");
                    }
                    // EMPTY PORTRAIT FIELD IS GOING TO BE NEUTRAL BY DEFAULT
                    else if(pair.Key.Contains("portrait") && pair.Value == "")
                    {
                        writer.WriteString("neutral");
                    }
                    // EMPTY REL PREREQ WILL BE NEUTRAL STRING
                    else if (pair.Key.Contains("relPrereq") && pair.Value == "")
                    {
                        writer.WriteString(DirectorConstants.REL_STATUS_STRING.NONE);
                    }
                    else if(pair.Key.Contains("trait") && pair.Value == "")
                    {
                        writer.WriteString("none");
                    }
                    else if(pair.Key.Contains("responseTone") && pair.Value == "")
                    {
                        writer.WriteString("neutral");
                    }
                    else
                    {
                        writer.WriteString(pair.Value);
                    }

                    writer.WriteEndElement();
                }
                
            }

            // write the dialogue effects if any
            WriteDialogueEffects(writer, effects);
            
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteEndDocument();
        writer.Close();
    }

    /// <summary>
    /// function to write xml elements that are in array form or multivalued.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    static void WriteArrayElements(XmlWriter writer, string key, string value)
    {
        writer.WriteStartElement(key);

        string[] values = value.Split(" / ", System.StringSplitOptions.RemoveEmptyEntries);

        // checking if the values.Length == 0, this means walang laman yung csv part na yan. We simply place an empty single
        // value into it.
        if (values.Length != 0)
        {
            // split all according to ' / ' character (include spaces)
            foreach (string item in values)
            {
                writer.WriteStartElement($"{key}Item");
                writer.WriteString(item);
                writer.WriteEndElement();
            }
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes the dialogue effects if there are effects existing
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="effects"></param>
    static void WriteDialogueEffects(XmlWriter writer, Dictionary<string, string> effects)
    {
        writer.WriteStartElement("DialogueEffect");

        // EFFECT DATA
        foreach (KeyValuePair<string, string> effect in effects)
        {
            // if multivalue
            if (effect.Key.Contains("add"))
            {
                WriteArrayElements(writer, effect.Key, effect.Value);
            }
            else
            {
                writer.WriteStartElement(effect.Key);

                // check if emoty or null yung relationshipEffect
                if (effect.Key == "relationshipEffect" && effect.Value == "")
                {
                    writer.WriteString("0");
                }
                else if(effect.Key == "exit")
                {
                    if (effect.Value == "")
                    {
                        writer.WriteString("false");
                    }
                    else
                    {
                        writer.WriteString(effect.Value.ToLower());
                    }
                }
                else
                {
                    writer.WriteString(effect.Value);
                }

                writer.WriteEndElement();
            }
        }

        writer.WriteEndElement();
    }

    #endregion

    static void TestPrint(List<Dictionary<string,string>> data)
    {
        int i = 0;
        foreach(Dictionary<string, string> dict in data)
        {
            Debug.Log($"Row count: {i++}");

            // access each keyvalue pair
            foreach(KeyValuePair<string, string> pair in dict)
            {
                Debug.Log($"{pair.Key} : {pair.Value}");
            }
        }
    }
    
}
