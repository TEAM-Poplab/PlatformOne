using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitMenu : MonoBehaviour
{
    public Button quitButton;

    private void Awake()
    {
        quitButton.onClick.AddListener(HandleQuitClick);
    }

    void HandleQuitClick()
    {
        GameManager.Instance.QuitGame();
    }
}
