using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSounds : MonoBehaviour
{
    [Header("Sound Names")]
    [SerializeField] private string buttonHoverSound = "ButtonHover";
    [SerializeField] private string buttonClickSound = "ButtonClick";
    [SerializeField] private string menuOpenSound = "MenuOpen";
    [SerializeField] private string menuCloseSound = "MenuClose";

    void Start()
    {
        // Encontrar todos los botones en el menú y añadirles sonidos
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button button in allButtons)
        {
            // Añadir sonidos de hover y click
            AddSoundsToButton(button);
        }
    }

    private void AddSoundsToButton(Button button)
    {
        // Añadir un trigger de sonido al hover
        var buttonEvents = button.gameObject.AddComponent<EventTrigger>();
        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => OnButtonHover());
        buttonEvents.triggers.Add(enterEntry);

        // Añadir sonido al click
        button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonHover()
    {
        AudioManager.Instance?.PlaySound(buttonHoverSound);
    }

    public void OnButtonClick()
    {
        AudioManager.Instance?.PlaySound(buttonClickSound);
    }

    public void PlayMenuOpenSound()
    {
        AudioManager.Instance?.PlaySound(menuOpenSound);
    }

    public void PlayMenuCloseSound()
    {
        AudioManager.Instance?.PlaySound(menuCloseSound);
    }
}