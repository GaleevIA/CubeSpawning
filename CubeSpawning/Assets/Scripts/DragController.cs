using System.Threading.Tasks;
using UniRx;
using UnityEngine;

//Данный класс отвечает за перетаскивание объектов
public class DragController : MonoBehaviour
{
    public static Subject<GameObject> OnDragStart = new Subject<GameObject>();

    private bool dragIsOn;
    private Camera camera;

    private void Start()
    {
        camera = Camera.main;

        InitSubscribes();
    }

    private void InitSubscribes()
    {
        OnDragStart.Subscribe(x => DragStart(x));

        //При отпускании левой кнопки прекращаем перетаскивание
        Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonUp(0))
                .Subscribe(_ => DragEnd(true))
                .AddTo(this);

        //При зажатой левой кнопке мыши выполняем перетаскивание
        Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButton(0))
                .Select(_ => Input.mousePosition)
                .Subscribe(x => Drag(x))
                .AddTo(this);
    }

    //Перетаскивание объекта
    private void Drag(Vector3 point)
    {
        if (!dragIsOn) return;

        Ray ray = camera.ScreenPointToRay(point);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameController.currentCube.transform.position = new Vector3(hit.point.x, GameController.currentCube.transform.position.y, hit.point.z);
        }
    }

    //Начало перетаскивания объекта
    private void DragStart(GameObject cube)
    {
        if (!dragIsOn)
        {
            dragIsOn = true;
            GameController.currentCube = cube;
        }
    }

    //Окончание перетаскивания объекта
    private async void DragEnd(bool isEnd)
    {
        //Приходится делать небольшую задержку, чтобы окончание не срабатывало раньше начала
        await Task.Delay(200);

        if (dragIsOn)
        {
            dragIsOn = false;
            GameController.currentCube = null;
        }
    }
}
