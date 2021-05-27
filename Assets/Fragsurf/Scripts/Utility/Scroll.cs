using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Scroll : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    private Camera mainCamera;
    private RectTransform canvasRect;
    public RectTransform viewport;
    public RectTransform content;
    private Rect viewportOld;
    private Rect contentOld;

    private List<Vector2> dragCoordinates = new List<Vector2>();
    private List<float> offsets = new List<float>();
    private int offsetsAveraged = 4;
    private float offset;
    private float velocity = 0;
    private bool changesMade = false;

    public float decelration = 0.135f;
    public float scrollSensitivity;
    public OnValueChanged onValueChanged;


    [System.Serializable]
    public class OnValueChanged : UnityEvent { }

    [HideInInspector]
    public float verticalNormalizedPosition
    {
        get
        {
            float sizeDelta = CaculateDeltaSize();
            if (sizeDelta == 0)
            {
                return 0;
            }
            else
            {
                return 1 - content.transform.localPosition.y / sizeDelta;
            }
        }
        set
        {
            float o_verticalNormalizedPosition = verticalNormalizedPosition;
            float m_verticalNormalizedPosition = Mathf.Max(0, Mathf.Min(1, value));
            float maxY = CaculateDeltaSize();
            content.transform.localPosition = new Vector3(content.transform.localPosition.x, Mathf.Max(0, (1 - m_verticalNormalizedPosition) * maxY), content.transform.localPosition.z);
            float n_verticalNormalizedPosition = verticalNormalizedPosition;
            if (o_verticalNormalizedPosition != n_verticalNormalizedPosition)
            {
                onValueChanged.Invoke();
            }
        }
    }

    private float CaculateDeltaSize()
    {
        return Mathf.Max(0, content.rect.height - viewport.rect.height); ;
    }


    private void Awake()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        canvasRect = transform.root.GetComponent<RectTransform>();
    }

    private Vector2 ConvertEventDataDrag(PointerEventData eventData)
    {
        return new Vector2(eventData.position.x / mainCamera.pixelWidth * canvasRect.rect.width, eventData.position.y / mainCamera.pixelHeight * canvasRect.rect.height);
    }

    private Vector2 ConvertEventDataScroll(PointerEventData eventData)
    {
        return new Vector2(eventData.scrollDelta.x / mainCamera.pixelWidth * canvasRect.rect.width, eventData.scrollDelta.y / mainCamera.pixelHeight * canvasRect.rect.height) * scrollSensitivity;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        velocity = 0;
        dragCoordinates.Clear();
        offsets.Clear();
        dragCoordinates.Add(ConvertEventDataDrag(eventData));
    }

    public void OnScroll(PointerEventData eventData)
    {
        UpdateOffsetsScroll(ConvertEventDataScroll(eventData));
        OffsetContent(offsets[offsets.Count - 1]);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragCoordinates.Add(ConvertEventDataDrag(eventData));
        UpdateOffsetsDrag();
        OffsetContent(offsets[offsets.Count - 1]);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragCoordinates.Add(ConvertEventDataDrag(eventData));
        UpdateOffsetsDrag();
        OffsetContent(offsets[offsets.Count - 1]);
        float totalOffsets = 0;
        foreach (float offset in offsets)
        {
            totalOffsets += offset;
        }
        velocity = totalOffsets / offsetsAveraged;
        dragCoordinates.Clear();
        offsets.Clear();
    }

    private void OffsetContent(float givenOffset)
    {
        float newY = Mathf.Max(0, Mathf.Min(CaculateDeltaSize(), content.transform.localPosition.y + givenOffset));
        if (content.transform.localPosition.y != newY)
        {
            content.transform.localPosition = new Vector3(content.transform.localPosition.x, newY, content.transform.localPosition.z);
        }
        onValueChanged.Invoke();
    }

    private void UpdateOffsetsDrag()
    {
        offsets.Add(dragCoordinates[dragCoordinates.Count - 1].y - dragCoordinates[dragCoordinates.Count - 2].y);
        if (offsets.Count > offsetsAveraged)
        {
            offsets.RemoveAt(0);
        }
    }

    private void UpdateOffsetsScroll(Vector2 givenScrollDelta)
    {
        offsets.Add(givenScrollDelta.y);
        if (offsets.Count > offsetsAveraged)
        {
            offsets.RemoveAt(0);
        }
    }

    private void LateUpdate()
    {
        if (viewport.rect != viewportOld)
        {
            changesMade = true;
            viewportOld = new Rect(viewport.rect);
        }
        if (content.rect != contentOld)
        {
            changesMade = true;
            contentOld = new Rect(content.rect);
        }
        if (velocity != 0)
        {
            changesMade = true;
            velocity = (velocity / Mathf.Abs(velocity)) * Mathf.FloorToInt(Mathf.Abs(velocity) * (1 - decelration));
            offset = velocity;
        }
        if (changesMade)
        {
            OffsetContent(offset);
            changesMade = false;
            offset = 0;
        }
    }
}