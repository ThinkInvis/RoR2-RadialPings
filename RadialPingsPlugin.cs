using BepInEx;
using R2API.Utils;
using UnityEngine;
using BepInEx.Configuration;
using R2API;
using System.Reflection;
using Path = System.IO.Path;
using RoR2;
using System.IO;
using UnityEngine.Networking;
using TILER2;

namespace ThinkInvisible.RadialPings {
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(R2API.Networking.NetworkingAPI), nameof(LanguageAPI))]
    public class RadialPingsPlugin:BaseUnityPlugin {
        public const string ModVer = "2.1.0";
        public const string ModName = "RadialPings";
        public const string ModGuid = "com.ThinkInvisible.RadialPings";
        
        internal static BepInEx.Logging.ManualLogSource logger;

        internal static ConfigFile cfgFile;
        internal static AssetBundle resources;

        public ClientConfig clientConfig;
        public class ClientConfig : AutoConfigContainer {
            [AutoConfig("Time between ping keydown and opening of the radial menu. Faster keyups will cause a quick ping (vanilla behavior).",
                AutoConfigFlags.None, 0f, float.MaxValue)]
            [AutoConfigRoOSlider("{0:N1} seconds", 0f, 5f)]
            public float mainMenuOpenDelay { get; internal set; } = 0.2f;
        }

        public void Awake() {
            logger = Logger;
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            clientConfig = new ClientConfig();
            clientConfig.BindAll(cfgFile, "RadialPings", "Client");

            //todo: option to keep first ping preview as result

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RadialPings.radialpings_assets")) {
                resources = AssetBundle.LoadFromStream(stream);
            }

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RadialPings.lang_en.json"))
            using(var reader = new StreamReader(stream)) {
                LanguageAPI.Add(reader.ReadToEnd());
            }

            RadialMenu.Setup();

            new MainPingMenuBindings();
            new PlayersMenuBindings();
            new DroneMenuBindings();
            new RespondablesMenuBindings();

            On.RoR2.PlayerCharacterMasterController.CheckPinging += On_PlayerCharacterMasterController_CheckPinging;
            On.RoR2.Run.OnDestroy += On_Run_OnDestroy;

            R2API.Networking.NetworkingAPI.RegisterMessageType<PingMenuHelper.MsgCustomPing>();
        }

        void Start() {
            PingCatalog.Init();
        }
        
        private void On_Run_OnDestroy(On.RoR2.Run.orig_OnDestroy orig, Run self) {
            orig(self);
            RespondablesMenuBindings.UpdateRespondables();
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

            if(menuTracker.btnHoldStopwatch > clientConfig.mainMenuOpenDelay && !menuTracker.latestMenu && !menuTracker.btnHoldActioned) {
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