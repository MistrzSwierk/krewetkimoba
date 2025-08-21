using UnityEngine;
using UnityEngine.AI;

public class ChampionMelee : MonoBehaviour
{
    [Header("Stats")]
    public int level = 1; // 1-18
    public float baseAD = 20f;
    public float baseAPPercent = 0.4f; // 40%AD
    public float attackSpeed = 1f; // ataki na sekundê
    public float attackRange = 2f;
    public float maxHealth = 500f;
    public float currentHealth;
    public float movementSpeed = 8f;
    private float baseMovementSpeed;

    [Header("Additional Stats")]
    public float armor = 0f;
    public float physicalPenetration = 0f; // flat penetration
    public float magicPenetration = 0f; // flat penetration

    [Header("Combat")]
    public float attackCooldown = 0f;
    private TrainingDummy currentTarget;
    private NavMeshAgent agent;

    [Header("Mana & Leapy")]
    public float maxMana;
    public float currentMana;
    public float manaRegen = 1f;
    public float qCooldown = 5f;
    public float qTimer = 0f;
    public float qRange = 5f;
    public float qManaCost = 50f;

    [Header("W – Dark Stance")]
    public float wManaCost = 45f;
    public float wCooldown = 10f;
    public float wTimer = 0f;
    public float wADBonusPercent = 0.1f; // 10% bonus AD
    private bool wActive = false;
    private float scaledWADBonusPercent;
    private float scaledWManaCost;

    [Header("E – Blade Jump")]
    public float eManaCost = 60f;
    public float eCooldown = 12f;
    public float eTimer = 0f;
    public float eJumpRange = 8f; // maksymalny zasiêg skoku
    public float eADBase = 140f;
    public float eAPPercent = 0.2f; // 20% AD zamienione w AP
    private bool eRecentlyLanded = false;
    private float eLandingWindow = 2f;

    [Header("R – Heavenly Judgment (Ultimate)")]
    public float rManaCost = 100f;
    public float rCooldown = 80f;
    public float rTimer = 0f;
    private bool rActive = false;
    private float rDuration = 8f;
    private float rAttackSpeedBonusPercent = 0.5f; // +50% attack speed
    private float rADMultiplier = 2.5f; // 250% AD
    private float rAPMultiplier = 1.5f; // 150% AD -> AP

    [Header("Q Range Visualization")]
    public int circleSegments = 50;
    private LineRenderer qLineRenderer;
    private bool showQRange = false;
    private float qHoldTime = 0f;

    private int previousLevel;

    void Awake()
    {
        baseMovementSpeed = movementSpeed;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
        currentMana = GetMaxMana();
        maxMana = GetMaxMana();
        previousLevel = level;
        currentHealth = maxHealth;
        UpdateMaxMana();
        UpdateManaRegen();
        UpdateWScaling();

        GameObject qCircle = new GameObject("QRangeCircle");
        qCircle.transform.parent = this.transform;
        qCircle.transform.localPosition = Vector3.zero;

        qLineRenderer = qCircle.AddComponent<LineRenderer>();
        qLineRenderer.useWorldSpace = false;
        qLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        qLineRenderer.startColor = Color.magenta;
        qLineRenderer.endColor = Color.magenta;
        qLineRenderer.startWidth = 0.05f;
        qLineRenderer.endWidth = 0.05f;
        qLineRenderer.positionCount = circleSegments + 1;
        qLineRenderer.enabled = false;
    }

    void Update()
    {
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
        if (qTimer > 0f) qTimer -= Time.deltaTime;
        if (wTimer > 0f) wTimer -= Time.deltaTime;
        if (eTimer > 0f) eTimer -= Time.deltaTime;
        if (rTimer > 0f) rTimer -= Time.deltaTime;

        if (level != previousLevel)
        {
            UpdateMaxMana();
            UpdateManaRegen();
            UpdateWScaling();
            previousLevel = level;
        }

        currentMana += manaRegen * Time.deltaTime;
        currentMana = Mathf.Min(GetCurrentMana(), GetMaxMana());

        HandleMovement();
        HandleTargetSelection();
        HandleQRangeVisualization();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                agent.isStopped = true;
                AttackTarget();
            }
        }

        // --- Q ---
        if (Input.GetKeyDown(KeyCode.Q) && currentMana >= qManaCost && qTimer <= 0f)
            qHoldTime = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            qHoldTime += Time.deltaTime;
            if (qHoldTime >= 0.1f) showQRange = true;
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (qHoldTime < 0.1f)
                UseQNearestTarget();
            showQRange = false;
            qLineRenderer.enabled = false;
            qHoldTime = 0f;
        }

        // --- W ---
        if (Input.GetKeyDown(KeyCode.W) && GetCurrentMana() >= scaledWManaCost && wTimer <= 0f)
        {
            ActivateW();
        }

        // --- E ---
        if (Input.GetKeyDown(KeyCode.E) && GetCurrentMana() >= eManaCost && eTimer <= 0f)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TrainingDummy dummy = hit.collider.GetComponent<TrainingDummy>();
                if (dummy != null)
                {
                    float distance = Vector3.Distance(transform.position, dummy.transform.position);
                    if (distance <= 6)
                    {
                        currentTarget = dummy; // ustawiamy cel pod kursorem
                        currentTarget.Highlight(true);
                        UseE();
                    }
                }
            }
        }
        // --- R ---
        if (Input.GetKeyDown(KeyCode.R) && GetCurrentMana() >= rManaCost && rTimer <= 0f)
        {
            ActivateR();
        }
    }


    public float GetMaxHP() => maxHealth;
    public float GetCurrentHP() => currentHealth;
    public float GetMaxMana() => maxMana;
    public float GetCurrentMana() => currentMana;

    public void UpdateMaxMana()
    {
        maxMana = Mathf.RoundToInt(300f * Mathf.Pow(1.08f, level - 1));
        currentMana = Mathf.Min(GetCurrentMana(), GetMaxMana());
    }

    void UpdateManaRegen()
    {
        manaRegen = Mathf.Round(1f * Mathf.Pow(1.17f, level - 1) * 100f) / 100f;
    }

    void UpdateWScaling()
    {
        scaledWADBonusPercent = wADBonusPercent * Mathf.Pow(1.024f, level - 1);
        scaledWManaCost = wManaCost;
    }

    void ActivateW()
    {
        currentMana -= scaledWManaCost;
        wTimer = wCooldown;
        wActive = true;
        Invoke("DeactivateW", 5f);
    }

    void DeactivateW()
    {
        wActive = false;
    }

    void HandleMovement()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.point);

                if (currentTarget != null)
                {
                    currentTarget.Highlight(false);
                    currentTarget = null;
                }
            }
        }
    }

    void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TrainingDummy dummy = hit.collider.GetComponent<TrainingDummy>();
                if (dummy != null)
                {
                    if (currentTarget != null)
                        currentTarget.Highlight(false);

                    currentTarget = dummy;
                    currentTarget.Highlight(true);
                }
            }
        }
    }

    void HandleQRangeVisualization()
    {
        if (showQRange)
        {
            qLineRenderer.enabled = true;
            float deltaTheta = (2f * Mathf.PI) / circleSegments;
            float theta = 0f;

            for (int i = 0; i < circleSegments + 1; i++)
            {
                float x = qRange * Mathf.Cos(theta);
                float z = qRange * Mathf.Sin(theta);
                qLineRenderer.SetPosition(i, new Vector3(x, 0.01f, z));
                theta += deltaTheta;
            }
        }
    }

    void AttackTarget()
    {
        if (attackCooldown > 0f || currentTarget == null) return;

        float ad = baseAD * Mathf.Pow(1.08f, level - 1); // scaling AD x1.15
        float ap = baseAD * baseAPPercent * Mathf.Pow(1.1f, level - 1); // AP nie ruszamy

        if (wActive) ad *= 1f + scaledWADBonusPercent; // W bonus

        if (rActive)
        {
            ad *= rADMultiplier;
            ap = baseAD * rAPMultiplier; // 150% AD jako AP
        }

        float effectiveAD = currentTarget.GetPhysicalDamageTaken(ad, physicalPenetration);
        float effectiveAP = currentTarget.GetMagicDamageTaken(ap, magicPenetration);

        currentTarget.TakePhysicalDamage(ad, physicalPenetration);
        currentTarget.TakeMagicDamage(ap, magicPenetration);

        // Skracanie cooldownu E jeœli trafiono autoattackiem w 2s po skoku
        if (eRecentlyLanded)
        {
            eTimer *= 0.5f;
            eRecentlyLanded = false;
        }

        // Damage splash
        float adXOffset = Random.Range(1.25f, 1.9f);
        float apXOffset = Random.Range(1.25f, 1.9f);
        float adYOffset = Random.Range(0.3f, 1.4f);
        float apYOffset = Random.Range(0.3f, 1.4f);

        Vector3 adOffset = currentTarget.transform.right * adXOffset;
        Vector3 apOffset = -currentTarget.transform.right * apXOffset;

        CreateDamageSplash(currentTarget.transform.position + adOffset, effectiveAD, new Color(1f, 0.5f, 0f), adYOffset);
        CreateDamageSplash(currentTarget.transform.position + apOffset, effectiveAP, Color.blue, apYOffset);

        attackCooldown = 1f / attackSpeed;
    }

    void UseQNearestTarget()
    {
        if (qTimer > 0f || currentMana < qManaCost) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, qRange);
        TrainingDummy nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            TrainingDummy dummy = hit.GetComponent<TrainingDummy>();
            if (dummy != null)
            {
                float dist = Vector3.Distance(transform.position, dummy.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = dummy;
                }
            }
        }

        if (nearest == null) return;

        currentTarget = nearest;
        currentTarget.Highlight(true);

        currentMana -= qManaCost;
        attackCooldown = 0f;
        qTimer = qCooldown;

        Vector3 direction = (nearest.transform.position - transform.position).normalized;
        agent.isStopped = false;
        agent.SetDestination(nearest.transform.position);

        // Damage
        float qDamage = 50f * Mathf.Pow(1.09f, level - 1);
        nearest.TakePhysicalDamage(qDamage, physicalPenetration);
        CreateDamageSplash(nearest.transform.position, qDamage, Color.magenta, 1f);
    }

    void UseE()
    {
        if (currentTarget == null) return;

        // Skok w stronê celu
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        float distance = Mathf.Min(eJumpRange, Vector3.Distance(transform.position, currentTarget.transform.position));
        transform.position += direction * distance;

        // Obra¿enia w promieniu 3
        Collider[] hits = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hit in hits)
        {
            TrainingDummy dummy = hit.GetComponent<TrainingDummy>();
            if (dummy != null)
            {
                float adDamage = eADBase * Mathf.Pow(1.06f, level - 1);
                float apDamage = adDamage * eAPPercent;

                dummy.TakePhysicalDamage(adDamage, physicalPenetration);
                dummy.TakeMagicDamage(apDamage, magicPenetration);

                CreateDamageSplash(dummy.transform.position, adDamage, new Color(1f, 0.5f, 0f), 1f);
                CreateDamageSplash(dummy.transform.position, apDamage, Color.blue, 1.2f);
            }
        }

        currentMana -= eManaCost;
        eTimer = eCooldown;

        // Aktywujemy okno skracania cooldownu przy autoattacku
        eRecentlyLanded = true;
        Invoke("ResetELandingWindow", eLandingWindow);

        // --- NOWOŒÆ: Cancel target po trafieniu E ---
        if (currentTarget != null)
        {
            currentTarget.Highlight(false); // odznacz zaznaczenie
            currentTarget = null;
            agent.isStopped = true; // zatrzymaj poruszanie siê
        }
    }


    void ResetELandingWindow()
    {
        eRecentlyLanded = false;
    }

    void ActivateR()
    {
        currentMana -= rManaCost;
        rTimer = rCooldown;
        rActive = true;

        attackSpeed *= 1f;
        movementSpeed *= 1.5f;
        agent.speed = movementSpeed;

        Invoke("DeactivateR", rDuration);
    }

    void DeactivateR()
    {
        rActive = false;

        attackSpeed /= 1f + rAttackSpeedBonusPercent;
        movementSpeed = baseMovementSpeed;
        agent.speed = movementSpeed;
    }


    void CreateDamageSplash(Vector3 position, float damage, Color color, float yOffset)
    {
        GameObject go = new GameObject("DamageSplash");
        go.transform.position = position + Vector3.up * yOffset;
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = Mathf.RoundToInt(damage).ToString();
        tm.color = color;
        tm.fontSize = 32;
        Destroy(go, 0.7f);
    }

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
