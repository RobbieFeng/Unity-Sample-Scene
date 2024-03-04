using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Placeholder texture randomly choose from Assets\Resources\PlaceHolder

public class MaterialLost : BugCreator
{
    Material[][] matOld = new Material[100][];
    GameObject[] go = new GameObject[100];
    int length = 0;
    public override void Create_bug()
    {
        fix_bug();
        go[0] = get_victim();
        //Deal with LOD
        if (go[0].TryGetComponent<LODGroup>(out var _c))
        {
            Create_bug_LOD();
            return;
        }
        //If no LOD
        //Randomly choose a material
        matOld[0] = get_victim().GetComponent<Renderer>().materials;
        var files = Directory.GetFiles(Application.dataPath + "/Resources/PlaceHolder","*.mat"); ;
        string file = files[Random.Range(0, files.Length)];
        file = "PlaceHolder/" + Path.GetFileName(file);
        file = file.Remove(file.Length - 4);
        Material mat = Resources.Load<Material>(file);
        Material[] mats = new Material[matOld[0].Length];
        for (int i = 0; i < matOld[0].Length; i++)
        {
            mats[i] = mat;

        }
        go[0].GetComponent<Renderer>().materials = mats;
        length = 1;
    }
    //If with LOD
    protected void Create_bug_LOD()
    {
        var files = Directory.GetFiles(Application.dataPath + "/Resources/PlaceHolder", "*.mat"); ;
        string file = files[Random.Range(0, files.Length)];
        file = "PlaceHolder/" + Path.GetFileName(file);
        file = file.Remove(file.Length - 4);
        Material mat = Resources.Load<Material>(file);
        GameObject victim = go[0];
        foreach (Transform t in victim.GetComponentsInChildren<Transform>())
        {
            GameObject child = t.gameObject;
            if (child.TryGetComponent<Renderer>(out var _c))
            {
                go[length] = child;
                matOld[length] = child.GetComponent<Renderer>().materials;
                length++;
                int len = child.GetComponent<Renderer>().materials.Length;
                Material[] mats = new Material[len];
                for (int i = 0; i < len; i++)
                {
                    mats[i] = mat;
                }
                child.GetComponent<Renderer>().materials = mats;
            }

        }
    }

    public override void fix_bug()
    {
        for (int i = 0; i < length; i++)
        {
            go[i].GetComponent<Renderer>().materials = matOld[i];
        }
        length = 0;
    }

    protected override void initialize()
    {
        Bug_Desription = "Material is broken for";
    }

    public override bool victim_requiement(GameObject go)
    {
        return true;
    }
    public override string get_tag()
    {
        return "TextureLost";
    }

    public override Result get_discription(Result result, GameObject go)
    {
        result.desription = "Materials for " + go.name + " are all broken, so this object has a wrong color and texture.";
        return result;
    }
}
