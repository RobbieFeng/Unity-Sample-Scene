using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AssignVariables;
using static Result;

public abstract class BugCreator : MonoBehaviour
{
    [HideInInspector]public GameObject victim;
    [HideInInspector]public GameObject ControlBlock;
    [HideInInspector] public string Bug_Desription = "Unkown";
    private bool initialized = false;
    //To get the target of creating bug
    public GameObject get_victim()
    {
        if (ControlBlock == null)
        {
            victim = GameObject.Find("ControlBlock").GetComponent<AssignVariables>().victim;
            return victim;
        } else
        {
            return this.victim;
        }
    }

    void Awake()
    {
        if (!initialized)
        {
            initialize();
        }

        GameObject.Find("ControlBlock").GetComponent<AssignVariables>().setBugController(this.gameObject);
    }
    protected abstract void initialize();

    public abstract void Create_bug();
    public abstract void fix_bug();
    public string Fix_bug_delegate()
    {
        fix_bug();
        return "";
    }
    //Return true if the input gameobject can be a valid victim.
    public virtual bool victim_requiement(GameObject go)
    {
        return true;
    }
    public virtual Result get_discription(Result result, GameObject go)
    {
        result.desription = Bug_Desription + "on this object:" + go.name;
        return result;
    }
    public virtual bool camera_placement_requiement(Camera camera,GameObject target)
    {
        return true;
    }
    //override this if bug involves victim position change
    //Return a copy of the victim, at the position where it will be after bug is created
    //victim must move to this position after next bug_create()
    public virtual GameObject predict_create(GameObject target, out bool RequireDestroy)
    {
        //GameObject go = Instantiate(target,target.transform.position, target.transform.rotation);
        RequireDestroy = false;
        return target;
    }

    public abstract string get_tag();
}
