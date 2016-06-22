using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif
namespace Invector
{
    public class vGameController : MonoBehaviour
    {
        [Tooltip("Assign here the locomotion (empty transform) to spawn the Player")]
        public Transform spawnPoint;
        [Tooltip("Assign the Character Prefab to instantiate at the SpawnPoint, leave unassign to Restart the Scene")]
        public GameObject playerPrefab;
        [Tooltip("Time to wait until the scene restart or the player will be spawned again")]
        public float respawnTimer = 4f;
        [Tooltip("Check if you want to leave your dead body at the place you died")]
        public bool destroyBodyAfterDead;

        public bool lockEnemies;
        
        [HideInInspector]
        public GameObject currentPlayer
        {
            get
            {
                if (lockEnemies) return null;
                return _currentPlayer;
            }
            set
            {
                _currentPlayer = value;
            }
        }

        private GameObject _currentPlayer;

        private static vGameController _instance;        
        public static vGameController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<vGameController>();
                    //Tell unity not to destroy this object when loading a new scene
                    //DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        void Start()
        {
            var characterController = FindObjectOfType<vThirdPersonMotor>();
            if (characterController != null)
            {
                currentPlayer = characterController.gameObject;
                return;
            }

            if (currentPlayer == null && playerPrefab != null && spawnPoint != null)
                Spawn(spawnPoint);
        }

        public void Spawn(Transform _spawnPoint)
        {
            if (playerPrefab != null)
            {
                var oldPlayer = currentPlayer;
                currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;

                if (oldPlayer != null && destroyBodyAfterDead)
                    Destroy(oldPlayer);
                else if (oldPlayer != null)
                {
                    var comps = oldPlayer.GetComponents<MonoBehaviour>();
                    foreach (Component comp in comps) Destroy(comp);
                    var coll = oldPlayer.GetComponent<Collider>();
                    if (coll != null) Destroy(coll);
                    var rigdbody = oldPlayer.GetComponent<Rigidbody>();
                    if (rigdbody != null) Destroy(rigdbody);
                    var animator = oldPlayer.GetComponent<Animator>();
                    if (animator != null) Destroy(animator);
                }
            }
        }

        public void Spawn()
        {
            if (playerPrefab != null && spawnPoint != null)
            {
                var oldPlayer = currentPlayer;                

                if (oldPlayer != null && destroyBodyAfterDead)
                    Destroy(oldPlayer);
                else 
                {
                    var comps = oldPlayer.GetComponents<MonoBehaviour>();
                    foreach (Component comp in comps) Destroy(comp);
                    var coll = oldPlayer.GetComponent<Collider>();
                    if(coll!=null) Destroy(coll);
                    var rigdbody = oldPlayer.GetComponent<Rigidbody>();
                    if (rigdbody != null) Destroy(rigdbody);
                    var animator = oldPlayer.GetComponent<Animator>();
                    if (animator != null) Destroy(animator);
                }
                currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
            }
        }

        public void ResetScene()
        {
        #if UNITY_5_3
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        #else
            Application.LoadLevel(Application.loadedLevel);
        #endif
        }      
    }
}