using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Mesh sequence playback, 1/04/2018 MikeF
/// Attach to gameobject containing a series of child gameobjects/meshes to cycle through
/// Modify public vars below in the inspector an hit play
/// </summary>
public class MeshSequence : MonoBehaviour
{

	public bool PingPong = false; //ping pong sequence
	public float PlaybackSpeed = 1.0f; //framerate of sequence playback
	public int StartFrame = 0; //starting frame of sequence playback

	private GameObject m_lastFrame;
	private GameObject m_currentFrame;
	private bool m_reverse = false;
	private float CycleTimer = 0.0f;
	private int m_NextFrame = 0;
	private List<GameObject> m_frames = new List<GameObject>();

	void Start()
	{
		SetupFrames();
	}

	private void SetupFrames()
	{
		foreach (Transform child in transform)
		{
			m_frames.Add(child.gameObject);
			child.gameObject.SetActive(false);
		}

		if (StartFrame >= m_frames.Count)
		{
			Debug.Log("Start frame is higher than total number of frames, starting from frame 0");
			StartFrame = 0;
		}

		m_currentFrame = m_frames[StartFrame];
		m_currentFrame.gameObject.SetActive(true);
	}


	void Update()
	{
		CycleTimer += Time.deltaTime * PlaybackSpeed;

		if (CycleTimer >= 1)
		{
			CycleTimer = 0;
			m_lastFrame = m_currentFrame;
			m_lastFrame.gameObject.SetActive(false);
			m_currentFrame = m_frames[m_NextFrame];
			m_currentFrame.gameObject.SetActive(true);

			if (m_NextFrame == m_frames.Count - 1 && !m_reverse)
			{

				if (PingPong)
				{
					m_reverse = true;
					m_NextFrame--;
				}

				else if (!PingPong)
				{
					m_NextFrame = 0;
				}

			}

			if (m_reverse)
			{
				m_NextFrame--;
				if (m_NextFrame == 0)
				{
					m_reverse = false;
					m_NextFrame = 0;
				}
			}

			else m_NextFrame++;
		}
	}


}