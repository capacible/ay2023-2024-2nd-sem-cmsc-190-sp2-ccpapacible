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
        DirectorData data = model.UniformDirectorData();

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

            /*
             *  Getting trait and relationship info
             */
            int[] traitPrereqs;
            int[] relPrereqs;
            int traitPrereq = Director.NumKeyLookUp(line.traitPrereq, refDict: traitsDB);
            int relPrereq = RelStrToInt(line.relPrereq);

            // if trait doesn't exist (EMPTY)
            if (traitPrereq == -1)
            {
                traitPrereqs = traitsDB.Keys.ToArray();
            }
            else
            {
                // if the trait does exist, then that's our observation.
                traitPrereqs = new int[]
                {
                    traitPrereq
                };
            }

            if(relPrereq == (int)REL_STATUS_NUMS.NONE)
            {
                // consider all possible relationships
                relPrereqs = new int[]
                {
                    (int)REL_STATUS_NUMS.GOOD,
                    (int)REL_STATUS_NUMS.BAD,
                    (int)REL_STATUS_NUMS.NEUTRAL
                };
            }
            else
            {
                relPrereqs = new int[] { relPrereq };
            }

            // we add each possible observation that can make this dialogue line appear
            // note that when the dialogue line has an empty trait or relationship field, this just means that
            // all other traits and relationships are to be considered -- there is no requirement, so any trait/rel goes.
            foreach(int trait in traitPrereqs)
            {
                foreach(int rel in relPrereqs)
                {
                    if (prereqEvents.Count == 0)
                    {
                        // we basically consider ALL possible events, because prerequisite events being empty = walangv prerequisite
                        // same idea when it comes to traits and rels as seen in outer loops, by the way.
                        foreach(int ev in eventsDB.Keys)
                        {
                            lineObservations.Add(lineId);
                            traitObservations.Add(trait);
                            relObservations.Add(rel);
                            eventObservations.Add(ev);
                        }
                    }
                    else
                    {
                        // for every related event
                        // we observe that the line with id lineId is the effect
                        // and we also observe that to get said lineId, we also have the trait listed in the Dline and relationship.
                        foreach (int ev in prereqEvents)
                        {
                            lineObservations.Add(lineId);
                            traitObservations.Add(trait);
                            relObservations.Add(rel);
                            eventObservations.Add(ev);
                        }
                    }
                }
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

        // make inferences here
        model.Learn(lineArr, evArr, traitArr, relArr, data);
        // return the inference as director data
        DirectorData learned = model.DataFromPosteriors();

        // IN HERE WE SERIALIZE THE DIRICHLET INTO AN XML
        string path = "Assets/Data/XML/Dialogue/";

        SerializeCPT<Dirichlet[][][]>(path, "lineCPT.xml", learned.dialogueProb);
        SerializeCPT<Dirichlet>(path, "eventCPT.xml", learned.eventsProb);
        SerializeCPT<Dirichlet>(path, "traitCPT.xml", learned.traitsProb);
        SerializeCPT<Dirichlet>(path, "relCPT.xml", learned.relProb);
    }

    public static void SerializeCPT<T>(string path, string fname, object toSerialize)
    {
        T distribution = (T)toSerialize;

        DataContractSerializer serializer = new DataContractSerializer(typeof(T),
            new DataContractSerializerSettings
            {
                DataContractResolver = new InferDataContractResolver()
            });

        using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(new FileStream(path + fname, FileMode.Create)))
        {
            serializer.WriteObject(writer, distribution);
        }
    }
}
