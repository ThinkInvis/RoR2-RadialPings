using RoR2.UI;
using TMPro;
using UnityEngine;

namespace ThinkInvisible.RadialPings {
    public static class RadialMenu {
        public static GameObject genericRadialMenuPrefab {get; private set;}

        internal static void Setup() {
            ProceduralRadialMenu.buttonPrefab = Resources.Load<GameObject>("@RadialPings:Assets/RadialPings/ProceduralRadialButton.prefab");
            genericRadialMenuPrefab = Resources.Load<GameObject>("@RadialPings:Assets/RadialPings/ProceduralRadialMenu.prefab");
            
            genericRadialMenuPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var menuCtrl = genericRadialMenuPrefab.GetComponent<ProceduralRadialMenu>();
            menuCtrl.inOutAnimSpeed = 0.2f;
            menuCtrl.extraOutroScale = 1.25f;
            
            var tmpfont = Resources.Load<TMP_FontAsset>("tmpfonts/misc/tmpsquaresboldhud");
            var tmpmtl = Resources.Load<Material>("tmpfonts/misc/tmpsquaresboldhud");

			var newText = genericRadialMenuPrefab.transform.Find("DisplayContainer").Find("Caption").gameObject.AddComponent<TextMeshPro>();
            newText.alignment = TextAlignmentOptions.Center;
            newText.enableAutoSizing = true;
            newText.fontSizeMin = 120;
            newText.fontSizeMax = 960;
            _ = newText.renderer;
            newText.font = tmpfont;
            newText.material = tmpmtl;
           newText.color = Color.white;
            newText.text = "";
            newText.ComputeMarginSize();

			var newSubText = genericRadialMenuPrefab.transform.Find("DisplayContainer").Find("ContextCaption").gameObject.AddComponent<TextMeshPro>();
            newSubText.alignment = TextAlignmentOptions.Center;
            newSubText.enableAutoSizing = true;
            newSubText.fontSizeMin = 60;
            newSubText.fontSizeMax = 480;
            _ = newSubText.renderer;
            newSubText.font = tmpfont;
            newSubText.material = tmpmtl;
            newSubText.color = Color.white;
            newSubText.text = "";
            newSubText.ComputeMarginSize();

            genericRadialMenuPrefab.AddComponent<MPEventSystemLocator>();
            genericRadialMenuPrefab.AddComponent<CursorOpener>();
        }
    }
}
