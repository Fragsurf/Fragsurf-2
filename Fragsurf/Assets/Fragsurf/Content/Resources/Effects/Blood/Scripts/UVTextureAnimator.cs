using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

internal class UVTextureAnimator : MonoBehaviour
{
    public int Rows = 4;
    public int Columns = 4;
    public float Fps = 20;
    public int OffsetMat = 0;
    public bool IsLoop = true;
    public float StartDelay = 0;

    private bool isInizialised;
    private int index;
    private int count, allCount;
    private float deltaFps;
    private bool isVisible;
    private bool isCorutineStarted;
    private Renderer currentRenderer;
    private Material instanceMaterial;

    #region Non-public methods

    private void Start()
    {
        currentRenderer = GetComponent<Renderer>();
        InitDefaultVariables();
        isInizialised = true;
        isVisible = true;
        Play();
    }

    private void InitDefaultVariables()
    {
        currentRenderer = GetComponent<Renderer>();
        if (currentRenderer == null)
            throw new Exception("UvTextureAnimator can't get renderer");
        if (!currentRenderer.enabled) currentRenderer.enabled = true;
        allCount = 0;
        deltaFps = 1f / Fps;
        count = Rows * Columns;
        index = Columns - 1;
        var offset = Vector3.zero;
        OffsetMat = OffsetMat - (OffsetMat / count) * count;
        var size = new Vector2(1f / Columns, 1f / Rows);

        if (currentRenderer != null)
        {
            instanceMaterial = currentRenderer.material;
            instanceMaterial.SetTextureScale("_MainTex", size);
            instanceMaterial.SetTextureOffset("_MainTex", offset);
        }
    }

    private void Play()
    {
        if (isCorutineStarted) return;
        if (StartDelay > 0.0001f) Invoke("PlayDelay", StartDelay);
        else StartCoroutine(UpdateCorutine());
        isCorutineStarted = true;
    }

    private void PlayDelay()
    {
        StartCoroutine(UpdateCorutine());
    }

    #region CorutineCode

    private void OnEnable()
    {
        if (!isInizialised)
            return;
        InitDefaultVariables();
        isVisible = true;
        Play();
    }

    private void OnDisable()
    {
        isCorutineStarted = false;
        isVisible = false;
        StopAllCoroutines();
        CancelInvoke("PlayDelay");
    }


    private IEnumerator UpdateCorutine()
    {
        while (isVisible && (IsLoop || allCount != count))
        {
            UpdateCorutineFrame();
            if (!IsLoop && allCount == count)
                break;
            yield return new WaitForSeconds(deltaFps);
        }
        isCorutineStarted = false;
        currentRenderer.enabled = false;
    }

    #endregion CorutineCode

    private void UpdateCorutineFrame()
    {
        ++allCount;
        ++index;
        if (index >= count) index = 0;
        var offset = new Vector2((float)index / Columns - (index / Columns), 1 - (index / Columns) / (float)Rows);
        if (currentRenderer != null) instanceMaterial.SetTextureOffset("_MainTex", offset);
    }

    void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
            instanceMaterial = null;
        }
    }

    #endregion
}