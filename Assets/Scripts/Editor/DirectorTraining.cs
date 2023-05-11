using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Serialization;
using UnityEngine;

/// <summary>
/// Class that learns the probabilities of dialogue based on our linedb
/// </summary>
public class DirectorTraining
{
    public const int TOTAL_REL = 4;

    public static int RelStrToInt(string rel)
    {
        if (rel == REL_STATUS_STRING.GOOD)
        {
            return (int)REL_STATUS_NUMS.GOOD;
        }
        else if (rel == REL_STATUS_STRING.BAD)
        {
            return (int)REL_STATUS_NUMS.BAD;
        }
        else if(rel == REL_STATUS_STRING.NEUTRAL)
        {
            return (int)REL_STATUS_NUMS.NEUTRAL;
        }

        return (int)REL_STATUS_NUMS.NONE;
    }

    /// <summary>
    /// Gets the conditional probability table values of the dialogue given all possible values. 
    /// We use all the lines in our linedb as our "observations" or sample.
    /// </summary>
    [UnityEditor.MenuItem("Tools/Training/Infer initial Dialogue CPTs")]
    public static void GetDialogueCPT()
    {
        // load events
        Dictionary<int, string> eventsDB = IdCollection.LoadArrayAsDict(Director.EVENTS_XML_PATH);
        Dictionary<int, string> traitsDB = IdCollection.LoadArrayAsDict(Director.TRAITS_XML_PATH);

        Debug.Log(eventsDB.Count);
        Debug.Log(traitsDB.Count);

        int totalRelStatus = TOTAL_REL;

        // initialize the lineDb
        Dictionary<int, DialogueLine> lineDB = DialogueLineCollection.LoadAll(new string[] {
            "Data/XML/dialogue/dialoguePlayer.xml",
            "Data/XML/dialogue/dialogueJonathan.xml",
            "Data/XML/dialogue/dialogueFiller_Custodian.xml"
        });

        // initialize the model
        DirectorModel model = new DirectorModel(eventsDB.Count, traitsDB.Count, lineDB.Count, totalRelStatus);

        // data is first set as uniform here.
        DirectorData data = model.SetData();

        List<int> lineObservations = new List<int>();
        List<int> eventObservations = new List<int>();
        List<int> traitObservations = new List<int>();
        List<int> relObservations = new List<int>();

        // compiling our observations from lineDb
        foreach (KeyValuePair<int, DialogueLine> dlPair in lineDB)
        {
            /*
             *  here we compile each line and their requirements as samples
             *      The ith element in each list lineObservations, eventObservations, traitObservations, and relObservations
             *      represent the same row within the cpt kung baga
             *      
             *      Basically it looks like this:
             *         | lineObs  | evObs | traitObs | relObs |  => these cols are the list above, passed into param learning
             *       1 |    obs   | obs   |    obs   |  obs   |  => the values (integer form) of the observation
             *       2 |    obs   | obs   |    obs   |  obs   |
             *     ... |    obs   | obs   |    obs   |  obs   |
             */

            // for eazy access
            DialogueLine line = dlPair.Value;
            int lineId = dlPair.Key;

            // we get ALL event requirements of the line, and convert them to their respective key
            List<int> prereqEvents = new List<int>();
            // converting each related event to respective key...
            line.relatedEvents.ToList().ForEach(
                e => prereqEvents.Add(
                    Director.NumKeyLookUp(e, refDict: eventsDB)));
            
            // for every related event
            // we observe that the line with id lineId is the effect
            // and we also observe that to get said lineId, we also have the trait listed in the Dline and relationship.
            foreach (int relEvent in prereqEvents)
            {
                // save the line as an observation
                lineObservations.Add(lineId);
                // save relEvent as observation
                eventObservations.Add(relEvent);
                traitObservations.Add(Director.NumKeyLookUp(line.traitPrereq, refDict: traitsDB));
                relObservations.Add(RelStrToInt(line.relPrereq));
            }
            
        }

        Debug.Log("LENGTHS OF THE OBSERVATIONS:\n" +
            $"line {lineObservations.Count}\n" +
            $"event {eventObservations.Count}\n" +
            $"trait {traitObservations.Count}\n" +
            $"rel {relObservations.Count}\n");

        // once all lines are finished, we convert them to array and make inferences.
        // all of these must have the SAME length. this is the overall row count of our "table"
        int[] lineArr = lineObservations.ToArray();
        int[] evArr = eventObservations.ToArray();
        int[] traitArr = traitObservations.ToArray();
        int[] relArr = relObservations.ToArray();

        // make inferences.
        Dirichlet[][][] dlineDistrib = model.LearnDialogueCPT(lineArr, evArr, traitArr, relArr, data);

        // IN HERE WE SERIALIZE THE DIRICHLET INTO AN XML
        string path = "Assets/Data/XML/Dialogue/";
        string fname = "lineCPT.xml";

        DataContractSerializer serializer = new DataContractSerializer(typeof(Dirichlet[][][]),
            new DataContractSerializerSettings
            {
                DataContractResolver = new InferDataContractResolver()
            });

        using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(new FileStream(path+fname, FileMode.Create)))
        {
            serializer.WriteObject(writer, dlineDistrib);
        }
    }
}
