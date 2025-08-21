using UnityEngine;
using UnityEngine.UI;

public class TrainingDummyStatsUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject panel;

    [Header("UI Elements")]
    public InputField healthInput;
    public InputField armorInput;
    public InputField magicResistInput;

    [Header("Dummy Reference")]
    public TrainingDummy dummy;

    void Start()
    {
        panel.SetActive(false);
        UpdateUIFromDummy();
    }

    void Update()
    {
        // Shift+F3 otwiera/zamyka panel
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F3))
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) UpdateUIFromDummy();
        }

        if (panel.activeSelf)
        {
            ApplyUIToDummy();
        }
    }

    void UpdateUIFromDummy()
    {
        if (dummy == null) return;

        healthInput.text = dummy.maxHealth.ToString();
        armorInput.text = dummy.armor.ToString();
        magicResistInput.text = dummy.magicResist.ToString();
    }

    void ApplyUIToDummy()
    {
        if (dummy == null) return;

        float health = Mathf.Max(1f, float.Parse(healthInput.text));
        dummy.maxHealth = health;

        float armor = Mathf.Max(0f, float.Parse(armorInput.text));
        dummy.armor = armor;

        float magicResist = Mathf.Max(0f, float.Parse(magicResistInput.text));
        dummy.magicResist = magicResist;
    }
}
