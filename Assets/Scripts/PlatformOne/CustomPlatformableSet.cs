using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlatformableSet : MonoBehaviour
{
    [Header("Materials")]
    public Material columnRealityOnMaterial;
    public Material columnRealityOffMaterial;

    [Space(10)]
    public Material wingframeRealityOnMaterial;
    public Material wingglassRealityOnMaterial;
    public Material wingframeRealityOffMaterial;
    public Material wingglassRealityOffMaterial;

    [Space(10)]
    public Material platformRealityOnMaterial;
    public Material platformRealityOffMaterial;

    [Space(10)]
    public Material glassMaterialWhenPlatformed;
    public Material generalMaterialWhenPlatformed;

    private GeometrySetModule.savedTranform custom;
    private GeometrySetModule.savedTranform custom2;
    private bool done = false;
    private bool done2 = false;
    private bool done3 = false;

    // Start is called before the first frame update
    void Start()
    {
        custom.position = Vector3.zero;
        custom.rotation = Quaternion.identity;
        custom.scale = Vector3.one;

        custom2.position = transform.position;
        custom2.rotation = transform.rotation;
        custom2.scale = Vector3.one;

        //GameManager.Instance.OnDayLightSet.AddListener(OnDayLightSetMaterial);
        //GameManager.Instance.OnNightLightSet.AddListener(OnNightLightSetMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        if (!done)
        {
            StartCoroutine(SetOriginValues());
            done = true;
        }

        if (!done2 && GetComponent<Platformable>().Platformstate == Platformable.PlatformState.Platformed)
        {
            OnDockedSetMaterial();
            done2 = true;
        } else if (done2 && GetComponent<Platformable>().Platformstate == Platformable.PlatformState.Unplatformed)
        {
            OnNightLightSetMaterial();
            done2 = false;
        }
    }

    IEnumerator SetOriginValues()
    {
        yield return new WaitForSeconds(1f);
        GetComponent<Platformable>().SavedOriginTransform = custom;
        GetComponent<Platformable>().SavedCentroidTransformWhenPivoted = custom2;

    }

    IEnumerator StopAndPlay(Animator anim)
    {
        anim.keepAnimatorControllerStateOnDisable = false;
        anim.enabled = false;
        yield return new WaitForSeconds(0.45f);
        anim.enabled = true;
    }

    public void OnDayLightSetMaterial()
    {
        var mesh = transform.GetChild(0);
        Material[] changingMaterial;

        //mesh children have different materials and different order so they have to be set one by one.
        //This is a custom geometry so it won't be a problem doing this once
        mesh.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = columnRealityOnMaterial;

        changingMaterial = mesh.GetChild(1).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = wingglassRealityOnMaterial;
        changingMaterial[1] = wingframeRealityOnMaterial;
        mesh.GetChild(1).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOnMaterial;
        changingMaterial[0] = wingframeRealityOnMaterial;
        mesh.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(3).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOnMaterial;
        changingMaterial[0] = wingframeRealityOnMaterial;
        mesh.GetChild(3).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(4).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOnMaterial;
        changingMaterial[0] = wingframeRealityOnMaterial;
        mesh.GetChild(4).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(5).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = wingglassRealityOnMaterial;
        changingMaterial[1] = wingframeRealityOnMaterial;
        mesh.GetChild(5).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(6).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOnMaterial;
        changingMaterial[0] = wingframeRealityOnMaterial;
        mesh.GetChild(6).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(7).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOnMaterial;
        changingMaterial[0] = wingframeRealityOnMaterial;
        mesh.GetChild(7).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(8).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = wingglassRealityOnMaterial;
        changingMaterial[1] = wingframeRealityOnMaterial;
        mesh.GetChild(8).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(9).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOnMaterial;
        changingMaterial[0] = wingframeRealityOnMaterial;
        changingMaterial[2] = wingframeRealityOnMaterial;
        mesh.GetChild(9).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        mesh.GetChild(10).gameObject.GetComponent<MeshRenderer>().material = platformRealityOnMaterial;
    }

    public void OnNightLightSetMaterial()
    {
        var mesh = transform.GetChild(0);
        Material[] changingMaterial;

        //mesh children have different materials and different order so they have to be set one by one.
        //This is a custom geometry so it won't be a problem doing this once
        mesh.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = columnRealityOffMaterial;

        changingMaterial = mesh.GetChild(1).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = wingglassRealityOffMaterial;
        changingMaterial[1] = wingframeRealityOffMaterial;
        mesh.GetChild(1).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOffMaterial;
        changingMaterial[0] = wingframeRealityOffMaterial;
        mesh.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(3).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOffMaterial;
        changingMaterial[0] = wingframeRealityOffMaterial;
        mesh.GetChild(3).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(4).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOffMaterial;
        changingMaterial[0] = wingframeRealityOffMaterial;
        mesh.GetChild(4).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(5).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = wingglassRealityOffMaterial;
        changingMaterial[1] = wingframeRealityOffMaterial;
        mesh.GetChild(5).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(6).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOffMaterial;
        changingMaterial[0] = wingframeRealityOffMaterial;
        mesh.GetChild(6).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(7).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOffMaterial;
        changingMaterial[0] = wingframeRealityOffMaterial;
        mesh.GetChild(7).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(8).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = wingglassRealityOffMaterial;
        changingMaterial[1] = wingframeRealityOffMaterial;
        mesh.GetChild(8).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(9).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = wingglassRealityOffMaterial;
        changingMaterial[0] = wingframeRealityOffMaterial;
        changingMaterial[2] = wingframeRealityOffMaterial;
        mesh.GetChild(9).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        mesh.GetChild(10).gameObject.GetComponent<MeshRenderer>().material = platformRealityOffMaterial;
    }

    private void OnDockedSetMaterial()
    {
        var mesh = transform.GetChild(0);
        Material[] changingMaterial;

        //mesh children have different materials and different order so they have to be set one by one.
        //This is a custom geometry so it won't be a problem doing this once
        mesh.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = generalMaterialWhenPlatformed;

        changingMaterial = mesh.GetChild(1).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = glassMaterialWhenPlatformed;
        changingMaterial[1] = generalMaterialWhenPlatformed;
        mesh.GetChild(1).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = glassMaterialWhenPlatformed;
        changingMaterial[0] = generalMaterialWhenPlatformed;
        mesh.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(3).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = glassMaterialWhenPlatformed;
        changingMaterial[0] = generalMaterialWhenPlatformed;
        mesh.GetChild(3).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(4).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = glassMaterialWhenPlatformed;
        changingMaterial[0] = generalMaterialWhenPlatformed;
        mesh.GetChild(4).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(5).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = glassMaterialWhenPlatformed;
        changingMaterial[1] = generalMaterialWhenPlatformed;
        mesh.GetChild(5).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(6).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = glassMaterialWhenPlatformed;
        changingMaterial[0] = generalMaterialWhenPlatformed;
        mesh.GetChild(6).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(7).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = glassMaterialWhenPlatformed;
        changingMaterial[0] = generalMaterialWhenPlatformed;
        mesh.GetChild(7).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(8).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[0] = glassMaterialWhenPlatformed;
        changingMaterial[1] = generalMaterialWhenPlatformed;
        mesh.GetChild(8).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        changingMaterial = mesh.GetChild(9).gameObject.GetComponent<MeshRenderer>().materials;
        changingMaterial[1] = glassMaterialWhenPlatformed;
        changingMaterial[0] = generalMaterialWhenPlatformed;
        changingMaterial[2] = generalMaterialWhenPlatformed;
        mesh.GetChild(9).gameObject.GetComponent<MeshRenderer>().materials = changingMaterial;

        mesh.GetChild(10).gameObject.GetComponent<MeshRenderer>().material = generalMaterialWhenPlatformed;
    }
}
