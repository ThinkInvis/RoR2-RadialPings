using BepInEx;
using R2API.Utils;
using UnityEngine;
using BepInEx.Configuration;
using R2API;
using System.Reflection;
using Path = System.IO.Path;
using RoR2;
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

            RadialMenu.Setup();

            new MainPingMenuBindings();
            new PlayersMenuBindings();
            new DroneMenuBindings();

            On.RoR2.PlayerCharacterMasterController.CheckPinging += On_PlayerCharacterMasterController_CheckPinging;

            R2API.Networking.NetworkingAPI.RegisterMessageType<PingMenuHelper.MsgCustomPing>();
        }

        void Start() {
            PingCatalog.Init();
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