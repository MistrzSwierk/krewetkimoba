using UnityEngine;
using UnityEngine.UI;

public class ChampionStatsUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject panel;

    [Header("UI Elements")]
    public InputField levelInput;
    public InputField baseADInput;
    public InputField baseAPInput;
    public InputField attackSpeedInput;
    public InputField attackRangeInput;

    // Nowe pola
    public InputField armorInput;
    public InputField physicalPenInput;
    public InputField magicPenInput;

    [Header("Champion Reference")]
    public ChampionMelee champion;

    void Start()
    {
        panel.SetActive(false);
        UpdateUIFromChampion();
        if (champion == null)
        {
            champion = FindFirstObjectByType<ChampionMelee>();
        }
    }

    void Update()
    {
        // Shift+F4 otwiera/zamyka panel
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F4))
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) UpdateUIFromChampion();
        }

        if (panel.activeSelf)
        {
            ApplyUIToChampion();
        }
    }

    void UpdateUIFromChampion()
    {
        if (champion == null) return;

        levelInput.text = champion.level.ToString();
        baseADInput.text = champion.baseAD.ToString();
        baseAPInput.text = (champion.baseAPPercent * 100f).ToString(); // w %
        attackSpeedInput.text = champion.attackSpeed.ToString();
        attackRangeInput.text = champion.attackRange.ToString();

        // Nowe pola
        armorInput.text = champion.armor.ToString();
        physicalPenInput.text = champion.physicalPenetration.ToString();
        magicPenInput.text = champion.magicPenetration.ToString();
    }

    void ApplyUIToChampion()
    {
        if (champion == null) return;

        int lvl = Mathf.Clamp(int.Parse(levelInput.text), 1, 18);
        champion.level = lvl;

        float ad = float.Parse(baseADInput.text);
        champion.baseAD = Mathf.Max(0f, ad);

        float ap = float.Parse(baseAPInput.text) / 100f; // wprowadzamy w %
        champion.baseAPPercent = Mathf.Max(0f, ap);

        float atkSpd = float.Parse(attackSpeedInput.text);
        champion.attackSpeed = Mathf.Max(0.1f, atkSpd);

        float range = float.Parse(attackRangeInput.text);
        champion.attackRange = Mathf.Max(0f, range);

        // Nowe pola
        float armorVal = float.Parse(armorInput.text);
        champion.armor = Mathf.Max(0f, armorVal);

        float physPen = float.Parse(physicalPenInput.text);
        champion.physicalPenetration = Mathf.Max(0f, physPen);

        float magicPen = float.Parse(magicPenInput.text);
        champion.magicPenetration = Mathf.Max(0f, magicPen);
    }
}