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
            string spriteFname = archetype + "_sprite_0.png";
            string portraitFname = archetype + "_portrait.png";

            newNpc.npcSprite = (Sprite)AssetDatabase.LoadAssetAtPath("Sprites/Characters/World" + spriteFname, typeof(Sprite));
            newNpc.npcPortrait = (Sprite)AssetDatabase.LoadAssetAtPath("Sprites/Characters/Portrait" + portraitFname, typeof(Sprite));

            // create new scriptableobject if its a valid folder
            if (AssetDatabase.IsValidFolder(destPath))
            {
                // filename = NPC_speakerarchetype.asset
                AssetDatabase.CreateAsset(newNpc, destPath + "/NPC_" + newNpc.speakerArchetype + ".asset");
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
