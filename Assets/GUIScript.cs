using UnityEngine;
using UnityEngine.UI;

public class ChampionHUD : MonoBehaviour
{
    [Header("References to Champion")]
    public ChampionMelee champion;

    [Header("UI Elements")]
    public Image hpBar;
    public Image manaBar;

    public Image qIcon;
    public Image wIcon;
    public Image eIcon;
    public Image rIcon;

    public Text qCooldownText;
    public Text wCooldownText;
    public Text eCooldownText;
    public Text rCooldownText;
    public Text hpText;
    public Text manaText;

    [Header("Ability Description Panel")]
    public GameObject abilityDescriptionPanel;  // <-- tutaj w Inspectorze pod³¹cz swój nowy Panel

    void Start()
    {
       if (champion == null)
        {
            champion = FindFirstObjectByType<ChampionMelee>();
        }
    }

    void Update()
    {
        if (champion == null) return;

        // --- HP i mana ---
        float hpFraction = Mathf.Clamp01(champion.GetCurrentHP() / champion.GetMaxHP());
        float manaFraction = Mathf.Clamp01(champion.GetCurrentMana() / champion.GetMaxMana());

        hpBar.fillAmount = Mathf.Clamp01(champion.GetCurrentHP() / champion.GetMaxHP());
        manaBar.fillAmount = Mathf.Clamp01(champion.GetCurrentMana() / champion.GetMaxMana());

        if (hpText != null)
            hpText.text = $"{champion.GetCurrentHP():0} / {champion.GetMaxHP():0}";
        if (manaText != null)
            manaText.text = $"{champion.GetCurrentMana():0} / {champion.GetMaxMana():0}";

        // --- Cooldowny abilit ---
        UpdateCooldownUI(qIcon, qCooldownText, champion.qTimer, champion.qCooldown);
        UpdateCooldownUI(wIcon, wCooldownText, champion.wTimer, champion.wCooldown);
        UpdateCooldownUI(eIcon, eCooldownText, champion.eTimer, champion.eCooldown);
        UpdateCooldownUI(rIcon, rCooldownText, champion.rTimer, champion.rCooldown);

        // --- Panel opisów umiejêtnoœci ---
        if (abilityDescriptionPanel != null)
        {
            // pokazuj panel tylko, gdy przytrzymany C
            abilityDescriptionPanel.SetActive(Input.GetKey(KeyCode.C));
        }
    }

    void UpdateCooldownUI(Image icon, Text cooldownText, float timer, float maxCooldown)
    {
        float fill = 1f - Mathf.Clamp01(timer / maxCooldown);
        icon.fillAmount = fill;

        if (timer > 0f)
        {
            cooldownText.text = Mathf.Ceil(timer).ToString();
            cooldownText.enabled = true;
        }
        else
        {
            cooldownText.enabled = false;
        }
    }
}
