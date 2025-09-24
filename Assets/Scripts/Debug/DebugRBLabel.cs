using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DebugRBLabel : MonoBehaviour
{
    PhysicsHandlerS physics;

    [SerializeField] TextMeshProUGUI labelPrefab;

    public List<Rigidbody4D> rigidbodies = new List<Rigidbody4D>();
    public List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();

    public bool active;

    [SerializeField] float fontSize = 16;
    [SerializeField] float maxFontSize = 48;



    void OnEnable()
    {
        physics = PhysicsHandlerS.singleton;
    }

    public void SetActive(bool act)
    {
        if (active != act)
        {
            if (!act)
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    Destroy(labels[i].gameObject);
                }
                labels.Clear();
            }
        }

        active = act;
    }

    void Update()
    {
        if (!active) return;

        if (!physics) physics = PhysicsHandlerS.singleton;

        rigidbodies = physics.rigidbodyList;

        for (int i = rigidbodies.Count - labels.Count; i != 0; i = rigidbodies.Count - labels.Count)
        {
            if (i > 0)
            {
                TextMeshProUGUI lab = Instantiate(labelPrefab, transform);
                labels.Add(lab);
            }
            else
            {
                labels.RemoveAt(0);
            }
        }

        for (int i = 0; i < labels.Count; i++)
        {
            SetLabel(labels[i], rigidbodies[i]);
        }
    }

    void SetLabel(TextMeshProUGUI label, Rigidbody4D rb)
    {
        if (rb == null) return;

        label.text = rb.gameObject.name;

        Vector4 pos4 = rb.transform4.positionNorm;

        pos4 = Camera.main.worldToCameraMatrix * pos4;

        Vector3 clipPos = UFunc.SterographicProjection(pos4);


        if (clipPos.z > 0)
        {
            label.gameObject.SetActive(false);
            return;
        }
        else
        {
            label.gameObject.SetActive(true);
        }


        clipPos = -clipPos / clipPos.z;
        clipPos.y *= (float)Screen.width / (float)Screen.height;

        Vector2 screenPos = 0.5f * clipPos * new Vector2(Screen.width, Screen.height);

        label.GetComponent<RectTransform>().anchoredPosition = screenPos;

        //float scale = fontSize * Mathf.Lerp(1.5f,0.5f,Mathf.Acos(pos4.w)/Mathf.PI);
        float scale = fontSize * Mathf.PI / Mathf.Acos(pos4.w);
        scale = Mathf.Min(scale, maxFontSize);
        label.fontSize = scale;
    }
}
