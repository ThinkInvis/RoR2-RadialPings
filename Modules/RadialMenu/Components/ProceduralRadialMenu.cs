using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThinkInvisible.RadialPings {
	///<summary>Handles layout and cursor events for related instances of ProceduralRadialButton. Use RadialMenuBindings to create an instance.</summary>
	public class ProceduralRadialMenu : MonoBehaviour {
		internal static GameObject buttonPrefab = null;

		//behavior
		public float deadZoneSplitRadiusNorm = 1f;

		public ProceduralRadialButton.OnActivate onInnerDeadZoneActivate;
		public ProceduralRadialButton.OnActivate onOuterDeadZoneActivate;
		public ProceduralRadialButton.ContextStringProvider innerDeadZoneContext;
		public ProceduralRadialButton.ContextStringProvider outerDeadZoneContext;
		public string innerDeadZoneToken;
		public string outerDeadZoneToken;

		public event ProceduralRadialButton.OnActivate onAnyActivate;

		//display
		public float inOutAnimSpeed = 0.2f;
		public float hoverScale = 1.1f;
		public float unhoverTrans = 0.5f;
		public float hoverTrans = 0.9f;
		public float extraOutroScale = 1f;
		public Camera targetCam;
		public float thetaOffsetDegr = 0f;
		public float inOutTwistAngleDegr = 45f;

		float visualScale = 0f;
		float scaleVel = 0f;
		float visualAlpha = 0f;
		float alphaVel = 0f;

		private float displayOnlyThetaOffsetDegr = 0f;


		List<ProceduralRadialButton> buttons = new List<ProceduralRadialButton>();
		
		public enum InOutState {
			Intro,
			Active,
			Outro
		}
		public InOutState inOutState {get; private set;} = InOutState.Intro;
		float inOutTimer = 0f;
		bool layoutDirty = true;
		float buttonThetaRad = 0f;
		
		RectTransform myRect;
		TextMeshPro mainCaption;
		TextMeshPro contextCaption;

		int selectedButton;
		public enum SelectionRegion {
			InnerDeadZone,
			Button,
			OuterDeadZone
		}
		SelectionRegion selectedRegion;

		void OnEnable() {
			myRect = GetComponent<RectTransform>();
			mainCaption = transform.Find("DisplayContainer").Find("Caption").GetComponent<TextMeshPro>();
			contextCaption = transform.Find("DisplayContainer").Find("ContextCaption").GetComponent<TextMeshPro>();
			mainCaption.fontSizeMax *= 10;
			contextCaption.fontSizeMax *= 10;
			inOutState = InOutState.Intro;
			inOutTimer = 0f;
			displayOnlyThetaOffsetDegr = inOutTwistAngleDegr;
		}

		void Update() {
			if(layoutDirty) {
				layoutDirty = false;
				UpdateLayout();
			}
			if(inOutState != InOutState.Outro) UpdateSelection();
			UpdateDisplay();
		}
		
		void UpdateSelection() {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, targetCam, out Vector2 cursorPosNorm);
			
			cursorPosNorm = Rect.PointToNormalized(myRect.rect, cursorPosNorm);
			
			float cursorThetaRad = Mathf.Atan2(cursorPosNorm.y-0.5f, cursorPosNorm.x-0.5f);
			if(cursorThetaRad < 0) cursorThetaRad += Mathf.PI * 2f;
			float cursorDistNorm = ((cursorPosNorm - new Vector2(0.5f, 0.5f))*2f).magnitude;
			
			float cursorThetaWrapDown = cursorThetaRad - Mathf.PI * 2;
			float cursorThetaWrapUp = cursorThetaRad + Mathf.PI * 2;

			selectedButton = -1;

			var thetaOffsetRad = thetaOffsetDegr * Mathf.PI/180f;

			for(int i = 0; i < buttons.Count; i++) {
				float targetThetaMin = i * buttonThetaRad + thetaOffsetRad;
				float targetThetaMax = (i + 1) * buttonThetaRad + thetaOffsetRad;
				if((cursorThetaRad > targetThetaMin && cursorThetaRad < targetThetaMax)
					|| (cursorThetaWrapDown > targetThetaMin && cursorThetaWrapDown < targetThetaMax)
					|| (cursorThetaWrapUp > targetThetaMin && cursorThetaWrapUp < targetThetaMax)) {
					if(cursorDistNorm > buttons[i].outerRadiusFrac) {
						selectedRegion = SelectionRegion.OuterDeadZone;
						buttons[i].hoverActive = false;
					} else if(cursorDistNorm < buttons[i].innerRadiusFrac) {
						selectedRegion = SelectionRegion.InnerDeadZone;
						buttons[i].hoverActive = false;
					} else {
						selectedButton = i;
						selectedRegion = SelectionRegion.Button;
						buttons[i].hoverActive = true;
						if(buttons[i].isHoverComplete) {
							Activate(true);
							return;
						}
					}
				} else {
					buttons[i].hoverActive = false;
				}
			}

			if(selectedButton == -1) selectedRegion = (cursorDistNorm > deadZoneSplitRadiusNorm) ? SelectionRegion.OuterDeadZone : SelectionRegion.InnerDeadZone;
		}

		void UpdateDisplay() {
			float targetAlpha = 1f;
			float targetScale = 1f;
			if(inOutState == InOutState.Intro) {
				inOutTimer += Time.unscaledDeltaTime;
				float timerAmt = inOutTimer/inOutAnimSpeed;
				displayOnlyThetaOffsetDegr = (1f - timerAmt) * inOutTwistAngleDegr;
				targetScale = timerAmt;
				targetAlpha = timerAmt;
				if(inOutTimer >= inOutAnimSpeed) {
					displayOnlyThetaOffsetDegr = 0f;
					inOutState = InOutState.Active;
					inOutTimer = 0;
				}
			} else if(inOutState == InOutState.Outro) {
				inOutTimer += Time.unscaledDeltaTime;
				float timerAmt = inOutTimer/inOutAnimSpeed;
				displayOnlyThetaOffsetDegr = timerAmt * -inOutTwistAngleDegr;
				targetScale = timerAmt*extraOutroScale + 1f;
				targetAlpha = 1f - timerAmt;
				if(inOutTimer >= inOutAnimSpeed) {
					GameObject.Destroy(gameObject);
				}
			}
			
			visualScale = Mathf.SmoothDamp(visualScale, targetScale, ref scaleVel, 0.125f, 100f, Time.unscaledDeltaTime);
			visualAlpha = Mathf.SmoothDamp(visualAlpha, targetAlpha, ref alphaVel, 0.125f, 100f, Time.unscaledDeltaTime);
			transform.Find("DisplayContainer").localScale = new Vector3(visualScale, visualScale, 1f);

			var buttonThetaDegr = 360f/buttons.Count;
			for(int i = 0; i < buttons.Count; i++) {
				buttons[i].targetAlphaMul = targetAlpha;
				buttons[i].targetRotDegr = displayOnlyThetaOffsetDegr;
				buttons[i].directRotDegr = thetaOffsetDegr + i * buttonThetaDegr;
				buttons[i].targetScale = buttons[i].hoverActive ? hoverScale : 1f;
			}
			
			string mainCaptionToken = "INDEX ERROR!";
			ProceduralRadialButton.ContextStringProvider contextProvider = null;
			switch(selectedRegion) {
				case SelectionRegion.InnerDeadZone:
					mainCaptionToken = innerDeadZoneToken;
					contextProvider = innerDeadZoneContext;
					break;
				case SelectionRegion.OuterDeadZone:
					mainCaptionToken = outerDeadZoneToken;
					contextProvider = outerDeadZoneContext;
					break;
				case SelectionRegion.Button:
					if(selectedButton == -1) break;
					mainCaptionToken = buttons[selectedButton].descriptionToken;
					contextProvider = buttons[selectedButton].contextStringProvider;
					break;
			}
			mainCaption.text = Language.GetString(mainCaptionToken);
			contextCaption.text = contextProvider?.Invoke(this) ?? "";
			mainCaption.color = new Color(1f, 1f, 1f, visualAlpha);
			contextCaption.color = new Color(1f, 1f, 1f, visualAlpha);
		}

		void UpdateLayout() {
			buttonThetaRad = Mathf.PI*2/buttons.Count;
			for(int i = 0; i < buttons.Count; i++) {
				buttons[i].GetComponent<ProceduralRadialButton>().widthAngleRad = buttonThetaRad;
			}
		}
		
		public GameObject Add(string descriptionToken, ProceduralRadialButton.OnActivate onActivate, ProceduralRadialButton.ContextStringProvider contextStringProvider, Sprite sprite, Color iconColor) {
			var newBtn = GameObject.Instantiate(buttonPrefab, transform.Find("DisplayContainer"));
			var btnCpt = newBtn.GetComponent<ProceduralRadialButton>();
			btnCpt.descriptionToken = descriptionToken;
			btnCpt.contextStringProvider = contextStringProvider;
			btnCpt.onActivate += onActivate;
			btnCpt.hoverActivationTime = -1f;
			btnCpt.iconTint = iconColor;
			newBtn.transform.Find("Icon").GetComponent<Image>().sprite = sprite;
			
			float targetAlpha = 1f;
			if(inOutState == InOutState.Intro) {
				targetAlpha = inOutTimer/inOutAnimSpeed;
			} else if(inOutState == InOutState.Outro) {
				targetAlpha = 1f - inOutTimer/inOutAnimSpeed;
			}

			btnCpt.visualAlphaMul = targetAlpha;
			btnCpt.visualRotDegr = thetaOffsetDegr + displayOnlyThetaOffsetDegr;

			buttons.Add(btnCpt);
			
			layoutDirty = true;
			
			return newBtn;
		}

		public GameObject Add(string descriptionToken, ProceduralRadialButton.OnActivate onActivate, ProceduralRadialButton.ContextStringProvider contextStringProvider, Sprite sprite, Color iconColor, Color hoverFillColor, float hoverActivationTime, float hoverColorWobble) {
			var newBtn = Add(descriptionToken, onActivate, contextStringProvider, sprite, iconColor);
			var btnCpt = newBtn.GetComponent<ProceduralRadialButton>();
			btnCpt.hoverActivationTime = hoverActivationTime;
			btnCpt.hoverFillColor = hoverFillColor;
			btnCpt.hoverFillColorWobble = hoverColorWobble;
			return newBtn;
		}
		
		public void RemoveAt(int index) {
			buttons.RemoveAt(index);
			layoutDirty = true;
		}

		public void Activate(bool isHover) {
			Activate(selectedRegion, isHover);
		}

		public void Activate(SelectionRegion region, bool isHover) {
			if(inOutState == InOutState.Outro) return;
			GetComponent<CursorOpener>().enabled = false;
			onAnyActivate?.Invoke(this, isHover);
			switch(region) {
				case SelectionRegion.InnerDeadZone:
					inOutState = InOutState.Outro;
					onInnerDeadZoneActivate?.Invoke(this, isHover);
					break;
				case SelectionRegion.OuterDeadZone:
					inOutState = InOutState.Outro;
					onOuterDeadZoneActivate?.Invoke(this, isHover);
					break;
				case SelectionRegion.Button:
					if(selectedButton < 0 || selectedButton > buttons.Count) {
						RadialPingsPlugin.logger.LogError("ProceduralRadialMenu was activated with a bad button index selected, possible layout failure");
						inOutState = InOutState.Outro;
						return;
					}
					if(buttons[selectedButton].closeOnActivate) {
						inOutState = InOutState.Outro;
					}
					buttons[selectedButton].Activate(this, isHover);
					break;
			}
		}
	}
}