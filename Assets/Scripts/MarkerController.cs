using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    public delegate void OnMarkerClicked(int xPos, int yPos, string piece);
    public static event OnMarkerClicked OnMarkerClickedAction;

    private void OnEnable()
    {
        PieceController.OnFinishedChoosingAction += OnFinishedChoosingAction;
    }

    private void OnDisable()
    {
        PieceController.OnFinishedChoosingAction -= OnFinishedChoosingAction;
    }

    private void OnFinishedChoosingAction()
    { 
        GameManager._instance.isChoosing = false;
        Destroy(gameObject);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePosition2D, Vector2.zero);

            if (hit.transform.name == name)
            {
                Vector3 position = gameObject.transform.position;
                int xPos = (int) position.x;
                int yPos = (int) position.y;
                
                OnMarkerClickedAction?.Invoke(xPos, yPos, tag);
            }
        }
    }
}
