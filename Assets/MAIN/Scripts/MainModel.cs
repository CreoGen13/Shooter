using UnityEngine;
using UnityEngine.UI;

namespace MAIN.Scripts
{
    public class MainModel
    {
        public bool Overlayed { get; private set; }
        public Main Main { get; private set; }
        public static MainModel Instance;

        public MainModel(Main main)
        {
            Instance = this;
            Main = main;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Overlayed = !Overlayed;
                Main.Overlay.SetActive(Overlayed);
            }
        }
    }
}