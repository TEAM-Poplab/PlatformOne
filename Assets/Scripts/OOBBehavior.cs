using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OOBBehavior : MonoBehaviour
{
    public float threshold = 1.05f;

    [Tooltip("Enables the overlay of objects when the screen becomes black")]
    public bool allowsRenderQueueBehavior = true;

    public Material renderQueueMaterial;

    public List<MeshRenderer> objectsMeshRenderer = new List<MeshRenderer>();
    
    private GameObject deathZone;
    private GameObject platform;
    private Animator _animator;
    private bool _isOOB = false;

    private Vector3 maskVector = new Vector3(1, 0, 1);
    private List<Material> originalMaterials = new List<Material>();

    // Start is called before the first frame update
    void Start()
    {
        //playspace = GameObject.Find("MixedRealityPlayspace");
        platform = GameObject.Find("GuardianCenter/Platform");
        deathZone = GameObject.Find("GuardianCenter/OOBSphere");
        _animator = GetComponent<Animator>();

        deathZone.GetComponent<SphereCollider>().radius = 0.5f * (threshold / 1.05f);
    }

    // Update is called once per frame
    void Update()
    {
        if(_isOOB)
        {
            float distancePlayerFromPlatform =  Vector3.Distance(Vector3.Scale(platform.transform.position, maskVector), Vector3.Scale(transform.position, maskVector)) - threshold;
            //Fade del materiale con interpolazione lineare fra distanza dal centro e posizione attuale del player fuori dalla piattaforma
            distancePlayerFromPlatform *= 5f;
            _animator.SetFloat("FadeMotionTime", Mathf.Lerp(0, 1f, distancePlayerFromPlatform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "DeathZone")
        {
            //GetComponent<Animator>().SetTrigger("OOBStarted");
            _isOOB = true;
            _animator.SetBool("OOBStatus", _isOOB);

            if(allowsRenderQueueBehavior)
            {
                foreach(MeshRenderer mr in objectsMeshRenderer)
                {
                    originalMaterials.Add(mr.material);
                    mr.material = renderQueueMaterial;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DeathZone")
        {
            //GetComponent<Animator>().SetTrigger("OOBStarted");
            _isOOB = false;
            _animator.SetBool("OOBStatus", _isOOB);

            if (allowsRenderQueueBehavior)
            {
                for (int i = 0; i < objectsMeshRenderer.Count; i++)
                {
                    objectsMeshRenderer[i].material = originalMaterials[i];
                }
            }
        }
    }

    public void Reset2DPlayerPosition()
    {
        //playspace.transform.position -= new Vector3(transform.position.x, 0, transform.position.z);
    }
}
