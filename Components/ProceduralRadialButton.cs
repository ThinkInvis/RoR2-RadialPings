using UnityEngine;
using UnityEngine.UI;

namespace ThinkInvisible.RadialPings {
	public class ProceduralRadialButton : MaskableGraphic {
		const float HOVER_TIME_ANIM_SPEED = 0.0625f;
		const float INOUT_ANIM_SPEED = 0.125f;
		const int RADIAL_SUBDIVS_RATE = 256; //number of subdivs per full circle

		//used in menu:
		public string descriptionToken;
		public delegate string ContextStringProvider(ProceduralRadialMenu sender);
		public ContextStringProvider contextStringProvider;
		public delegate void OnActivate(ProceduralRadialMenu sender, bool isHover);
		public event OnActivate onActivate;
		public bool closeOnActivate = true;
		public Color iconTint;

		//hover behavior:
		public float baseAlpha = 0.5f;
		public float hoverAlpha = 0.9f;
		public Color hoverFillColor;
		public float hoverFillColorWobble = 0.1f;
		public float hoverActivationTime;
		public bool hoverActive = false;
		float hoverTime = 0f;
		float visualHoverTime = 0f;
		float hoverTimeVel = 0f;

		//for in/out animation:
		public float targetScale = 0f;
		public float visualScale = 0f;
		float scaleVel = 0f;
		public float targetAlphaMul = 0f;
		public float visualAlphaMul = 0f;
		float alphaMulVel = 0f;
		public float directRotDegr = 0f;
		public float targetRotDegr = 0f;
		public float visualRotDegr = 0f;
		float rotVel = 0f;

		public bool isHoverComplete {get; private set;} = false;
		
		[SerializeField]
		float _innerRadiusFrac = 0.25f;
		public float innerRadiusFrac {get => _innerRadiusFrac; set {_innerRadiusFrac = value; SetCustomLayoutDirty();}}
		[SerializeField]
		float _widthAngleRad = Mathf.PI/4f;
		public float widthAngleRad {get => _widthAngleRad; set {_widthAngleRad = value; SetCustomLayoutDirty();}}
		
		bool _customLayoutDirty = true;
		
		[SerializeField]
		Texture _mainTex;
		
		public Texture mainTex {
			get => _mainTex;
			set {
				if(_mainTex == value) return;
				_mainTex = value;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}
		
		public override Texture mainTexture {
			get => _mainTex ?? s_WhiteTexture;
		}

		public void Activate(ProceduralRadialMenu sender, bool isHover) {
			onActivate?.Invoke(sender, isHover);
		}

		void SetCustomLayoutDirty() {
			_customLayoutDirty = true;
			SetVerticesDirty();
			SetMaterialDirty();
		}
		
		void Update() {
			UpdateHover();
			//if(_customLayoutDirty || Application.isEditor) {
				UpdateIcon();
				//_customLayoutDirty = false;
			//}
		}

		void UpdateHover() {
			var hoverObj = transform.Find("FillOverlay");
			if(hoverActive) {
				hoverTime += Time.unscaledDeltaTime;
			} else {
				hoverTime = 0f;
			}
			
			visualAlphaMul = Mathf.SmoothDamp(visualAlphaMul, targetAlphaMul, ref alphaMulVel, INOUT_ANIM_SPEED, 100f, Time.unscaledDeltaTime);
			visualScale = Mathf.SmoothDamp(visualScale, targetScale, ref scaleVel, INOUT_ANIM_SPEED, 100f, Time.unscaledDeltaTime);
			visualRotDegr = Mathf.SmoothDampAngle(visualRotDegr, targetRotDegr, ref rotVel, INOUT_ANIM_SPEED, 36000f, Time.unscaledDeltaTime);
			
			float finalAlpha = visualAlphaMul * (hoverActive ? hoverAlpha : baseAlpha);
			color = new Color(color.r, color.g, color.b, finalAlpha);

			transform.localScale = new Vector3(visualScale, visualScale, 1f);
			transform.localRotation = Quaternion.Euler(0f, 0f, visualRotDegr+directRotDegr);

			if(hoverActivationTime <= 0f) {
				hoverObj.localScale = Vector3.zero;
			} else {
				if(hoverTime >= hoverActivationTime) isHoverComplete = true;
				visualHoverTime = Mathf.SmoothDamp(visualHoverTime, hoverTime, ref hoverTimeVel, HOVER_TIME_ANIM_SPEED, float.PositiveInfinity, Time.unscaledDeltaTime);
				float hoverFrac = Mathf.Min(visualHoverTime/hoverActivationTime*(1f-innerRadiusFrac)+innerRadiusFrac, 1f);
				hoverObj.localScale = new Vector3(hoverFrac, hoverFrac, 1f);
				hoverObj.gameObject.GetComponent<Image>().color = hoverFillColor * (1f+hoverFillColorWobble*Mathf.Sin(Time.unscaledTime));
			}
		}
		
		void UpdateIcon() {
			var offset = new Vector2(0,0) + rectTransform.pivot;
			
			var outerRadius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height)/2f;
			var innerRadius = outerRadius * innerRadiusFrac;
			var centerRadius = (outerRadius+innerRadius)/2f;
			var iconTheta = widthAngleRad/2f;
			var iconX = Mathf.Cos(iconTheta)*centerRadius + offset.x;
			var iconY = Mathf.Sin(iconTheta)*centerRadius + offset.y;
			
			var icon = transform.Find("Icon");
			icon.rotation = Quaternion.Euler(0f, 0f, visualRotDegr);
			icon.GetComponent<Image>().color = color * iconTint;

			var iconTsf = icon.GetComponent<RectTransform>();
			iconTsf.localPosition = new Vector3(iconX, iconY, -1f);
			float maxRadius = 2f/5f * (-2f * innerRadius + Mathf.Sqrt(5*outerRadius*outerRadius-innerRadius*innerRadius));
			float totalDist = 2 * centerRadius * Mathf.Sin(iconTheta);
			float minRadius = Mathf.Min(maxRadius, iconTheta < Mathf.PI/2f ? totalDist : maxRadius);
			iconTsf.sizeDelta = new Vector2(minRadius, minRadius);
			
			//todo: this size calculation works for fitting circles, but anything outside the circle can clip; need to find a way to adjust for an axis-aligned square
		}

		protected override void OnPopulateMesh(VertexHelper vh) {
			vh.Clear();
			
			var offset = rectTransform.pivot;
			var outerRadius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height)/2f;
			var innerRadius = outerRadius * innerRadiusFrac;
			
			var vertIndex = vh.currentVertCount;
			
			//todo: margin
			
			var radialSubdivs = Mathf.Ceil(widthAngleRad/(Mathf.PI*2)*RADIAL_SUBDIVS_RATE);
				
			for(int i = 0; i < radialSubdivs; i++) {
				var theta = i*widthAngleRad/(radialSubdivs - 1);
				var tCos = Mathf.Cos(theta);
				var tSin = Mathf.Sin(theta);
				
				//exterior
				vh.AddVert(new UIVertex {
					color = color,
					position = new Vector2(tCos*innerRadius+offset.x, tSin*innerRadius+offset.y),
					uv0 = new Vector2(i/(radialSubdivs-1f), 0f)
				});
				vh.AddVert(new UIVertex {
					color = color,
					position = new Vector2(tCos*outerRadius+offset.x, tSin*outerRadius+offset.y),
					uv0 = new Vector2(i/(radialSubdivs-1f), 1f)
				});
				
				if(i > 0) {
					vh.AddTriangle(vertIndex, vertIndex+1, vertIndex+2);
					vh.AddTriangle(vertIndex+2, vertIndex+3, vertIndex+1);
					vertIndex += 2;
				}
			}
			
		}
		
		protected override void OnRectTransformDimensionsChange() {
			base.OnRectTransformDimensionsChange();
			SetVerticesDirty();
			SetMaterialDirty();
		}
	}
}
