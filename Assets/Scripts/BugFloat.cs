using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Result;
using static RecordData;
using static UnityEngine.GraphicsBuffer;

public class BugFloat : BugCreator
{
    private GameObject victimOld;
    private Vector3 posOld;
    public float hight = 10;
    public override void Create_bug()
    {
        fix_bug();
        victim = get_victim();
        victimOld = victim;
        //GameObject newobject = Instantiate(victim, new Vector3(victim.transform.position.x, victim.transform.position.y + hight, victim.transform.position.z), victim.transform.rotation);
        posOld = victim.transform.position;
        victim.transform.position = new Vector3(victim.transform.position.x, victim.transform.position.y + hight, victim.transform.position.z);
    }
    public override void fix_bug()
    {
        if (victimOld)
        {
            victimOld.transform.position = posOld;
            victimOld = null;
        }
    }
    public override string get_tag()
    {
        return "Floating";
    }
    protected override void initialize()
    {
        Bug_Desription = "Floating";
        //It seems that animator prevent objects from been moved
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
        }
    }
    public override bool victim_requiement(GameObject go)
    {
        return true;
    }

    public override Result get_discription(Result result, GameObject go)
    {
        result.desription = go.name + " is floating on the air, and it is supposed to be on the ground";
        return result;
    }

    public override bool camera_placement_requiement(Camera camera, GameObject target)
    {
        //target should not be under the camera
        if (Physics.Raycast(camera.gameObject.transform.position, Vector3.down, out RaycastHit hit)) {
            if (hit.collider.gameObject == target)
            {
                return false;
            }
        }


        GameObject newobject = Instantiate(target, new Vector3(target.transform.position.x, target.transform.position.y + hight, target.transform.position.z), target.transform.rotation);
        check_visual_with_corners(camera.gameObject, newobject, out RaycastHit _ray, out int num, out bool see_center);
        bool inFrustum = turn_helper(camera, newobject);
        Destroy(newobject);
        return ((num > 2 || see_center) && inFrustum);
    }

    public override GameObject predict_create(GameObject target, out bool RequireDestroy)
    {
        RequireDestroy = true;
        return Instantiate(target, new Vector3(target.transform.position.x, target.transform.position.y + hight, target.transform.position.z), target.transform.rotation);
    }
}
