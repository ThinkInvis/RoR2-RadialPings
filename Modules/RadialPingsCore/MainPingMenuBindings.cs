using RoR2;
using RoR2.UI;
using System.Linq;
using UnityEngine;
using static ThinkInvisible.RadialPings.PingCatalog;

namespace ThinkInvisible.RadialPings {
    internal class MainPingMenuBindings : RadialMenuBindings<MainPingMenuBindings> {
        internal MainPingMenuBindings() {
            var LookHerePingTypeTarget = new PingType("RADIALPINGS_CONTEXT_MAIN_LOOK_TARGET",
                (pingData, tokens) => {
                    if(pingData.targets[0] == null) return false;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(pingData.targets[0], ref token)) return false;
                    MiscUtil.TryGetPurchaseAppend(pingData.targets[0], ref token);
                    tokens.Add(token);
                    return true;
                });
            LookHerePingTypeTarget.pingSkins.Add(new PingSkin(Color.white, Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsLookIcon.png"), 8f,
                "RADIALPINGS_MESSAGE_MAIN_LOOK_TARGET", PingIndicator.PingType.Default));
            
            var LookHerePingTypeNoTarget = new PingType(null,
                (pingData, tokens) => {return true;});
            LookHerePingTypeNoTarget.pingSkins.Add(new PingSkin(Color.white, Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsLookIcon.png"), 8f,
                "RADIALPINGS_MESSAGE_MAIN_LOOK_NOTARGET", PingIndicator.PingType.Default));

            var AggressivePingTypeEnemy = new PingType("RADIALPINGS_CONTEXT_MAIN_AGGRESSIVE_ENEMY",
                (pingData, tokens) => {
                    if(pingData.targets[0] == null || !pingData.targets[0].GetComponent<CharacterBody>()) return false;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(pingData.targets[0], ref token)) return false;
                    tokens.Add(token);
                    return true;
                });
            AggressivePingTypeEnemy.pingSkins.Add(new PingSkin(new Color(1f, 0.5f, 0f), Resources.Load<Sprite>("textures/miscicons/texAttackIcon"), 15f,
                "RADIALPINGS_MESSAGE_MAIN_AGGRESSIVE_ENEMY", PingIndicator.PingType.Enemy,
                Highlight.HighlightColor.teleporter, 1f));

            var AggressivePingTypeTeleporter = new PingType("RADIALPINGS_CONTEXT_MAIN_AGGRESSIVE_TELEPORTER",
                (pingData, tokens) => {
                    if(pingData.targets[0] == null || !pingData.targets[0].GetComponent<TeleporterInteraction>()) return false;
                    return true;
                });
            AggressivePingTypeTeleporter.pingSkins.Add(new PingSkin(new Color(1f, 0.5f, 0f), Resources.Load<Sprite>("textures/miscicons/texTeleporterIconOutlined"), 15f,
                "RADIALPINGS_MESSAGE_MAIN_AGGRESSIVE_TELEPORTER", PingIndicator.PingType.Interactable,
                Highlight.HighlightColor.teleporter, 1f));

            var AggressivePingTypeInteractable = new PingType("RADIALPINGS_CONTEXT_MAIN_AGGRESSIVE_INTERACTABLE",
                (pingData, tokens) => {
                    var targetObj = pingData.targets[0];
                    if(targetObj == null) return false;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(pingData.targets[0], ref token)) return false;
                    if(MiscUtil.TryGetPurchaseAppend(pingData.targets[0], ref token)) {
                        tokens.Add(token);
                        return true;
                    }
                    if(!targetObj.GetComponent<GenericPickupController>() && !targetObj.GetComponent<PickupPickerController>()) return false;
                    tokens.Add(token);
                    return true;
                });
            AggressivePingTypeInteractable.pingSkins.Add(new PingSkin(new Color(1f, 0.5f, 0f), Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsNoLootIcon.png"), 30f,
                "RADIALPINGS_MESSAGE_MAIN_AGGRESSIVE_INTERACTABLE", PingIndicator.PingType.Interactable,
                Highlight.HighlightColor.interactive, 1f));

            var AggressivePingTypeNoTarget = new PingType("RADIALPINGS_CONTEXT_MAIN_AGGRESSIVE_NOTARGET",
                (targetObj, tokens) => {
                    return true;
                }, MiscUtil.ModifyTargetSelf);
            AggressivePingTypeNoTarget.pingSkins.Add(new PingSkin(new Color(1f, 0.5f, 0f), Resources.Load<Sprite>("textures/miscicons/texSprintIcon"), 8f,
                "RADIALPINGS_MESSAGE_MAIN_AGGRESSIVE_NOTARGET", PingIndicator.PingType.Default));

            var RespondNoPingType = new PingType(null,
                (targetObj, tokens) => {
                    return true;
                }, MiscUtil.ModifyTargetSelf);
            RespondNoPingType.pingSkins.Add(new PingSkin(new Color(1f, 0.5f, 0.5f), Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsXIcon.png"), 8f,
                "RADIALPINGS_MESSAGE_MAIN_NO", PingIndicator.PingType.Default));
            
            var RespondHelpPingType = new PingType(null,
                (targetObj, tokens) => {
                    return true;
                }, MiscUtil.ModifyTargetSelf);
            RespondHelpPingType.pingSkins.Add(new PingSkin(Color.red, Resources.Load<Sprite>("textures/miscicons/texCriticallyHurtIcon"), 15f,
                "RADIALPINGS_MESSAGE_MAIN_HELP", PingIndicator.PingType.Default,
                Highlight.HighlightColor.teleporter, 2f));
            
            var RespondYesPingType = new PingType(null,
                (targetObj, tokens) => {
                    return true;
                }, MiscUtil.ModifyTargetSelf);
            RespondYesPingType.pingSkins.Add(new PingSkin(new Color(0.5f, 1f, 0.5f), Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsOIcon.png"), 8f,
                "RADIALPINGS_MESSAGE_MAIN_YES", PingIndicator.PingType.Default));

            PingCatalog.getAdditionalEntries += (list) => {
                list.Add(LookHerePingTypeTarget);
                list.Add(LookHerePingTypeNoTarget);
                list.Add(AggressivePingTypeEnemy);
                list.Add(AggressivePingTypeTeleporter);
                list.Add(AggressivePingTypeInteractable);
                list.Add(AggressivePingTypeNoTarget);
                list.Add(RespondNoPingType);
                list.Add(RespondHelpPingType);
                list.Add(RespondYesPingType);
            };

            buttonBindingInfos.Add(new BindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_TODRONES",
                sprite = Resources.Load<Sprite>("textures/miscicons/texWIPIcon"),
                iconColor = new Color(1f, 0.75f, 0.5f),
                hoverFillColor = new Color(1f, 0.75f, 0.5f, 0.5f),
                hoverActivationTime = 0.5f,
                onActivate = (sender, isHover) => {
                    /*if(isHover) {
                        var pingHelper = sender.GetComponent<PingMenuHelper>();
                        pingHelper.owner.GetComponent<PingMenuInstanceTracker>().latestMenu = DroneMenuBindings.instance.Instantiate(pingHelper.owner);
                    } else*/
                        CancelMenuAction(sender, isHover);
                }
            });

            buttonBindingInfos.Add(new PingBindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_LOOK",
                sprite = Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsLookIcon.png"),
                iconColor = Color.white,
                orderedTypes = new[] {LookHerePingTypeTarget, LookHerePingTypeNoTarget}
            });

            buttonBindingInfos.Add(new PingBindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_AGGRESSIVE",
                sprite = Resources.Load<Sprite>("textures/miscicons/texAttackIcon"),
                iconColor = new Color(1f, 0.5f, 0f),
                orderedTypes = new[] {AggressivePingTypeEnemy, AggressivePingTypeTeleporter, AggressivePingTypeInteractable, AggressivePingTypeNoTarget}
            });

            buttonBindingInfos.Add(new BindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_MOVE",
                sprite = Resources.Load<Sprite>("textures/miscicons/texSprintIcon"),
                iconColor = Color.green,
                onActivate = MoveOnlyPingMenuAction
            });

            buttonBindingInfos.Add(new BindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_TOPLAYERS",
                sprite = Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsPlayerIcon.png"),
                iconColor = new Color(0.5f, 0.5f, 1f),
                hoverFillColor = new Color(0.5f, 0.5f, 1f, 0.5f),
                hoverActivationTime = 0.5f,
                onActivate = (sender, isHover) => {
                    if(isHover) {
                        var pingHelper = sender.GetComponent<PingMenuHelper>();
                        pingHelper.owner.GetComponent<PingMenuInstanceTracker>().latestMenu = PlayersMenuBindings.instance.Instantiate(pingHelper.owner);
                    } else
                        CancelMenuAction(sender, isHover);
                }
            });

            buttonBindingInfos.Add(new PingBindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_NO",
                sprite = Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsXIcon.png"),
                iconColor = new Color(1f, 0.625f, 0.625f),
                orderedTypes = new[] {RespondNoPingType}
            });

            buttonBindingInfos.Add(new PingBindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_HELP",
                sprite = Resources.Load<Sprite>("textures/miscicons/texCriticallyHurtIcon"),
                iconColor = Color.red,
                orderedTypes = new[] {RespondHelpPingType}
            });

            buttonBindingInfos.Add(new PingBindingInfo {
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_YES",
                sprite = Resources.Load<Sprite>("@RadialPings:Assets/RadialPings/RadialPingsOIcon.png"),
                iconColor = new Color(0.625f, 1f, 0.625f),
                orderedTypes = new[] {RespondYesPingType}
            });

            innerDeadZoneBindingInfo = new BindingInfo {
                contextStringProvider = (sender) => {
                    var tgtObj = sender.GetComponent<PingMenuHelper>().lastPingInfo.targetGameObject;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(tgtObj, ref token)) return token;
                    MiscUtil.TryGetPurchaseAppend(tgtObj, ref token);
                    return token;
                },
                descriptionToken = "RADIALPINGS_CAPTION_MAIN_QUICK",
                onActivate = MiscUtil.DefaultPingMenuAction
            };

            thetaOffsetDegr = -22.5f;
        }

        protected internal override GameObject Instantiate(PlayerCharacterMasterController owner) {
            var menuObj = base.Instantiate(owner);

            var pingHelper = menuObj.AddComponent<PingMenuHelper>();
            pingHelper.owner = owner;
            pingHelper.TryUpdatePingInfo();

            return menuObj;
        }

        private void MoveOnlyPingMenuAction(ProceduralRadialMenu sender, bool isHover) {
            var pingHelper = sender.GetComponent<PingMenuHelper>();
            if(!pingHelper.BuildPosOnlyPingInfo(LayerIndex.entityPrecise.mask | LayerIndex.world.mask | LayerIndex.defaultLayer.mask, QueryTriggerInteraction.Collide))
                return;
            if(!pingHelper.TryDeductPingStock())
                return;

            var menuTracker = pingHelper.owner.GetComponent<PingMenuInstanceTracker>();
            if(menuTracker.currentPingIsCustom && pingHelper.owner.pingerController.pingIndicator) {
                GameObject.Destroy(pingHelper.owner.pingerController.pingIndicator.gameObject);
                pingHelper.owner.pingerController.pingIndicator = null;
            }
            menuTracker.currentPingIsCustom = false;

            pingHelper.owner.pingerController.SetCurrentPing(pingHelper.lastPingInfo);
        }
    }
}
