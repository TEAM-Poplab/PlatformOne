/************************************************************************************
* 
* Class Purpose: singleton class which controls any UI related events
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Bolt;
using Ludiq;

public class UserMenuManager : MonoBehaviour
{
    public CommandEvent commandEvent;

    [Header("Handled Objects")]
    [Tooltip("Buttons that the script manages. IMPORTANT: be sure both Buttons and Buttons Events have the same dimension, and that the event called for the button has the same index")]
    public List<CommandMenuButton> buttons;

    [Header("Custom Events")]
    [Tooltip("Events called when the button at the corresponding index in Buttons list is pressed. IMPORTANT: be sure both Buttons and Buttons Events have the same dimension")]
    public List<UnityEvent> buttonsEvents = new List<UnityEvent>();

    // Start is called before the first frame update
    void Start()
    {
        if (commandEvent == null)
            commandEvent = new CommandEvent();

        commandEvent.AddListener(CommandHandler);
    }

    private void CommandHandler(string commandName)
    {
        int index = 0;

        foreach (CommandMenuButton commButton in buttons)
        {
            if (commButton.CommandName == commandName)
            {
                buttonsEvents[index].Invoke();
                commButton.GetComponent<TMP_Text>().faceColor = new Color32(255, 255, 255, 255);
            }
            index++;
        }
    }
}

public class CommandEvent : UnityEvent<string>
{

}
