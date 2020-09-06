using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.RadialPings {
    public class PingMenuHelper : MonoBehaviour {
        public const float UPDATE_RATE = 0.25f;
        public PlayerCharacterMasterController owner;
        public PingerController.PingInfo lastPingInfo = PingerController.emptyPing;

        float stopwatch = 0f;

        void Update() {
            stopwatch += Time.unscaledDeltaTime;
            if(stopwatch > UPDATE_RATE) {
                stopwatch = 0f;

                TryUpdatePingInfo();
            }
        }

        public bool TryUpdatePingInfo() {
            if(!owner.body || !owner.bodyInputs) return false;
            PingerController.GeneratePingInfo(new Ray(owner.bodyInputs.aimOrigin, owner.bodyInputs.aimDirection), owner.body.gameObject, out lastPingInfo);
            return true;
        }

        public bool BuildPosOnlyPingInfo(LayerMask layerMask, QueryTriggerInteraction queryInteract, float distanceBonus = 1000f) {
            if(!owner.body || !owner.bodyInputs) return false;
            float rayDist;
			var aimRay = CameraRigController.ModifyAimRayIfApplicable(new Ray(owner.bodyInputs.aimOrigin, owner.bodyInputs.aimDirection), owner.body.gameObject, out rayDist);
			float maxDist = rayDist + distanceBonus;
			RaycastHit raycastHit;
            if(!Util.CharacterRaycast(owner.body.gameObject, aimRay, out raycastHit, maxDist, layerMask, queryInteract))
                return false;
            lastPingInfo = new PingerController.PingInfo {
                active = true,
                origin = raycastHit.point,
                normal = raycastHit.normal,
                targetNetworkIdentity = null
            };
            return true;
        }

        public bool TryDeductPingStock() {
            if(!owner) return false;
            
            if(RoR2Application.isInSinglePlayer) return true;
            if(owner.pingerController.pingStock > 0) {
                owner.pingerController.pingStock --;
                return true;
            }
            Chat.AddMessage(Language.GetString("PLAYER_PING_COOLDOWN"));
            return false;
        }
        
        public void ManualBuildPing(GameObject targetObject, Vector3 origin, Vector3 targetNormal, string ownerText, string chatText, Color pingColor, Color textColor, Color spriteColor, Sprite sprite, float duration, PingIndicator.PingType basePingType, bool doHighlight, Highlight.HighlightColor highlightColor, float highlightStrength) {
            var pmit = owner.GetComponent<PingMenuInstanceTracker>();
            if(!pmit) {
                pmit = owner.gameObject.AddComponent<PingMenuInstanceTracker>();
            }
            pmit.currentPingIsCustom = true;

            if(owner.pingerController.pingIndicator) GameObject.Destroy(owner.pingerController.pingIndicator.gameObject);

            //normal setup
			//var pingIndObj = GameObject.Instantiate(RadialPingsPlugin.customPingIndicatorPrefab);
            var pingIndObj = GameObject.Instantiate(Resources.Load<GameObject>("prefabs/PingIndicator"));
            var pingInd = pingIndObj.GetComponent<PingIndicator>();
			owner.pingerController.pingIndicator = pingInd;
			owner.pingerController.pingIndicator.pingOwner = owner.pingerController.gameObject;

            pingInd.pingOrigin = origin;
            pingInd.pingNormal = targetNormal;
            pingInd.pingTarget = targetObject;
            pingInd.pingHighlight.isOn = false;
            pingInd.positionIndicator.targetTransform = null;
            
            if(targetObject) {
                if(pingInd.pingOrigin == null) pingInd.pingOrigin = targetObject.transform.position;
                var targetBody = targetObject.GetComponent<CharacterBody>();
                var targetModelLoc = targetObject.GetComponent<ModelLocator>();
                var targetModel = targetModelLoc?.modelTransform?.GetComponent<CharacterModel>();
                if(targetBody) pingInd.targetTransformToFollow = targetBody.coreTransform;
                pingInd.transform.position = targetObject.transform.position;
                pingInd.positionIndicator.targetTransform = targetObject.transform;

                if(doHighlight) {
                    Renderer targetRenderer = null;
                    if(targetBody && targetModel) {
						foreach(var renInfo in targetModel.baseRendererInfos) {
                            if(!renInfo.ignoreOverlays) {
                                targetRenderer = renInfo.renderer;
                                break;
                            }
						}
                    } else if(!targetBody) {
                        targetRenderer = targetModelLoc?.modelTransform?.GetComponentInChildren<Renderer>() ?? targetObject.GetComponentInChildren<Renderer>();
                    }
                    
                    if(targetRenderer != null) {
                        pingInd.pingHighlight.highlightColor = highlightColor;
                        pingInd.pingHighlight.targetRenderer = targetRenderer;
                        pingInd.pingHighlight.strength = highlightStrength;
                        pingInd.pingHighlight.isOn = true;
                    }
                }
            } else {
                pingInd.transform.position = origin;
            }
            pingInd.transform.rotation = Util.QuaternionSafeLookRotation(targetNormal);
            pingInd.transform.localScale = Vector3.one;
            pingInd.positionIndicator.defaultPosition = pingInd.transform.position;
            pingInd.pingType = PingIndicator.PingType.Default;
            pingInd.pingObjectScaleCurve.enabled = false;
            pingInd.pingObjectScaleCurve.enabled = true;
            foreach(var obj in pingInd.interactablePingGameObjects)
                obj.SetActive(false);
            foreach(var obj in pingInd.enemyPingGameObjects)
                obj.SetActive(false);
            foreach(var obj in pingInd.defaultPingGameObjects)
                obj.SetActive(false);

            GameObject[] listToSetup;
            switch(basePingType) {
                case PingIndicator.PingType.Enemy:
                    listToSetup = pingInd.enemyPingGameObjects;
                    break;
                case PingIndicator.PingType.Interactable:
                    listToSetup = pingInd.interactablePingGameObjects;
                    break;
                default:
                    listToSetup = pingInd.defaultPingGameObjects;
                    break;
            }
            foreach(var obj in listToSetup) {
                obj.SetActive(true);
            }

            pingInd.pingText.enabled = true;
            pingInd.pingText.text = ownerText;
            pingInd.pingColor = pingColor;
            pingInd.defaultPingColor = pingColor;
            pingInd.pingDuration = duration;
            pingInd.pingText.color = textColor;
            pingInd.fixedTimer = duration;

            if(chatText != null)
			    Chat.AddMessage(chatText);

            //BoingyScaler setup
			listToSetup[0].GetComponent<SpriteRenderer>().sprite = sprite;
            listToSetup[0].GetComponent<SpriteRenderer>().color = spriteColor;
            var psmain = listToSetup[1].GetComponent<ParticleSystem>().main;
            psmain.startColor = pingColor;
        }

        public void AuthorityPerformCustomPing(GameObject[] extraTargets, params PingCatalog.PingType[] orderedPingTypesToTry) {
            if(!TryUpdatePingInfo()) return;
            
            var ownerText = Util.GetBestMasterName(owner.master);
            
            List<string> args = new List<string> {ownerText};

            PingCatalog.PingType selectedType = null;

            var allTargets = new List<GameObject>{lastPingInfo.targetGameObject};
            if(extraTargets != null) allTargets.AddRange(extraTargets);

            var pingData = new PingCatalog.PingData {
                origin = lastPingInfo.origin,
                normal = lastPingInfo.normal,
                targets = allTargets,
                owner = this.owner
            };

            foreach(var pingType in orderedPingTypesToTry) {
                if(pingType.catalogIndex == -1) continue;
                if(pingType.tryApply(pingData, args)) {
                    selectedType = pingType;
                    break;
                }
            }

            if(selectedType == null) return;

            if(owner.pingerController.hasAuthority && (pingData.origin != null || pingData.targets.Count > 0) && TryDeductPingStock())
                new MsgCustomPing(selectedType, pingData).Send(R2API.Networking.NetworkDestination.Clients);
        }

        public string GetFormattedContext(GameObject[] extraTargets, params PingCatalog.PingType[] pingTypes) {
            PingCatalog.PingType selectedType = null;

            var allTargets = new List<GameObject>{lastPingInfo.targetGameObject};
            if(extraTargets != null) allTargets.AddRange(extraTargets);
            
            List<string> args = new List<string>();
            var pingData = new PingCatalog.PingData {
                origin = lastPingInfo.origin,
                normal = lastPingInfo.normal,
                targets = allTargets,
                owner = this.owner
            };

            foreach(var pingType in pingTypes) {
                if(pingType.tryApply(pingData, args)) {
                    selectedType = pingType;
                    break;
                }
            }

            if(selectedType == null || selectedType.previewToken == null) return null;

            return string.Format(Language.GetString(selectedType.previewToken), args.ToArray());
        }

        private static void PerformCustomPing(PingCatalog.PingType pingType, PingCatalog.PingData pingData) {
            if(!pingData.owner) {
                RadialPingsPlugin.logger.LogError("Received PerformCustomPing for nonexistent owner");
                return;
            }

            var pingHelper = pingData.owner.GetComponent<PingMenuHelper>();
            if(!pingHelper) {
                pingHelper = pingData.owner.gameObject.AddComponent<PingMenuHelper>();
                pingHelper.owner = pingData.owner;
            }

            var ownerText = Util.GetBestMasterName(pingData.owner.master);
            List<string> args = new List<string> {ownerText};
            if(!pingType.tryApply(pingData, args)) {
                RadialPingsPlugin.logger.LogWarning($"PerformCustomPing failed to apply CustomPingType {nameof(pingType)} (likely missing networkidentity on target object)");
                return;
            }
            var skin = pingType.pingSkins[pingType.modifyAndSelectSkin?.Invoke(ref pingData) ?? 0];

            var chatText = string.Format(Language.GetString(skin.chatToken), args.ToArray());

            pingHelper.ManualBuildPing(pingData.targets.Count > 0 ? pingData.targets[0] : null, pingData.origin, pingData.normal,
                ownerText, chatText,
                skin.pingColor, skin.textColor, skin.spriteColor, skin.sprite, skin.duration, skin.basePingType,
                skin.doHighlight, skin.highlightColor, skin.highlightStrength);
        }

        public struct MsgCustomPing : INetMessage {
            public PingCatalog.PingType _pingType;
            public PingCatalog.PingData _pingData;
            
            public void Serialize(NetworkWriter writer) {
                writer.Write(_pingType.catalogIndex);
                _pingData.Serialize(writer);
            }

            public void Deserialize(NetworkReader reader) {
                _pingType = PingCatalog.Get(reader.ReadInt32());
                _pingData = reader.Read<PingCatalog.PingData>();
            }

            public void OnReceived() {
                PerformCustomPing(_pingType, _pingData);
            }

            public MsgCustomPing(PingCatalog.PingType pingType, PingCatalog.PingData pingData) {
                _pingType = pingType;
                _pingData = pingData;
            }
        }
    }
}
