/**************************************************************************************************************
* Controls Menu
*
*   A show/hide function for the UI buttons...
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public void ShowHide(bool show) { gameObject.SetActive(show); }
}
