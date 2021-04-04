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

namespace ThinkInvisible.RadialPings {
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ResourcesAPI), nameof(R2API.Networking.NetworkingAPI), nameof(LanguageAPI))]
    public class RadialPingsPlugin:BaseUnityPlugin {
        public const string ModVer =
#if DEBUG
            "0." +
#endif
            "2.0.1";
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
            new RespondablesMenuBindings();

            On.RoR2.PlayerCharacterMasterController.CheckPinging += On_PlayerCharacterMasterController_CheckPinging;
            On.RoR2.Run.OnDestroy += On_Run_OnDestroy;

            R2API.Networking.NetworkingAPI.RegisterMessageType<PingMenuHelper.MsgCustomPing>();

            #if DEBUG
            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (orig, self, conn) => {
                if(!self.clientLoadedScene) {
				    ClientScene.Ready(conn);
				    if(self.autoCreatePlayer) ClientScene.AddPlayer(0);
			    }
			    self.clientRttFrame = 0f;
			    self.filteredClientRttFixed = 0f;
                self.ClientSetPlayers(conn);
                RoR2.Networking.RttManager.OnConnectionDiscovered(conn);
            };
            On.RoR2.NetworkUser.UpdateUserName += (orig, self) => {
                self.userName = $"DEBUG USER NetID:{self.netId}";
            };
            #endif
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