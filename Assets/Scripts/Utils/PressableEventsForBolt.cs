using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressableEventsForBolt : MonoBehaviour
{
    public void TouchBeginForBolt(PressableButtonHoloLens2 button)
    {
        button.TouchBegin.Invoke();
    }

    public void TouchEndForBolt(PressableButtonHoloLens2 button)
    {
        button.TouchEnd.Invoke();
    }

    public void ButtonPressedForBolt(PressableButtonHoloLens2 button)
    {
        button.ButtonPressed.Invoke();
    }

    public void ButtonReleasedForBolt(PressableButtonHoloLens2 button)
    {
        button.ButtonReleased.Invoke();
    }

    public void OnClickForBolt(Interactable button)
    {
        button.OnClick.Invoke();
    }
}
