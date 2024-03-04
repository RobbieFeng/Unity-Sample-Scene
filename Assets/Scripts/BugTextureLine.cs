using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class BugTextureLine : BugCreator
{
    private Mesh[] meshOld = new Mesh[100];
    private GameObject[] targetOld = new GameObject[100];
    private int length = 0;
    public bool RandomNumberOfFaultVertices = true;
    public int NumOfFaultVertices = 50;
    
    public override void Create_bug()
    {

        fix_bug();
        victim = get_victim();
        if (RandomNumberOfFaultVertices)
        {
            NumOfFaultVertices = Random.Range(5, 30);
        }
        if (victim.TryGetComponent<LODGroup>(out var _lod))
        {
            Create_Bug_LOD();
            return;
        }
        Mesh mesh = victim.GetComponent<MeshFilter>().sharedMesh;
        meshOld[0] = mesh;
        targetOld[0] = victim;
        length = 1;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < NumOfFaultVertices; i++)
        {
            int index = Random.Range(0, vertices.Length);
            vertices[index] = new Vector3(Random.Range(-9999, 9999), Random.Range(-9999, 9999), Random.Range(-9999, 9999));
        }
        victim.GetComponent<MeshFilter>().mesh.vertices = vertices;

    }
    private void Create_Bug_LOD()
    {
        foreach (MeshFilter mf in victim.GetComponentsInChildren<MeshFilter>())
        {
            targetOld[length] = mf.gameObject;
            meshOld[length] = mf.sharedMesh;
            length++;
            Vector3[] vertices = mf.mesh.vertices;
            for (int i = 0; i < NumOfFaultVertices; i++)
            {
                int index = Random.Range(0, vertices.Length);
                vertices[index] = new Vector3(Random.Range(-9999, 9999), Random.Range(-9999, 9999), Random.Range(-9999, 9999));
            }
            mf.mesh.vertices = vertices;
        }
        return;
    }

    public override void fix_bug()
    {
        for (int i = 0; i < length; i++)
        {
            targetOld[i].GetComponent<MeshFilter>().mesh = meshOld[i];
        }
        length = 0;
    }

    protected override void initialize()
    {
        Bug_Desription = "Some vertices of the mesh are mistakenly set to a very large value,causing distorted mesh for ";
    }
   

    public override bool victim_requiement(GameObject go)
    {
        return true;
    }

    public override Result get_discription(Result result, GameObject go)
    {
        result.desription = "Some vertices of the mesh for object named "+go.name+" are mistakenly set to a very large value,causing sharp mesh lines.";
        return result;
    }

    public override string get_tag()
    {
        return "SharpMesh";
    }

}
