using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sacristan.Utils.Average;
using System.Text;
using HurricaneVR;

public class DebugMetricsVR : MonoBehaviour
{
#if ENABLE_DEBUG_METRICS

    readonly YieldInstruction refreshRate = new WaitForSeconds(0.1f);
    FloatAveraged fps = new FloatAveraged(10);
    string FPS => Mathf.Round(fps.GetAverageOrLastValIfNotSampled()).ToString("n0");
    string Batches => _batchesRecorder.LastValue.ToString();
    string Triangles => _trianglesRecorder.LastValue.ToString();
    string SystemUsedMemory => (_systemMemoryRecorder.LastValue / (1024 * 1024)).ToString();
    string SystemMemory => (SystemInfo.systemMemorySize).ToString();

    // ProfilerRecorder _totalReservedMemoryRecorder;
    Unity.Profiling.ProfilerRecorder _batchesRecorder;
    Unity.Profiling.ProfilerRecorder _trianglesRecorder;
    Unity.Profiling.ProfilerRecorder _systemMemoryRecorder;
    Text _text;

    bool IsActive
    {
        get => _text.enabled;
        set => _text.enabled = value;
    }

    private void OnEnable()
    {
        _batchesRecorder = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Render, "Batches Count");
        _trianglesRecorder = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Render, "Triangles Count");
        _systemMemoryRecorder = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory, "System Used Memory");

    }

    private void OnDisable()
    {
        // _totalReservedMemoryRecorder.Dispose();
        _batchesRecorder.Dispose();
        _systemMemoryRecorder.Dispose();
        _trianglesRecorder.Dispose();
    }

    IEnumerator Start()
    {
        _text = GetComponentInChildren<Text>();
        StringBuilder stringBuilder = new StringBuilder();

        IsActive = false;
        yield return null;

        GameManager.Instance.HVRControllerEvents?.LeftPrimaryActivated?.AddListener(() => IsActive = !IsActive);

        while (true)
        {
            if (IsActive)
            {
                stringBuilder.Append($"<size=30>v{Application.version}</size>\n");
                stringBuilder.Append($"FPS: {FPS}\n");

                stringBuilder.Append($"Batches: {Batches}\n");
                stringBuilder.Append($"Triangles: {Triangles}\n");
                stringBuilder.Append($"Mem: {SystemUsedMemory}/{SystemMemory}MB\n");

                _text.text = stringBuilder.ToString();
                stringBuilder.Clear();
                yield return refreshRate;
            }
            else yield return null;
        }
    }

    private void Update()
    {
        fps.Sample(1f / Time.smoothDeltaTime);
        //if (InputBridge.Instance.YButtonDown) IsActive = !IsActive;

    }
#endif
}
