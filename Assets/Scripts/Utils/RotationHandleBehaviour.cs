using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Bolt;
using Ludiq;

public class RotationHandleBehaviour : MonoBehaviour
{
    private GameObject target;
    private List<GameObject> scaleHandlesList;
    private List<GameObject> rotateHandlesList;
    private List<GameObject> translateHandleList;
    private CustomBoundsControl boundsControl;
    private Color originalMaterialColor;
    public Color interactioMaterialColor = new Color(205, 38, 83, 51);
    private Vector3 originalScale;

    public GameObject Target
    {
        get => target;
    }

    public void Init(GameObject target, CustomBoundsControl script)
    {
        this.target = target;
        boundsControl = script;
        scaleHandlesList = boundsControl.ScaleHandlesList;
        rotateHandlesList = boundsControl.RotateHandlesList;
        translateHandleList = boundsControl.TranslateHandleList;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Activation()
    {
        originalScale = transform.localScale;
        transform.localScale = new Vector3(transform.localScale.x * 1.5f, transform.localScale.y * 1.5f, transform.localScale.z * 1.5f);

        foreach (GameObject handle in scaleHandlesList)
        {
            handle.SetActive(false);
        }

        foreach (GameObject handle in rotateHandlesList)
        {
            if (handle.name != gameObject.name)
            {
                handle.SetActive(false);
            }     
        }

        foreach (GameObject handle in translateHandleList)
        {
            handle.SetActive(false);
        }

        originalMaterialColor = GetComponent<MeshRenderer>().material.GetColor("_Color");
        GetComponent<MeshRenderer>().material.SetColor("_Color", interactioMaterialColor);
    }

    public void Deactivation()
    {
        transform.localScale = originalScale;

        foreach (GameObject handle in scaleHandlesList)
        {
            handle.SetActive(true);
        }

        foreach (GameObject handle in rotateHandlesList)
        {
            if (handle.name != gameObject.name)
            {
                handle.SetActive(true);
            }
        }

        foreach (GameObject handle in translateHandleList)
        {
            handle.SetActive(true);
        }

        GetComponent<MeshRenderer>().material.SetColor("_Color", originalMaterialColor);
    }

    public void AddConstraintToTarget()
    {
        var aimConstraint = target.AddComponent<AimConstraint>();
        ConstraintSource source = new ConstraintSource();
        GameObject sourceObject = (GameObject) Variables.Application.Get("PinchingFinger");
        source.sourceTransform = sourceObject.transform;
        source.weight = 1f;
        aimConstraint.AddSource(source);
        SetRotationAxis(aimConstraint);
        Transform targetTransformBeforeConstraint = target.transform;
        aimConstraint.constraintActive = true;
        CalculateOffset(aimConstraint, targetTransformBeforeConstraint);
    }

    public void RemoveConstraintFromTarget()
    {
        var aimConstraint = target.GetComponent<AimConstraint>();
        aimConstraint.constraintActive = false;
        aimConstraint.RemoveSource(0);
        Destroy(aimConstraint);
    }

    private void SetRotationAxis(AimConstraint constraint)
    {
        if (transform.localEulerAngles.x != 0)
        {
            constraint.aimVector = new Vector3(1, 0, 0);
            constraint.rotationAxis = Axis.Z;
        } else if (transform.localEulerAngles.z != 0) {
            constraint.aimVector = new Vector3(0, 0, -1);
            constraint.rotationAxis = Axis.X;
        } else
        {
            constraint.aimVector = new Vector3(0, 0, 1);
            constraint.rotationAxis = Axis.Y;
        }
    }

    private void CalculateOffset(AimConstraint constraint, Transform trns)
    {
        Transform targetTransformAfterConstraint = target.transform;
        //constraint.constraintActive = false;
        float angleDiff;
        float sign;
        if (transform.localEulerAngles.x != 0)
        {
            angleDiff = Mathf.DeltaAngle(targetTransformAfterConstraint.localEulerAngles.z, trns.localEulerAngles.z);
            sign = Vector3.Dot(trns.position, targetTransformAfterConstraint.position);
            constraint.rotationOffset = new Vector3(targetTransformAfterConstraint.localEulerAngles.x, targetTransformAfterConstraint.localEulerAngles.y, angleDiff*Mathf.Sign(sign));
            constraint.rotationAtRest = trns.rotation.eulerAngles;
        } else if (transform.localEulerAngles.z !=0)
        {
            angleDiff = Mathf.DeltaAngle(targetTransformAfterConstraint.localEulerAngles.x, trns.localEulerAngles.x);
            sign = Vector3.Dot(trns.position, targetTransformAfterConstraint.position);
            constraint.rotationOffset = new Vector3(angleDiff * Mathf.Sign(sign), targetTransformAfterConstraint.localEulerAngles.y, targetTransformAfterConstraint.localEulerAngles.z);
            constraint.rotationAtRest = trns.rotation.eulerAngles;
        } else
        {
            angleDiff = Mathf.DeltaAngle(targetTransformAfterConstraint.localEulerAngles.y, trns.localEulerAngles.y);
            sign = Vector3.Dot(trns.position, targetTransformAfterConstraint.position);
            constraint.rotationOffset = new Vector3(targetTransformAfterConstraint.localEulerAngles.x, angleDiff * Mathf.Sign(sign), targetTransformAfterConstraint.localEulerAngles.z);
            constraint.rotationAtRest = trns.rotation.eulerAngles;
        }
        //constraint.constraintActive = true;
    }
}
