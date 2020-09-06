﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.UI;

namespace ThinkInvisible.RadialPings {
    public static class CustomPingCatalog {
        public struct CustomPingSkin {
            public Color pingColor;
            public Color textColor;
            public Color spriteColor;
            public Sprite sprite;
            public float duration;

            public PingIndicator.PingType basePingType;

            public string chatToken;

            public bool doHighlight;
            public Highlight.HighlightColor highlightColor;
            public float highlightStrength;

            public CustomPingSkin(Color color, Sprite sprite, float duration, string chatToken, PingIndicator.PingType pingType) {
                this.pingColor = color;
                this.textColor = color;
                this.spriteColor = color;
                this.sprite = sprite;
                this.duration = duration;
                this.chatToken = chatToken;
                this.basePingType = pingType;
                this.doHighlight = false;
                this.highlightColor = Highlight.HighlightColor.unavailable;
                this.highlightStrength = 0f;
            }

            public CustomPingSkin(Color pingColor, Color textColor, Color spriteColor, Sprite sprite, float duration, string chatToken, PingIndicator.PingType pingType)
                : this(pingColor, sprite, duration, chatToken, pingType) {
                this.textColor = textColor;
                this.spriteColor = spriteColor;
            }

            public CustomPingSkin(Color color, Sprite sprite, float duration, string chatToken, PingIndicator.PingType pingType, Highlight.HighlightColor highlightColor, float highlightStrength)
                : this(color, sprite, duration, chatToken, pingType) {
                this.doHighlight = true;
                this.highlightColor = highlightColor;
                this.highlightStrength = highlightStrength;
            }

            public CustomPingSkin(Color pingColor, Color textColor, Color spriteColor, Sprite sprite, float duration, string chatToken, PingIndicator.PingType pingType, Highlight.HighlightColor highlightColor, float highlightStrength)
                : this(pingColor, textColor, spriteColor, sprite, duration, chatToken, pingType) {
                this.doHighlight = true;
                this.highlightColor = highlightColor;
                this.highlightStrength = highlightStrength;
            }
        }
        public class CustomPingData : R2API.Networking.Interfaces.ISerializableObject {
            public PlayerCharacterMasterController owner;
            public List<GameObject> targets;
            public Vector3 origin;
            public Vector3 normal;

            public void Serialize(NetworkWriter writer) {
                writer.Write(owner.netId);
                writer.Write(targets.Count);
                for(var i = 0; i < targets.Count; i++) {
                    writer.Write(targets[i]);
                }
                writer.Write(origin);
                writer.Write(normal);
            }
            public void Deserialize(NetworkReader reader) {
                var netId = reader.ReadNetworkId();
                var ownerObj = Util.FindNetworkObject(netId);
                owner = ownerObj?.GetComponent<PlayerCharacterMasterController>() ?? null;
                var targetCount = reader.ReadInt32();
                targets = new List<GameObject>();
                for(var i = 0; i < targetCount; i++)
                    targets.Add(reader.ReadGameObject());
                origin = reader.ReadVector3();
                normal = reader.ReadVector3();
            }
        }
        public class CustomPingType {
            public readonly List<CustomPingSkin> pingSkins = new List<CustomPingSkin>();

            public delegate bool TryApplyDelegate(CustomPingData pingData, List<string> formatInserts);
            public delegate int ModifyDelegate(ref CustomPingData pingData);

            public readonly string previewToken;
            ///<summary>Used to retrieve object name information and pass/fail status for both previews and pings.</summary>
            public readonly TryApplyDelegate tryApply;
            ///<summary>Used to switch targets or other ping data before applying the ping, and to select a skin index.</summary>
            public readonly ModifyDelegate modifyAndSelectSkin;

            public CustomPingType(string previewToken, TryApplyDelegate tryApply, ModifyDelegate modify = null) {
                this.previewToken = previewToken;
                this.tryApply = tryApply;
                this.modifyAndSelectSkin = modify;
            }

            public int catalogIndex {get; internal set;} = -1;
        }

        private static CustomPingType[] pingTypes = Array.Empty<CustomPingType>();

        public static int pingTypeCount {get; private set;}
        public static int pingContextCount {get; private set;}

        public static CustomPingType Get(int index) {
            return HGArrayUtilities.GetSafe(pingTypes, index);
        }

        internal static void Init() {
            RadialPingsPlugin.logger.LogMessage("Initializing ping catalog...");
            List<CustomPingType> newPingTypes = new List<CustomPingType>();

            getAdditionalEntries?.Invoke(newPingTypes);

            newPingTypes = newPingTypes.Distinct().ToList();

            for(var i = 0; i < newPingTypes.Count; i++) {
                if(newPingTypes[i].pingSkins.Count == 0) {
                    RadialPingsPlugin.logger.LogError($"CustomPingType {newPingTypes[i].GetType().Name} has 0 skins and will not be registered.");
                    continue;
                }
                if(newPingTypes[i].tryApply == null) {
                    RadialPingsPlugin.logger.LogError($"CustomPingType {newPingTypes[i].GetType().Name} has no TryApply and will not be registered.");
                    continue;
                }
                newPingTypes[i].catalogIndex = i;
            }

            pingTypes = newPingTypes.ToArray();
            pingTypeCount = newPingTypes.Count;
            RadialPingsPlugin.logger.LogMessage($"Ping catalog ready ({pingTypeCount} types)");
        }

        public delegate void GetAdditionalEntries(List<CustomPingType> types);
        public static event GetAdditionalEntries getAdditionalEntries;
    }
}
