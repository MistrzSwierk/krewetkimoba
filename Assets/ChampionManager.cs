using UnityEngine;

public class ChampionSwitcher : MonoBehaviour
{
    [Header("Champ List")]
    public GameObject[] championsPrefabs;
    private int currentChampionIndex = 0;

    [Header("Camera")]
    public CameraFollow_LoL cameraFollow;
    public ChampionHUD hud;
    private GameObject currentChampion;

    void Start()
    {
        if (championsPrefabs.Length > 0)
            SpawnChampion(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8) && Input.GetKey(KeyCode.LeftShift))
        {
            int nextIndex = (currentChampionIndex + 1) % championsPrefabs.Length;
            SpawnChampion(nextIndex);
        }
    }

    void SpawnChampion(int index)
    {
        if (currentChampion != null)
        {
            Destroy(currentChampion);
        }
        currentChampion = Instantiate(championsPrefabs[index], Vector3.zero, Quaternion.identity);
        currentChampionIndex = index;

        if (cameraFollow != null)
        {
            cameraFollow.target = currentChampion.transform;
        }

        if (hud != null)
        {
            hud.champion = currentChampion.GetComponent<ChampionMelee>();
        }
    }
}
