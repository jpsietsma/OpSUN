using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
        // class to hide the unseen lights on the demo scene
    public class basicHide : MonoBehaviour
    {
        public List<GameObject> GameObjects;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (GameObject GO in GameObjects)
                {
                    GO.SetActive(true);
                }
            }

        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (GameObject GO in GameObjects)
                {
                    GO.SetActive(false);
                }
            }

        }
    }
}
