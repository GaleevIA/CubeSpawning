using UniRx;
using UnityEngine;

public class CubesController : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private LayerMask layerMask;

    public static Subject<Vector3> OnCubeSpawn = new Subject<Vector3>();
    public static Subject<GameObject> OnCubeDestroy = new Subject<GameObject>();

    private void Start()
    {
        InitSubscribes();
    }

    private void InitSubscribes()
    {
        OnCubeSpawn.Subscribe(x => SpawnCube(x)).AddTo(this);
        OnCubeDestroy.Subscribe(x => DestroyCube(x)).AddTo(this);
    }

    //Создание прямоугольника
    private void SpawnCube(Vector3 point)
    {
        var spawnPoint = point + new Vector3(0, 0.5f, 0);

        if (!FreeSpaceCheck(spawnPoint)) return;

        var cube = Instantiate(cubePrefab, spawnPoint, Quaternion.identity);

        cube.GetComponent<Renderer>().material.SetColor("_Color", GetRandomColor());

        //При изменении позиции прямоугольника нужно обновить все линии, связанные с ним
        cube.transform.ObserveEveryValueChanged(x => x.position).Subscribe(_ => LinkController.OnLinesUpdate.OnNext(cube));
    }

    //Уничтожение прямоугольника
    private void DestroyCube(GameObject cube)
    {
        LinkController.OnLinkDelete.OnNext(cube);

        Destroy(cube);
    }

    //Генерация случайного цвета
    private Color GetRandomColor()
    {
        var r = UnityEngine.Random.Range(0.001f, 1);
        var g = UnityEngine.Random.Range(0.001f, 1);
        var b = UnityEngine.Random.Range(0.001f, 1);

        return new Color(r, g, b, 1);
    }

    //Проверяет умещается ли куб в заданной зоне
    private bool FreeSpaceCheck(Vector3 point)
    {
        return Physics.OverlapBox(point, cubePrefab.transform.localScale / 2, Quaternion.identity, layerMask).Length == 0;
    }
}
