using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AutoLOD;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using static Result;

public class MaterialLowRes : BugCreator
{
    Mesh[] mesh_old = new Mesh[1000];
    GameObject[] victim_old = new GameObject[1000];
    [Range(0,1)]public float quality = 0.05f;
    public bool createInRange;
    public float distance = 10;
    int length = 0;
    public int minimumSize = 50;

    public override void Create_bug()
    {
        fix_bug();
        GameObject victim = get_victim();
        if (createInRange)
        {
            List<GameObject> victims = FindObjectsWithinDistance(victim, distance);
            foreach (GameObject v in victims)
            {
                create_helper(v);
            }
        }
        create_helper(victim);
    }
    //Create bug on all children recuisively
    private void create_helper(GameObject go)
    {
        Transform[] children = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            GameObject go1 = child.gameObject;
            if (victim_old.Contains(go1))//No duplicate
            {
                continue;
            }
            if (go1.TryGetComponent<MeshFilter>(out MeshFilter filter))
            {
                
                victim_old[length] = go1;
                mesh_old[length] = filter.sharedMesh;
                length++;
                MeshSimplifier m;
                try
                {
                    m = new MeshSimplifier(Instantiate(filter.sharedMesh));
                } catch (System.Exception e)
                {
                    Debug.LogError(e);
                    EditorApplication.isPlaying = false;
                    continue;
                }
                
                m.SimplifyMesh(quality);
                filter.sharedMesh = m.ToMesh();
                if (child.transform.childCount > 0 && child.gameObject != go)
                {
                    create_helper(go1);
                }
            }
        }
    }
    protected override void initialize()
    {
        if (createInRange)
        {
            Bug_Desription = "Mesh has a super low LOD, making the shape very distorted, for some obejects that are near";
        } else
        {
            Bug_Desription = "Mesh has a super low LOD, making the shape very distorted,";
        }
    }
    public override void fix_bug() {
        for (int i = 0; i < length; i++)
        {
            victim_old[i].GetComponent<MeshFilter>().mesh = mesh_old[i];
        }
        length = 0;
        victim_old = new GameObject[1000];
    }


    public override bool victim_requiement(GameObject go)
    {
        int i = 0;
        if (go.TryGetComponent<Renderer>(out var renderer))
        {
            return go.GetComponent<Renderer>().bounds.size.magnitude > minimumSize;
        }
        else
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds();
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
                if (bounds.size.magnitude > minimumSize)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public List<GameObject> FindObjectsWithinDistance(GameObject inputObject, float distance)
    {
        List<GameObject> objectsWithinDistance = new List<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(inputObject.transform.position, distance);

        foreach (Collider collider in colliders)
        {
            GameObject obj = collider.gameObject;
            if (obj != inputObject)
            {
                objectsWithinDistance.Add(obj);
            }
        }

        return objectsWithinDistance;
    }

    public override Result get_discription(Result result, GameObject go)
    {
        result.desription = go.name + " mesh has a super low LOD, making the shape of this object very distorted.";
        return result;
        
    }
    public override string get_tag()
    {
        if (createInRange){
            return "RangedMeshLowLOD";
        } else
        {
            return "MeshLowLOD";
        }
        
    }

    
}
