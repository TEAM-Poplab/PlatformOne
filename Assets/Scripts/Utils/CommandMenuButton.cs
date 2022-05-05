using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class CommandMenuButton : MonoBehaviour
{
    [SerializeField]
    private string _commandName;

    private UserMenuManager userMenuManager;

    public string CommandName
    {
        get => _commandName;
        set => _commandName = value;
    }
    // Start is called before the first frame update
    void Awake()
    {
        userMenuManager = GameObject.Find("UserMenuManager").GetComponent<UserMenuManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Index")
        {
            userMenuManager.commandEvent.Invoke(_commandName);
            StartCoroutine(pauseCollisions());
        }
    }

    IEnumerator pauseCollisions()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(0.7f);
        GetComponent<Collider>().enabled = true;
    }
}
