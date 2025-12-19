using System.Collections.Generic;
using UnityEngine;

public class GroundGenerator2D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject dirtPrefab;
    public GameObject grassPrefab;
    public GameObject itemPrefab;

    [Header("Item Settings")]
    public float itemSpawnChance = 0.05f;   // 5% peluang
    public float itemHeightOffset = 3f;

    [Header("Ground Settings")]
    public int startLevel = 3;
    public int maxLevel = 10;
    public int visibleColumns = 25;
    public float tileWidth = 1f;

    [Header("Gap Settings")]
    public float gapChance = 0.20f;
    public int maxGap = 2; // lebar gap max (dalam kolom)

    [Header("Movement")]
    public float moveSpeed = 3f;

    // Internal state
    private List<List<GameObject>> columns = new List<List<GameObject>>();
    private float nextX = -8f;
    private int currentLevel;

    void Start()
    {
        currentLevel = startLevel;

        // generate awal
        for (int i = 0; i < visibleColumns; i++)
            SpawnNextColumn();
    }

    void Update()
    {
        if (!GameManager.isGameStarted) return;
        MoveColumnsLeft();
        RemoveOffscreenColumns();
        EnsureColumnCount();
    }

    // Gerakkan semua kolom ke kiri
    private void MoveColumnsLeft()
    {
        float delta = moveSpeed * Time.deltaTime;

        foreach (var col in columns)
            foreach (var obj in col)
                if (obj != null)
                {
                    // Ambil posisi saat ini
                    Vector3 p = obj.transform.position;

                    // Ubah nilai sumbu X
                    p.x = p.x - delta;

                    // Set ulang posisi
                    obj.transform.position = p;
                }
    }

    // Hapus kolom yang sudah jauh di luar layar
    private void RemoveOffscreenColumns()
    {
        if (columns.Count == 0) return;

        var firstCol = columns[0];

        // Jika kolom kosong (gap placeholder), cukup hapus list
        if (firstCol.Count == 0)
        {
            columns.RemoveAt(0);
            return;
        }

        // jika kolom paling depan sudah jauh ke kiri (threshold ini bisa Anda sesuaikan)
        if (firstCol[0].transform.position.x < -20f)
        {
            // Destroy semua tile di kolom
            foreach (var obj in firstCol)
                Destroy(obj);

            columns.RemoveAt(0);
        }
    }

    // Pastikan jumlah kolom selalu = visibleColumns
    private void EnsureColumnCount()
    {
        while (columns.Count < visibleColumns)
            SpawnNextColumn();
    }

    // Spawn kolom berikutnya
    private void SpawnNextColumn()
    {
        // 1) Tentukan spawnX dari kolom terakhir
        float spawnX;
        if (columns.Count > 0 && columns[^1].Count > 0 && columns[^1][0] != null)
        {
            spawnX = columns[^1][0].transform.position.x + tileWidth;   // kolom terakhir berisi tanah
        }
        else
        {
            spawnX = nextX;     // kolom terakhir adalah gap / belum ada
        }

        // 2) Gap?
        if (Random.value <= gapChance)
        {
            int gapWidth = Random.Range(1, maxGap + 1);

            // tambah placeholder gap
            for (int i = 0; i < gapWidth; i++)
                columns.Add(new List<GameObject>());

            spawnX += gapWidth * tileWidth;
        }

        // 3) normal: spawn ground di spawnX
        List<GameObject> newCol = new List<GameObject>();

        // Tinggi tanah naik/turun halus
        currentLevel = Mathf.Clamp(currentLevel + Random.Range(-1, 2), 1, maxLevel);

        // Spawn dirt dari y = -5 sampai y < currentLevel
        for (int y = -5; y < currentLevel; y++)
        {
            var dirt = Instantiate(dirtPrefab, new Vector2(spawnX, y), Quaternion.identity);
            newCol.Add(dirt);
        }

        // Spawn grass tepat di puncak
        var grass = Instantiate(grassPrefab, new Vector2(spawnX, currentLevel), Quaternion.identity);                       //Quaternion.Euler(0f, 0f, 90f)
        newCol.Add(grass);

        // Coba spawn item
        TrySpawnItem(newCol, spawnX, currentLevel);

        // Masukkan kolom baru (ground) ke daftar
        columns.Add(newCol);

        // Update nextX agar selalu menunjuk ke slot berikutnya setelah ground baru ini
        nextX = spawnX + tileWidth;
    }

    // Spawn item random (satu jenis item)
    private void TrySpawnItem(List<GameObject> col, float x, int groundY)
    {
        if (itemPrefab == null) return;                 // hanya cek 1 prefab
        if (Random.value > itemSpawnChance) return;     // peluang spawn item

        var item = Instantiate(itemPrefab, new Vector2(x, groundY + itemHeightOffset), Quaternion.identity);
        col.Add(item);
    }
}
