using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif
public class vChangeScenes : MonoBehaviour
{
    public void LoadThirdPersonScene()
    {
        #if UNITY_5_3            
        SceneManager.LoadScene("3rdPersonController-Demo");
        #else
        Application.LoadLevel("3rdPersonController-Demo");
        #endif
    }

    public void LoadTopDownScene()
    {
       
        #if UNITY_5_3       
        SceneManager.LoadScene("TopDownController-Demo");
        #else
        Application.LoadLevel("TopDownController-Demo");
        #endif
    }

    public void LoadPlatformScene()
    {
       
        #if UNITY_5_3       
        SceneManager.LoadScene("2.5DController-Demo");
        #else
        Application.LoadLevel("2.5DController-Demo");
        #endif
    }

    public void LoadIsometricScene()
    {
        
        #if UNITY_5_3      
        SceneManager.LoadScene("IsometricController-Demo");
        #else
        Application.LoadLevel("IsometricController-Demo");
        #endif
    }

    public void LoadVMansion()
    {
     
        #if UNITY_5_3       
        SceneManager.LoadScene("V-Mansion");
        #else
        Application.LoadLevel("V-Mansion");
        #endif
    }

    public void LoadCompanionAI()
    {
        #if UNITY_5_3
        SceneManager.LoadScene("BeatnUp@CompanionAI-Demo");
        #else
        Application.LoadLevel("BeatnUp@CompanionAI-Demo");
        #endif
    }

    public void LoadPuzzleBox()
    {
        #if UNITY_5_3
        SceneManager.LoadScene("PuzzleBoxes");
        #else
        Application.LoadLevel("PuzzleBoxes");
        #endif
    }
}
