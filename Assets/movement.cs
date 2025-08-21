using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ChampionMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public RectTransform uiPanel; // przypisz Panel w Inspectorze

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("Brak NavMeshAgent na obiekcie " + gameObject.name);
    }

    void Update()
    {
        if (agent == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Sprawdzenie, czy kursor jest nad panelem
            if (IsPointerOverPanel())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
                agent.SetDestination(hit.point);
        }
    }

    bool IsPointerOverPanel()
    {
        Vector2 mousePos = Input.mousePosition;
        return RectTransformUtility.RectangleContainsScreenPoint(uiPanel, mousePos);
    }
}
