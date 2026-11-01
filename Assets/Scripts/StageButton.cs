using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    public int stageNumber;

    void OnEnable()
    {
        int clearStage = PlayerPrefs.GetInt("ClearStage", 0);
        GetComponent<Button>().interactable = stageNumber <= clearStage + 1;
    }

    public void OnClick()
    {
        SceneManager.LoadScene("Stage_" + stageNumber.ToString("00"));
    }
}
