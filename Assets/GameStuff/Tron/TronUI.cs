using UnityEngine;
using TMPro;

public class TronUI : MonoBehaviour
{
    public GameObject scoreObject;
    public TextMeshProUGUI scoreText;

    BikeGuy m_targetBike;
    public BikeGuy targetBike
    {
        get
        {
            return m_targetBike;
        }
        set
        {
            m_targetBike = value;
            UpdateScore();
            targetBike.onEatFood.AddListener(AddScore);
        }
    }

    public int score;

    void OnEnable()
    {
        if (targetBike != null)
        {
            targetBike.onEatFood.AddListener(AddScore);
        }
    }

    public void AddScore()
    {
        score++;
        UpdateScore();
    }
    public void ResetScore()
    {
        score = 0;
        UpdateScore();
    }

    public void UpdateScore()
    {
        scoreText.text = score.ToString();
    }

    public void SetUIActive(bool yes)
    {
        scoreObject.SetActive(yes);
    }
}
