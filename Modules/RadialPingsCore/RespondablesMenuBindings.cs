using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static ThinkInvisible.RadialPings.PingCatalog;

namespace ThinkInvisible.RadialPings {
    internal class RespondablesMenuBindings : RadialMenuBindings<RespondablesMenuBindings> {
        const float RESPONDABLE_LIFETIME = 30f;

        readonly PingType RespondNoPingType;
        readonly PingType RespondYesPingType;

        public delegate void OnVoteComplete(RespondablePingData rpd);
        public static event OnVoteComplete onVoteComplete;
        
        private static Dictionary<PlayerCharacterMasterController, RespondablePingData> respondables = new Dictionary<PlayerCharacterMasterController, RespondablePingData>();

        public struct RespondablePingData {
            public int pingTypeIndex;
            public int pingSkinIndex;
            public float birthday;
            public PingData pingData;
            public HashSet<PlayerCharacterMasterController> yesVotes;
            public HashSet<PlayerCharacterMasterController> noVotes;
            public string displayName;
        };

        internal static void UpdateRespondables() {
            if(!Run.instance || !Run.instance.isActiveAndEnabled) {
                if(respondables.Count > 0) respondables.Clear();
            } else {
                bool dictDirty = false;
                foreach(var kvp in respondables) {
                    if(Time.unscaledTime - kvp.Value.birthday > RESPONDABLE_LIFETIME) {
                        onVoteComplete?.Invoke(kvp.Value);
                        dictDirty = true;
                    }
                }
                if(dictDirty)
                    respondables = respondables.Where(kvp => Time.unscaledTime - kvp.Value.birthday <= RESPONDABLE_LIFETIME).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }
        public static void SetRespondable(PlayerCharacterMasterController owner, PingData pingData, int pingTypeIndex, int pingSkinIndex, string displayName) {
            respondables[owner] = new RespondablePingData {
                pingTypeIndex = pingTypeIndex,
                pingSkinIndex = pingSkinIndex,
                birthday = Time.unscaledTime,
                pingData = pingData,
                yesVotes = new HashSet<PlayerCharacterMasterController>(),
                noVotes = new HashSet<PlayerCharacterMasterController>(),
                displayName = displayName
            };
            respondables[owner].yesVotes.Add(owner);
            Chat.AddMessage(Language.GetStringFormatted("RADIALPINGS_SPECIAL_RESPONDABLE_START", RESPONDABLE_LIFETIME.ToString("N0")));
        }

        public enum ResponseType {
            No, Yes
        }
        internal ResponseType responseType;

        private Dictionary<ResponseType, PingType> responseMap = new Dictionary<ResponseType, PingType>();

        //0: self
        //1: user
        //2: message
        //3: yes count
        //4: other count
        //5: no count
        private bool RespondTryApply(PingData pingData, List<string> tokens, ResponseType responseType) {
            var targetPCMC = pingData.targets[1].GetComponent<PlayerCharacterMasterController>();
            if(!respondables.ContainsKey(targetPCMC)) return false;
            var resp = respondables[targetPCMC];
            int newYesVotes = resp.yesVotes.Count;
            int newNoVotes = resp.noVotes.Count;
            var nvc = resp.noVotes.Contains(pingData.owner);
            var yvc = resp.yesVotes.Contains(pingData.owner);
            switch(responseType) {
                case ResponseType.No:
                    if(nvc) return false;
                    newNoVotes++;
                    if(yvc) newYesVotes--;
                    break;
                case ResponseType.Yes:
                    if(yvc) return false;
                    newYesVotes++;
                    if(nvc) newNoVotes--;
                    break;
            }
            int newAbstainVotes = PlayerCharacterMasterController.instances.Count - newYesVotes - newNoVotes;
            tokens.Add(Util.GetBestMasterName(targetPCMC.master));
            tokens.Add(resp.displayName);
            tokens.Add(newYesVotes.ToString());
            tokens.Add(newAbstainVotes.ToString());
            tokens.Add(newNoVotes.ToString());
            return true;
        }
        private int RespondModify(ref PingData pingData, int catalogIndex, List<string> formatInserts, ResponseType responseType) {
            var targetPCMC = pingData.targets[1].GetComponent<PlayerCharacterMasterController>();
            var resp = respondables[targetPCMC];
            switch(responseType) {
                case ResponseType.No:
                    resp.noVotes.Add(pingData.owner);
                    resp.yesVotes.Remove(pingData.owner);
                    break;
                case ResponseType.Yes:
                    resp.yesVotes.Add(pingData.owner);
                    resp.noVotes.Remove(pingData.owner);
                    break;
            }
            return MiscUtil.ModifyTargetSelf(ref pingData, catalogIndex, formatInserts);
        }

        internal RespondablesMenuBindings() {
            RespondNoPingType = new PingType("RADIALPINGS_CONTEXT_RESPOND_NO",
                (PingData pingData, List<string> tokens) => {return RespondTryApply(pingData, tokens, ResponseType.No);},
                (ref PingData pingData, int catalogIndex, List<string> formatInserts) => {return RespondModify(ref pingData, catalogIndex, formatInserts, ResponseType.No);});
            RespondNoPingType.pingSkins.Add(new PingSkin(new Color(1f, 0.5f, 0.5f), RadialPingsPlugin.resources.LoadAsset<Sprite>("Assets/RadialPings/RadialPingsXIcon.png"), 8f,
                "RADIALPINGS_MESSAGE_RESPOND_NO", PingIndicator.PingType.Default));
            
            RespondYesPingType = new PingType("RADIALPINGS_CONTEXT_RESPOND_YES",
                (PingData pingData, List<string> tokens) => {return RespondTryApply(pingData, tokens, ResponseType.Yes);},
                (ref PingData pingData, int catalogIndex, List<string> formatInserts) => {return RespondModify(ref pingData, catalogIndex, formatInserts, ResponseType.Yes);});
            RespondYesPingType.pingSkins.Add(new PingSkin(new Color(0.5f, 1f, 0.5f), RadialPingsPlugin.resources.LoadAsset<Sprite>("Assets/RadialPings/RadialPingsOIcon.png"), 8f,
                "RADIALPINGS_MESSAGE_RESPOND_YES", PingIndicator.PingType.Default));

            PingCatalog.getAdditionalEntries += (list) => {
                list.Add(RespondNoPingType);
                list.Add(RespondYesPingType);
            };

            responseMap[ResponseType.No] = RespondNoPingType;
            responseMap[ResponseType.Yes] = RespondYesPingType;
        }

        protected internal override GameObject Instantiate(PlayerCharacterMasterController owner) {
            buttonBindingInfos.Clear();
            thetaOffsetDegr = 0f;
            foreach(var player in PlayerCharacterMasterController._instancesReadOnly) {
                if(!respondables.ContainsKey(player)) continue;
                var respdat = respondables[player];
                if(Time.unscaledTime - respdat.birthday > RESPONDABLE_LIFETIME) continue;
                var pingType = PingCatalog.Get(respdat.pingTypeIndex);
                var sprite = pingType.pingSkins[respdat.pingSkinIndex].sprite;
                buttonBindingInfos.Add(new DirectedPingBindingInfo {
                    iconColor = Color.white,
                    descriptionToken = $"{player.GetDisplayName()}",
                    sprite = sprite,
                    targetPCMC = player,
                    orderedTypes = new[] {responseMap[responseType]}
                });
            }

            var menuObj = base.Instantiate(owner);

            var pingHelper = menuObj.AddComponent<PingMenuHelper>();
            pingHelper.owner = owner;
            pingHelper.TryUpdatePingInfo();
            
            return menuObj;
        }
    }
}
