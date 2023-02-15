using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using UnityEditor;

public class Editor : MonoBehaviour
{

    [UnityEditor.MenuItem("Tools/Generate NPC Data from DB")]
    static void GenerateNPCData()
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
        
    }
    
}
