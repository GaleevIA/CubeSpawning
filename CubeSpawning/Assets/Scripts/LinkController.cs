using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

public class LinkController : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject trailPrefab;

    public static Subject<GameObject> OnLinkEnd = new Subject<GameObject>();
    public static Subject<GameObject> OnLinkDelete = new Subject<GameObject>();
    public static Subject<GameObject> OnLinesUpdate = new Subject<GameObject>();

    public static bool linkIsOn;

    //В данном списке будут храниться связи между объектами и визуальная линия
    //Первый и второй GO это связанные объекты
    //Третий GO это визуальная линия
    private List<Tuple<GameObject, GameObject, GameObject>> linkedCubes;
    private GameObject currentTrail;
    private Camera camera;
    
    private void Start()
    {
        camera = Camera.main;
        linkedCubes = new List<Tuple<GameObject, GameObject, GameObject>>();

        InitSubscribes();
    }

    private void InitSubscribes()
    {
        OnLinkEnd.Subscribe(x => LinkEnd(x)).AddTo(this);
        OnLinkDelete.Subscribe(x => DeleteLink(x)).AddTo(this);
        OnLinesUpdate.Subscribe(x => UpdateLines(x)).AddTo(this);

        //При нажатии правой кнопки мыши стартует событие связывания
        Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(1))
                .Select(x => Input.mousePosition)
                .Subscribe(x => LinkStart(x))
                .AddTo(this);

        //При изменении положения курсора отрисовывается шлейф
        Observable.EveryLateUpdate()
                .Select(_ => Input.mousePosition)
                .Subscribe(x => LinkSearch(x))
                .AddTo(this);
    }

    #region LinkEvents
    //Начало связывания объектов
    private void LinkStart(Vector3 point)
    {
        //При повторном щелчке правой кнопкой мыши, процесс связывания отменяется
        if (linkIsOn)
        {
            LinkCancel();
        }
        else
        {
            Ray ray = camera.ScreenPointToRay(point);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.tag == "Cube")
                {
                    linkIsOn = true;
                    GameController.currentCube = hit.collider.gameObject;

                    //Создаем небольшой шлейф, следующий за курсором мыши
                    SpawnTrail(hit.point);
                }
            }
        }
    }

    //Окончание связывания объектов
    private void LinkEnd(GameObject cube)
    {
        //Уничтожаем шлейф
        DeleteTrail();

        //Создаем непосредственную связь между объектами
        CreateLink(GameController.currentCube, cube);

        linkIsOn = false;
        GameController.currentCube = null;
    }

    //Поиск объекта для связи
    private void LinkSearch(Vector3 point)
    {
        //Пока связь не закончилась обновляем позицию шлейфа
        if (linkIsOn)
            UpdateTrail(point);
    }

    //Создание связи между объектами
    private void CreateLink(GameObject cube1, GameObject cube2)
    {
        if (cube1 == cube2) return;

        if (linkedCubes.Any(x => x.Item1 == cube1 && x.Item2 == cube2 || x.Item1 == cube2 && x.Item2 == cube1))
            return;

        //Создаем визуальную линию между объектами
        var line = SpawnLine(cube1.transform.position, cube2.transform.position);

        linkedCubes.Add(new Tuple<GameObject, GameObject, GameObject>(cube1, cube2, line));
    }

    //Уничтожение связи между объектами
    private void DeleteLink(GameObject cube)
    {
        var currentLinks = linkedCubes.Where(x => x.Item1 == cube || x.Item2 == cube).ToList<Tuple<GameObject, GameObject, GameObject>>();

        for (int i = 0; i < currentLinks.Count(); i++)
        {
            //Уничтожаем визуальную линию между объектами
            DestroyLine(currentLinks[i].Item3);

            linkedCubes.Remove(currentLinks[i]);
        }
    }

    //Отмена процесса создания связи
    private void LinkCancel()
    {
        linkIsOn = false;

        DeleteTrail();
    }
    #endregion

    #region LineEvents
    //Создание визуальной линии
    private GameObject SpawnLine(Vector3 startPos, Vector3 endPos)
    {
        var line = Instantiate(linePrefab, transform);

        var lineRenderer = line.GetComponent<LineRenderer>();

        lineRenderer.SetPositions(new Vector3[] { startPos, endPos });

        return line;
    }

    //Обновлении линии в связи с изменением позиции объектов
    private void UpdateLines(GameObject cube)
    {
        var lines = linkedCubes.Where(x => x.Item1 == cube || x.Item2 == cube);

        foreach (var line in lines)
            line.Item3.GetComponent<LineRenderer>().SetPositions(new Vector3[] { line.Item1.transform.position, line.Item2.transform.position });
    }

    //Уничтожение визуальной линии
    private void DestroyLine(GameObject line)
    {
        Destroy(line);
    }
    #endregion

    #region TrailEvents
    //Создание шлейфа
    private void SpawnTrail(Vector3 point)
    {
        currentTrail = Instantiate(trailPrefab, point, Quaternion.identity);
    }

    //Обновление позиции шлейфа
    private void UpdateTrail(Vector3 point)
    {
        currentTrail.transform.position = camera.ScreenToWorldPoint(new Vector3(point.x, point.y, 5));
    }

    //Удаление шлейфа
    private void DeleteTrail()
    {
        Destroy(currentTrail);
    }
    #endregion
}
