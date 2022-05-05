using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
    public List<ParticleSystem> triggleableParticleSystems;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach(ParticleSystem ps in triggleableParticleSystems)
            {
                ps.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach(ParticleSystem ps in triggleableParticleSystems)
            {
                ps.Stop();
            }
        }
    }
}
