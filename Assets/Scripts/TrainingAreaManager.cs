using UnityEngine;

public class TrainingAreaManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject catPrefab;
    public int count = 16;
    public float spacing = 10f;

    private void Start()
    {
        SpawnCats();
    }

    private void SpawnCats()
    {
        if (catPrefab == null)
        {
            Debug.LogError("TrainingAreaManager: catPrefab is not assigned.");
            return;
        }

        int rows = Mathf.CeilToInt(count / 4f);
        int cols = 4;

        int spawned = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (spawned >= count) break;

                Vector3 pos = new Vector3(c * spacing, 0, r * spacing);
                GameObject inst = Instantiate(catPrefab, pos, Quaternion.identity, transform);
                inst.name = $"Cat_{r}_{c}";

                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned} cats.");
    }
}
