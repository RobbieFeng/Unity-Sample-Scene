/*Randomly assign variables for all other cripts.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine.Animations;
using UnityEngine.XR;

public class AssignVariables : MonoBehaviour
{
    public string[] ExcludedTags;
    [HideInInspector] public GameObject victim;
    public int skipTo = 0;
    public bool createMeshCollider = false;
    public String progress = "";
    [HideInInspector] public bool targetChanged = false;
    [HideInInspector] public GameObject bugController;
    private GameObject GameArea;
    private List<GameObject> gos;
    private List<GameObject>.Enumerator em;
    private GameObject currentVictim;
    private Dictionary<string,int> usedAssets; //Record path of selected assets, avoid target repetition
    private int maxTargetRepetition;
    private int index = 0;
    void Awake()
    {
        if (GameArea == null)
        {
            GameArea = this.GetComponent<RecordData>().Area;
        }
        ExcludedTags.Append(GameArea.tag);
        usedAssets = new Dictionary<string,int>();
        maxTargetRepetition = this.GetComponent<RecordData>().maxTargetRepetition;
        //Create collider 
        if (createMeshCollider)
        {
            create_collider();
        }

        //Skip to
        if (skipTo > 0)
        {
            Find_all_victims();
            for (int i = 0; i < skipTo; i++)
            {
                if (!em.MoveNext())
                {
                    throw new Exception("All Victim skipped");
                }
            }
        }
        
    }

    public void setBugController(GameObject go)
    {
        if (bugController == null)
        {
            bugController = go;
        } else
        {
            EditorApplication.isPlaying = false;
            throw new Exception("More than 1 bug creaters are active");

        }
    }


    //This function finds all victims at the beginning, so Select_Victim() can return victim by order
    //Should only be called once at the beginning
    private void Find_all_victims()
    {
        ExcludedTags.Append(GameArea.tag);
        gos = new List<GameObject>();
        Collider[] cols = Physics.OverlapBox(GameArea.transform.position, GameArea.transform.localScale / 2, Quaternion.identity);
        this.transform.position = GameArea.transform.position;
        foreach (Collider c in cols)
        {
            GameObject go = c.gameObject;
            if (ExcludedTags.Contains(go.tag))
            {
                continue;
            }
            if (!go.TryGetComponent<Renderer>(out Renderer r))
            {
                continue;
            }
            //Deal with LODGroup
            if (!bugController.GetComponent<BugCreator>().victim_requiement(go))
            {
                continue;
            }
            if (go.transform.parent != null)
            {
                if (go.transform.parent.TryGetComponent<LODGroup>(out var _l))
                {
                    go = go.transform.parent.gameObject;
                }
            }
            
            gos.Add(go);

        }
        gos = Shuffle(gos);
        em = gos.GetEnumerator();
        em.MoveNext();
        index++;
        Debug.Log("Search Complete, "+gos.Count+" targets found.");
        return;
    }
    //Find a victim within GameArea, with order
    //recorded:If the previous victim is sampled. Only avoid repetition if the victim is sampled
    public GameObject Select_Victim(bool recorded=true)
    {
        if (gos == null)
        {
            Find_all_victims();
        }
        //Add to dictionary
        if (recorded)
        {
            if (currentVictim != null)
            {
                string path = get_key(currentVictim);
                if (usedAssets.ContainsKey(path))
                {
                    usedAssets[path]++;
                }
                else
                {
                    usedAssets.Add(path, 1);
                }

            }
        }
        //Check dictionary for repetition
        
        GameObject go;
        go = em.Current;
        while (true)
        {
            go = em.Current;
            if (usedAssets.ContainsKey(get_key(go)))
            {
                if (usedAssets[get_key(go)] < maxTargetRepetition)
                {
                    break;
                } else
                {
                    if (!em.MoveNext())
                    {                    
                        EditorApplication.isPlaying = false;
                        Debug.LogWarning("Running out of victims");
                        go = null;
                        break;
                    }
                    index++;
                    continue;
                }
            } else
            {
                break;
            }
        }
        
        currentVictim = go;
        if (!em.MoveNext())
        {
            Debug.LogWarning("Running out of victims");

            UnityEditor.EditorApplication.isPlaying = false;
            string path = "process.py";
        }
        index++;
        progress = index + "/" + gos.Count;
        GameObject.Find("ControlBlock").GetComponent<RecordData>().Lookat = go;
        victim = go;
        targetChanged = true;
        return go;

    }

    private string get_key(GameObject go)
    {
        string path;
        if (go.TryGetComponent<MeshFilter>(out MeshFilter mf))
        {
            path = AssetDatabase.GetAssetPath(mf.sharedMesh);
            return path;
        }
        else
        {
            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
            /*
            foreach (MeshFilter mf1 in mfs)
            {
                if (mf1.name.Last() == '0')
                {
                    path = AssetDatabase.GetAssetPath(mf1.sharedMesh);
                    return path;
                }
            }*/
            //Get LOD0 renderer
            path = AssetDatabase.GetAssetPath(mfs[0].sharedMesh);
            return path;
        }
        Debug.LogError("Fail to get key for " + go.name + " !");
        return null;
    }


    private void create_collider()
    {
        GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in gos ){
            if (go.TryGetComponent<MeshRenderer>(out _) && go != GameArea) { 
                go.AddComponent<MeshCollider>();
            }
        }
    }

    public List<GameObject> Shuffle(List<GameObject> listToShuffle)
    {
        System.Random rand = new System.Random();
        for (int i = listToShuffle.Count - 1; i > 0; i--)
        {
            var k = rand.Next(i + 1);
            var value = listToShuffle[k];
            listToShuffle[k] = listToShuffle[i];
            listToShuffle[i] = value;
        }

        return listToShuffle;
    }

}
