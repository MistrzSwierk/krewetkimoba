using UnityEngine;
using UnityEngine.UI;

public class DummyHealthBar : MonoBehaviour
{
    public TrainingDummy dummy; // referencja do dummy
    public Canvas canvas;       // canvas w world space
    public Image healthFill;    // obraz wype³nienia

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
        }
        if (healthFill == null)
        {
            healthFill = canvas.GetComponentInChildren<Image>();
        }
    }

    void Update()
    {
        if (dummy == null) return;

        // ustawienie healthbara nad dummy
        Vector3 worldPos = dummy.transform.position + Vector3.up * 2f; // 2 jednostki nad ziemi¹
        canvas.transform.position = worldPos;

        // ustawienie healthbara tak, aby zawsze patrzy³ na kamerê
        canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - mainCamera.transform.position);

        // aktualizacja wype³nienia paska
        healthFill.fillAmount = dummy.currentHealth / dummy.maxHealth;
    }
}
