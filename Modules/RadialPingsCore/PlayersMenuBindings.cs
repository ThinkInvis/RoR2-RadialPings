using RoR2;
using RoR2.UI;
using UnityEngine;
using static ThinkInvisible.RadialPings.PingCatalog;

namespace ThinkInvisible.RadialPings {
    internal class PlayersMenuBindings : RadialMenuBindings<PlayersMenuBindings> {
        readonly PingType PlayerPingTypeNoTarget;
        readonly PingType PlayerPingTypePickup;
        readonly PingType PlayerPingTypeInteractable;
        readonly PingType PlayerPingTypeEnemy;
        readonly PingType PlayerPingTypeAlly;
        readonly PingType PlayerPingTypeRecipient;

        internal PlayersMenuBindings() {
            PlayerPingTypeNoTarget = new PingType("RADIALPINGS_CONTEXT_PLAYER_NOTARGET",
                (pingData, tokens) => {
                    tokens.Add(Util.GetBestMasterName(pingData.targets[1].GetComponent<PlayerCharacterMasterController>().master));
                    return true;
                }, MiscUtil.ModifyTarget2PRecipient);
            PlayerPingTypeNoTarget.pingSkins.Add(new PingSkin(new Color(0.125f, 0.5f, 0.125f, 0.25f), LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texSprintIcon"), 15f,
                "RADIALPINGS_MESSAGE_PLAYER_NOTARGET", PingIndicator.PingType.Enemy));
            PlayerPingTypeNoTarget.pingSkins.Add(new PingSkin(Color.green, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texSprintIcon"), 15f,
                "RADIALPINGS_MESSAGE_PLAYER_NOTARGET_2P", PingIndicator.PingType.Enemy, Highlight.HighlightColor.pickup, 1f));

            PlayerPingTypePickup = new PingType("RADIALPINGS_CONTEXT_PLAYER_PICKUP",
                (pingData, tokens) => {
                    var targetObj = pingData.targets[0];
                    if(targetObj == null) return false;
                    if(!targetObj.GetComponent<GenericPickupController>() && !targetObj.GetComponent<PickupPickerController>()) return false;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(targetObj, ref token)) return false;
                    tokens.Add(Util.GetBestMasterName(pingData.targets[1].GetComponent<PlayerCharacterMasterController>().master));
                    tokens.Add(token);
                    return true;
                }, MiscUtil.CheckTarget2PRecipient);
            PlayerPingTypePickup.pingSkins.Add(new PingSkin(new Color(0.5f, 0.25f, 0.125f, 0.25f), RadialPingsPlugin.resources.LoadAsset<Sprite>("Assets/RadialPings/RadialPingsNoLootIcon.png"), 30f,
                "RADIALPINGS_MESSAGE_PLAYER_PICKUP", PingIndicator.PingType.Interactable));
            PlayerPingTypePickup.pingSkins.Add(new PingSkin(Color.yellow, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texLootIconOutlined"), 30f,
                "RADIALPINGS_MESSAGE_PLAYER_PICKUP_2P", PingIndicator.PingType.Interactable, Highlight.HighlightColor.pickup, 1f));
            
            PlayerPingTypeInteractable = new PingType("RADIALPINGS_CONTEXT_PLAYER_INTERACTABLE",
                (pingData, tokens) => {
                    var targetObj = pingData.targets[0];
                    if(targetObj == null) return false;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(targetObj, ref token) || (!MiscUtil.TryGetPurchaseAppend(targetObj, ref token) && targetObj.GetComponent<IInteractable>() == null)) return false;
                    tokens.Add(Util.GetBestMasterName(pingData.targets[1].GetComponent<PlayerCharacterMasterController>().master));
                    tokens.Add(token);
                    return true;
                }, MiscUtil.CheckTarget2PRecipient);
            PlayerPingTypeInteractable.pingSkins.Add(new PingSkin(new Color(0.5f, 0.25f, 0.125f, 0.25f), RadialPingsPlugin.resources.LoadAsset<Sprite>("Assets/RadialPings/RadialPingsNoLootIcon.png"), 30f,
                "RADIALPINGS_MESSAGE_PLAYER_INTERACTABLE", PingIndicator.PingType.Interactable));
            PlayerPingTypeInteractable.pingSkins.Add(new PingSkin(Color.yellow, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texLootIconOutlined"), 30f,
                "RADIALPINGS_MESSAGE_PLAYER_INTERACTABLE_2P", PingIndicator.PingType.Interactable, Highlight.HighlightColor.pickup, 1f));
            
            PlayerPingTypeEnemy = new PingType("RADIALPINGS_CONTEXT_PLAYER_ENEMY",
                (pingData, tokens) => {
                    var targetObj = pingData.targets[0];
                    if(targetObj == null || !targetObj.GetComponent<CharacterBody>()) return false;
                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(targetObj, ref token)) return false;
                    tokens.Add(Util.GetBestMasterName(pingData.targets[1].GetComponent<PlayerCharacterMasterController>().master));
                    tokens.Add(token);
                    return true;
                }, MiscUtil.CheckTarget2PRecipient);
            PlayerPingTypeEnemy.pingSkins.Add(new PingSkin(new Color(0.5f, 0.25f, 0.125f, 0.25f), LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texAttackIcon"), 15f,
                "RADIALPINGS_MESSAGE_PLAYER_ENEMY", PingIndicator.PingType.Enemy));
            PlayerPingTypeEnemy.pingSkins.Add(new PingSkin(Color.red, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texAttackIcon"), 15f,
                "RADIALPINGS_MESSAGE_PLAYER_ENEMY_2P", PingIndicator.PingType.Enemy, Highlight.HighlightColor.teleporter, 1f));

            PlayerPingTypeAlly = new PingType("RADIALPINGS_CONTEXT_PLAYER_ALLY",
                (pingData, tokens) => {
                    var targetObj = pingData.targets[0];
                    if(targetObj == null) return false;
                    var body = targetObj.GetComponent<CharacterBody>();
                    if(!body || !body.teamComponent) return false;

                    var ownerBody = pingData.owner.master.GetBody();
                    if(!ownerBody || !ownerBody.teamComponent || ownerBody.teamComponent.teamIndex != body.teamComponent.teamIndex) return false;

                    string token = null;
                    if(!MiscUtil.TryGetObjDisplayName(targetObj, ref token)) return false;
                    if(body.master) token = Util.GetBestMasterName(body.master) ?? token;
                    tokens.Add(Util.GetBestMasterName(pingData.targets[1].GetComponent<PlayerCharacterMasterController>().master));
                    tokens.Add(token);
                    return true;
                }, MiscUtil.CheckTarget2PRecipient);
            PlayerPingTypeAlly.pingSkins.Add(new PingSkin(new Color(0.125f, 0.5f, 0.125f, 0.25f), LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texSprintIcon"), 15f,
                "RADIALPINGS_MESSAGE_PLAYER_ALLY", PingIndicator.PingType.Enemy));
            PlayerPingTypeAlly.pingSkins.Add(new PingSkin(Color.green, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texSprintIcon"), 15f,
                "RADIALPINGS_MESSAGE_PLAYER_ALLY_2P", PingIndicator.PingType.Enemy, Highlight.HighlightColor.pickup, 1f));

            PlayerPingTypeRecipient = new PingType("RADIALPINGS_CONTEXT_PLAYER_RECIPIENT",
                (pingData, tokens) => {
                    var targetObj = pingData.targets[0];
                    if(targetObj == null) return false;
                    var body = targetObj.GetComponent<CharacterBody>();
                    if(!body) return false;

                    var targetPcmc = pingData.targets[1].GetComponent<PlayerCharacterMasterController>();
                    var targetBody = targetPcmc.master.GetBody();
                    if(!targetBody || targetBody != body) return false;

                    tokens.Add(Util.GetBestMasterName(targetPcmc.master));
                    return true;
                }, MiscUtil.CheckTarget2PRecipient);
            PlayerPingTypeRecipient.pingSkins.Add(new PingSkin(Color.white, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texCursor3"), 1f,
                "RADIALPINGS_MESSAGE_PLAYER_RECIPIENT", PingIndicator.PingType.Default));
            PlayerPingTypeRecipient.pingSkins.Add(new PingSkin(Color.white, LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texCursor3"), 5f,
                "RADIALPINGS_MESSAGE_PLAYER_RECIPIENT_2P", PingIndicator.PingType.Default));

            PingCatalog.getAdditionalEntries += (list) => {
                list.Add(PlayerPingTypeNoTarget);
                list.Add(PlayerPingTypePickup);
                list.Add(PlayerPingTypeInteractable);
                list.Add(PlayerPingTypeEnemy);
                list.Add(PlayerPingTypeAlly);
                list.Add(PlayerPingTypeRecipient);
            };
        }

        protected internal override GameObject Instantiate(PlayerCharacterMasterController owner) {
            buttonBindingInfos.Clear();
            thetaOffsetDegr = 0f;
            foreach(var player in PlayerCharacterMasterController._instancesReadOnly) {
                //if(player == owner) continue;
                var srcTex = player.master.GetBody().portraitIcon; //networked
                var destSprite = Sprite.Create((Texture2D)srcTex, new Rect(0, 0, srcTex.width, srcTex.height), new Vector2(0.5f, 0.5f));
                buttonBindingInfos.Add(new DirectedPingBindingInfo {
                    iconColor = Color.white,
                    descriptionToken = $"{player.GetDisplayName()}",
                    sprite = destSprite,
                    targetPCMC = player,
                    orderedTypes = new[] {PlayerPingTypeRecipient, PlayerPingTypeAlly, PlayerPingTypeEnemy, PlayerPingTypePickup, PlayerPingTypeInteractable, PlayerPingTypeNoTarget}
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
