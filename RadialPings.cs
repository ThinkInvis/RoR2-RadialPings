using BepInEx;
using R2API.Utils;
using UnityEngine;
using BepInEx.Configuration;
using R2API;
using System.Reflection;
using Path = System.IO.Path;
using RoR2.UI;
using RoR2;
using TMPro;
using System.IO;

namespace ThinkInvisible.RadialPings {
    [BepInDependency("com.bepis.r2api", "2.5.14")]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ResourcesAPI), nameof(R2API.Networking.NetworkingAPI))]
    public class RadialPingsPlugin:BaseUnityPlugin {
        public const string ModVer = "1.0.0";
        public const string ModName = "RadialPings";
        public const string ModGuid = "com.ThinkInvisible.RadialPings";
        
        internal static BepInEx.Logging.ManualLogSource logger;

        internal static ConfigFile cfgFile;

        internal static GameObject genericRadialMenuPrefab;

        //todo: configify this
        internal float mainMenuOpenDelay = 0.2f;

        public void Awake() {
            logger = Logger;
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            mainMenuOpenDelay = cfgFile.Bind(new ConfigDefinition("Client","MainMenuOpenDelay"), 0.2f,
                new ConfigDescription("Time between ping keydown and opening of the radial menu. Faster keyups will cause a quick ping (vanilla behavior).",
                new AcceptableValueRange<float>(0f, float.MaxValue))).Value;
            //todo: option to keep first ping preview as result

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RadialPings.radialpings_assets")) {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@RadialPings", bundle);
                ResourcesAPI.AddProvider(provider);
            }

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RadialPings.lang_en.json"))
            using(var reader = new StreamReader(stream)) {
                LanguageAPI.Add(reader.ReadToEnd());
            }

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

            new MainPingMenuBindings();
            new PlayersMenuBindings();
            new DroneMenuBindings();

            On.RoR2.PlayerCharacterMasterController.CheckPinging += On_PlayerCharacterMasterController_CheckPinging;

            R2API.Networking.NetworkingAPI.RegisterMessageType<PingMenuHelper.MsgCustomPing>();
        }

        void Start() {
            CustomPingCatalog.Init();
        }

        private void On_PlayerCharacterMasterController_CheckPinging(On.RoR2.PlayerCharacterMasterController.orig_CheckPinging orig, PlayerCharacterMasterController self) {
            if(!self.hasEffectiveAuthority || !self.body || !self.bodyInputs) return;

            var menuTracker = self.gameObject.GetComponent<PingMenuInstanceTracker>();
            if(!menuTracker) menuTracker = self.gameObject.AddComponent<PingMenuInstanceTracker>();
            if(self.bodyInputs.ping.down)
                menuTracker.btnHoldStopwatch += Time.unscaledDeltaTime;
            else {
                menuTracker.btnHoldStopwatch = 0f;
                menuTracker.btnHoldActioned = false;
            }

            if(menuTracker.btnHoldStopwatch > mainMenuOpenDelay && !menuTracker.latestMenu && !menuTracker.btnHoldActioned) {
                menuTracker.btnHoldActioned = true;
                menuTracker.latestMenu = MainPingMenuBindings.instance.Instantiate(self);
            } else if(self.bodyInputs.ping.justReleased) {
                if(menuTracker.latestMenu) {
                    menuTracker.btnHoldActioned = false;
                    var cpt = menuTracker.latestMenu.GetComponent<ProceduralRadialMenu>();

                    if(cpt.inOutState != ProceduralRadialMenu.InOutState.Outro) {
                        cpt.Activate(false);
                        menuTracker.latestMenu = null;
                        return;
                    }
                    menuTracker.latestMenu = null;
                }
                if(!menuTracker.btnHoldActioned)
                    MiscUtil.DefaultPing(self);
                menuTracker.btnHoldActioned = false;
            }
        }
    }
}