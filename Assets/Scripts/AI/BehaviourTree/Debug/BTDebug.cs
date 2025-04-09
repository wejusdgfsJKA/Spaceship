using TMPro;
using UnityEngine;

namespace BTree
{
    public class BTDebug : MonoBehaviour
    {
        [SerializeField] BTree LinkedBT;
        [SerializeField] TextMeshProUGUI LinkedDebugText;

        // Start is called before the first frame update
        void Start()
        {
            LinkedDebugText.text = "";
        }

        // Update is called once per frame
        void Update()
        {
            if (LinkedBT != null)
            {
                LinkedDebugText.text = LinkedBT.GetDebugText();
            }
        }
    }
}