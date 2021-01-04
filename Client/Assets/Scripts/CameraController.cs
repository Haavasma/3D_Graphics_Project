using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the main camera given scroll wheel and mousebutton 2 inputs
public class CameraController : MonoBehaviour
{
    private Transform focus;

    [SerializeField] float defaultCamDistance = 5.0f;
    private float lookSpeed = 3.0f;

    public float lookXLimit = 60.0f;

    public float speed = 5.0f;

    public float zoomSpeed = 2.0f;

    private float scrollValue;

    private float minScroll = -9.0f;
    private float maxScroll = 20.0f;

    Vector2 rotation = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        focus = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (scrollValue < maxScroll && Input.mouseScrollDelta.y < 0)
        {
            scrollValue -= Input.mouseScrollDelta.y;
        }
        else if (scrollValue > minScroll && Input.mouseScrollDelta.y > 0)
        {
            scrollValue -= Input.mouseScrollDelta.y;
        }
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += +Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            focus.eulerAngles = new Vector2(-rotation.x, rotation.y);
        }

        if (Input.GetMouseButtonUp(1))
        {
            Cursor.visible = true;
        }

        float step = 100f * Time.deltaTime;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(0.0f, 0.0f, (-defaultCamDistance - (scrollValue * 0.5f))),
                                                        step * zoomSpeed);
    }

    void OnMouseDown()
    {
    }
}
