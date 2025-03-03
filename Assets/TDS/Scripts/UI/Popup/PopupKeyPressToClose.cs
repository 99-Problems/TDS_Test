using UnityEngine;

public class PopupKeyPressToClose : MonoBehaviour
{
    private PopupBase popupBase;

    // Start is called before the first frame update
    void Start()
    {
        popupBase = GetComponent<PopupBase>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (popupBase.isAfeectedBlockUI && Managers.Popup.IsOpenBlockInputPopup())
                return;

            if (popupBase.IsFocus())
            {
                popupBase.PressBackButton();
            }
        }
    }
}
