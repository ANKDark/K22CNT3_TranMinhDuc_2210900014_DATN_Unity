using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public Canvas parentCanvas;
    public Transform TooltipTransform;
    public static ItemTooltip instance;
    public TMP_Text txt_name, txt_buff, txt_description;
    public CanvasGroup canvasGroup;
    bool isShowing;
    void Start()
    {
        instance = this;
        isShowing = false;
    }

    void Update()
    {
        if (!parentCanvas) return;
        if (isShowing)
        {
            if (canvasGroup.alpha < 1)
                canvasGroup.alpha += Time.unscaledDeltaTime * 5;
            Vector2 movePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                UnityEngine.InputSystem.Mouse.current.position.ReadValue(),
                parentCanvas.worldCamera,
                out movePosition
            );
            TooltipTransform.localPosition = movePosition;
        }
    }

    public void Show(string nameItem, string buffItem, string descriptionItem)
    {
        canvasGroup.alpha = 0;
        txt_name.text = nameItem;
        txt_buff.text = buffItem;
        txt_description.text = descriptionItem;
        TooltipTransform.gameObject.SetActive(true);
        isShowing = true;
    }

    public void Hide()
    {
        TooltipTransform.gameObject.SetActive(false);
        isShowing = false;
    }
}