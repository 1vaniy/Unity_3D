using UnityEngine;
using System.Collections;

public class IntroCamera : MonoBehaviour
{
    public Transform targetPivot;
    public CameraOrbit cameraOrbit;
    public float transitionDuration = 3f;

    private Vector3 startPos;
    private Quaternion startRot;

    // Убираем Start(), чтобы ничего не запоминать заранее

    public void StartTransition()
    {
        // ЗАПОМИНАЕМ ПОЗИЦИЮ В МОМЕНТ СТАРТА ПЕРЕЛЕТА (когда камера точно там, где надо)
        startPos = transform.position;
        startRot = transform.rotation;

        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        float t = 0f;

        if (cameraOrbit == null || targetPivot == null)
        {
            Debug.LogError("IntroCamera: Не назначен CameraOrbit или TargetPivot!");
            yield break;
        }

        // Вычисляем финальную позицию
        float distance = cameraOrbit.distance;
        float height = cameraOrbit.height;
        float offsetX = cameraOrbit.shoulderOffsetX;
        float offsetY = cameraOrbit.shoulderOffsetY;

        Quaternion rotation = Quaternion.Euler(20f, 0f, 0f);

        Vector3 basePosition = targetPivot.position + Vector3.up * height;
        Vector3 finalPos = basePosition - (rotation * Vector3.forward * distance);
        finalPos += rotation * new Vector3(offsetX, offsetY, 0f);

        Vector3 lookTarget = basePosition + (rotation * new Vector3(offsetX, offsetY, 0f));
        Quaternion finalRot = Quaternion.LookRotation(lookTarget - finalPos);

        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;

            transform.position = Vector3.Lerp(startPos, finalPos, Mathf.SmoothStep(0f, 1f, t));

            if (t < 0.7f)
            {
                transform.rotation = startRot;
            }
            else
            {
                float rotateT = (t - 0.7f) / 0.3f;
                transform.rotation = Quaternion.Slerp(startRot, finalRot, Mathf.SmoothStep(0f, 1f, rotateT));
            }

            yield return null;
        }

        cameraOrbit.SetAngles(0f, 20f);
        cameraOrbit.enabled = true;

        this.enabled = false;
    }
}