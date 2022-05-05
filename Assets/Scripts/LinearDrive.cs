/************************************************************************************
* 
* Class Purpose: it manages the linear mapping, handling calculation and conversions
*	to produce the linear map value
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearDrive : MonoBehaviour
{

    [Tooltip("The lower limit position for the linear mapping")] public Transform startPosition;
	[Tooltip("The upper limit position for the linear mapping")]  public Transform endPosition;
	public bool repositionGameObject = true;
	public bool maintainMomemntum = true;
	public float momemtumDampenRate = 5.0f;

	protected float initialMappingOffset;
	protected int numMappingChangeSamples = 5;
	protected float[] mappingChangeSamples;
	protected float prevMapping = 0.0f;
	protected float mappingChangeRate;
	protected int sampleCount = 0;
	public float linearMappingValue;

	protected virtual void Awake()
	{
		mappingChangeSamples = new float[numMappingChangeSamples];
	}

	// Start is called before the first frame update
	void Start()
    {
		UpdateLinearMapping(transform);
    }

    // Update is called once per frame
    void Update()
    {
		if (maintainMomemntum && mappingChangeRate != 0.0f)
		{
			//Dampen the mapping change rate and apply it to the mapping
			mappingChangeRate = Mathf.Lerp(mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime);
			linearMappingValue = Mathf.Clamp01(linearMappingValue + (mappingChangeRate * Time.deltaTime));

			if (repositionGameObject)
			{
				transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMappingValue);
			}
		}
	}

	protected void CalculateMappingChangeRate()
	{
		//Compute the mapping change rate
		mappingChangeRate = 0.0f;
		int mappingSamplesCount = Mathf.Min(sampleCount, mappingChangeSamples.Length);
		if (mappingSamplesCount != 0)
		{
			for (int i = 0; i < mappingSamplesCount; ++i)
			{
				mappingChangeRate += mappingChangeSamples[i];
			}
			mappingChangeRate /= mappingSamplesCount;
		}
	}

	public void UpdateLinearMapping(Transform updateTransform)
	{
		prevMapping = linearMappingValue;
		linearMappingValue = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

        mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = (1.0f / Time.deltaTime) * (linearMappingValue - prevMapping);
		sampleCount++;

		if (repositionGameObject)
		{
			transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMappingValue);
		}
	}

	protected float CalculateLinearMapping(Transform updateTransform)
	{
		Vector3 direction = endPosition.position - startPosition.position;
		float length = direction.magnitude;
		direction.Normalize();

		Vector3 displacement = updateTransform.position - startPosition.position;

		return Vector3.Dot(displacement, direction) / length;
	}
}
