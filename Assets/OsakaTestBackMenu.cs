using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OsakaTestBackMenu : MonoBehaviour
{
    private float timer = 0;
    private bool isDone = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= 1 && !isDone)
        {
            ScenesManager.Instance.LoadLevel("LoadingScene");
            isDone = true;
        }

        if(timer >= 4 && isDone)
        {
            ScenesManager.Instance.ActivateScene();
            //ScenesManager.Instance.UnloadLevel(SceneManager.GetActiveScene().name);
            timer = -10;
        }


        timer += Time.deltaTime;
    }
}
