using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Renderer))]
public class TrainingDummy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Defenses")]
    public float armor = 20f;
    public float magicResist = 15f;

    private Renderer rend;
    private Color originalColor;
    private bool highlighted = false;

    [Header("Passive")]
    public float passiveDuration = 3f;
    private float passiveTimer = 0f;

    // -------------------- DPS -------------------
    private float damageAccum = 0f;
    private float dpsTimer = 0f;
    public float dpsUpdateInterval = 1f; // aktualizacja DPS co 1 sekundę
    private float damagePerSecond = 0f;
    private TextMesh dpsText;

    void Awake()
    {
        currentHealth = maxHealth;
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;

        // Tworzymy TextMesh pod dummy
        GameObject textObj = new GameObject("DPS_Display");
        textObj.transform.parent = transform;
        textObj.transform.localPosition = new Vector3(2f, 0f, 0f); // pod dummy
        dpsText = textObj.AddComponent<TextMesh>();
        dpsText.fontSize = 32;
        dpsText.color = Color.black;
        dpsText.alignment = TextAlignment.Center;
        dpsText.anchor = TextAnchor.MiddleCenter;
    }

    void Update()
    {
        if (passiveTimer > 0f)
            passiveTimer -= Time.deltaTime;
        else if (currentHealth < maxHealth)
        {
            currentHealth = maxHealth;
            Debug.Log($"{gameObject.name} zregenerował się do pełnego zdrowia!");
        }

        // ---------------- DPS ----------------
        dpsTimer += Time.deltaTime;
        if (dpsTimer >= dpsUpdateInterval)
        {
            damagePerSecond = damageAccum / dpsTimer;
            dpsText.text = Mathf.RoundToInt(damagePerSecond).ToString();

            damageAccum = 0f; // resetujemy obrażenia
            dpsTimer = 0f;    // resetujemy timer
        }
    }

    // -------------------- DAMAGE CALCULATIONS --------------------
    public float GetPhysicalDamageTaken(float amount, float flatPen = 0f, float percentPen = 0f)
    {
        float effectiveArmor = Mathf.Max(0f, armor * (1f - percentPen) - flatPen);
        return amount * 100f / (100f + effectiveArmor);
    }

    public float GetMagicDamageTaken(float amount, float flatPen = 0f, float percentPen = 0f)
    {
        float effectiveMR = Mathf.Max(0f, magicResist * (1f - percentPen) - flatPen);
        return amount * 100f / (100f + effectiveMR);
    }

    public void TakePhysicalDamage(float amount, float flatPen = 0f, float percentPen = 0f)
    {
        float dmg = GetPhysicalDamageTaken(amount, flatPen, percentPen);
        ApplyDamage(dmg);
    }

    public void TakeMagicDamage(float amount, float flatPen = 0f, float percentPen = 0f)
    {
        float dmg = GetMagicDamageTaken(amount, flatPen, percentPen);
        ApplyDamage(dmg);
    }

    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        damageAccum += damage;

        Debug.Log($"{gameObject.name} took {damage:F1} damage! Current HP: {currentHealth:F1}");

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Debug.Log($"{gameObject.name} died!");
        }

        RegisterPassiveHit();
    }

    // -------------------- HIGHLIGHT --------------------
    public void Highlight(bool enable)
    {
        highlighted = enable;
        rend.material.color = highlighted ? Color.yellow : originalColor;
    }

    // -------------------- PASSIVE --------------------
    public void RegisterPassiveHit()
    {
        passiveTimer = passiveDuration;
    }

    public bool HasPassiveHit()
    {
        return passiveTimer > 0f;
    }
}
