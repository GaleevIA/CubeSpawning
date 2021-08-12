using System;
using UniRx;
using UnityEngine;
using System.Linq;

public class GameController : MonoBehaviour
{
    public static GameObject currentCube;

    private Camera camera;

    private void Start()
    {
        camera = Camera.main;

        InitSubscribes();
    }

    private void InitSubscribes()
    {
        //Чтобы определить одинарный или двойной клик был совершен, соберем все клики и высчитаем время между ними
        var clickStream = Observable.EveryUpdate().Select(x => new Tuple<long, Vector3>(x, Input.mousePosition)).Where(_ => Input.GetMouseButtonDown(0));
        var bufferStream = clickStream.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(200))).Publish().RefCount();

        bufferStream.Where(xs => xs.Count == 1)
                    .Subscribe(x => OneClickEvent(x[x.Count - 1].Item2))
                    .AddTo(this);

        bufferStream.Where(xs => xs.Count >= 2)
                    .Subscribe(x => DoubleClickEvent(x[x.Count-1].Item2))
                    .AddTo(this); 
    }

    #region ClickEvents
    //Одинарное нажатие левой кнопки мыши
    private void OneClickEvent(Vector3 point)
    {
        Ray ray = camera.ScreenPointToRay(point);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Поверх других кубов новые кубы не спавнятся
            if (hit.collider.tag == "Cube")
            {
                if (LinkController.linkIsOn)
                    LinkController.OnLinkEnd.OnNext(hit.collider.gameObject);
                else
                    DragController.OnDragStart.OnNext(hit.collider.gameObject);
            }            
            else if(!LinkController.linkIsOn)
                CubesController.OnCubeSpawn.OnNext(hit.point);               
        }
    }

    //Двойное нажатие левой кнопки мыши
    private void DoubleClickEvent(Vector3 point)
    {
        Ray ray = camera.ScreenPointToRay(point);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.tag == "Cube")
                CubesController.OnCubeDestroy.OnNext(hit.collider.gameObject);
        }
    }
    #endregion
}