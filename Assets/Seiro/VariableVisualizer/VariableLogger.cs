using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Seiro.VariableVisualizer {

	[System.Serializable]
	public class VariableLogger {

		static readonly Color[] colors = { Color.red, Color.green, Color.blue, Color.cyan };

		public SerializedProperty property;
		public int maxCount;
		public int dimentions;

		public List<float[]> data;
		public bool showGraph;

		public VariableLogger(SerializedProperty property, int count) {
			SetLoggingProperty(property);
			this.maxCount = count;
		}

		public void LogData() {
			if(data == null) {
				// Debug.Log("リストの再取得");
				data = new List<float[]>(this.maxCount);
			}
			var d = GetRawValues(property);
			// Debug.Log(d[0]);
			data.Add(d);
			if(data.Count > maxCount) {
				data.RemoveAt(0);
			}
		}

		public void SetLoggingProperty(SerializedProperty property) {
			this.property = property;
			this.dimentions = GetDimentions(property.propertyType);
			if(this.dimentions <= 0) {
				throw new Exception("Unsupported property");
			}
		}

#if UNITY_EDITOR
		public void DrawGraph(Rect area) {

			if(data != null && data.Count > 0) {

				float max = data.Max(v => v.Max());
				float min = data.Min(v => v.Min());

				float dx = area.width / (data.Count - 1);
				float x0 = area.x;
				float y0 = area.y + area.height;

				// 最大値と最小値
				EditorGUI.LabelField(new Rect(area.x - 50, area.y, 40, 16), string.Format("{0:f3}", max));
				EditorGUI.LabelField(new Rect(area.x - 50, area.y + area.height - 16, 40, 16), string.Format("{0:f3}", min));

				// グラフ
				for(int dim = 0; dim < dimentions; ++dim) {
					var values = new Vector3[data.Count];
					for(int i = 0; i < data.Count; ++i) {
						values[i] = new Vector3(
							x0 + dx * i,
							y0 - Mathf.InverseLerp(min, max, data[i][dim]) * area.height,
							0
						);
					}
					Handles.color = colors[dim];
					Handles.DrawAAPolyLine(values.ToArray());
				}
			}

			// 外枠
			Handles.color = Color.white;
			Handles.DrawSolidRectangleWithOutline(area, new Color(0, 0, 0, 0.1f), Color.white);
		}
#endif

		static int GetDimentions(SerializedPropertyType type) {
			switch(type) {
				case SerializedPropertyType.Float: return 1;
				case SerializedPropertyType.Integer: return 1;
				case SerializedPropertyType.Vector2: return 2;
				case SerializedPropertyType.Vector3: return 3;
				case SerializedPropertyType.Vector4: return 4;
				case SerializedPropertyType.Quaternion: return 4;
				case SerializedPropertyType.Color: return 4;
				default: return -1;
			}
		}

		static float[] GetRawValues(SerializedProperty property) {
			switch(property.propertyType) {
				case SerializedPropertyType.Float:
					return new float[] { property.floatValue };
				case SerializedPropertyType.Integer:
					return new float[] { property.intValue };
				case SerializedPropertyType.Vector2:
					var v2 = property.vector2Value;
					return new float[] { v2[0], v2[1] };
				case SerializedPropertyType.Vector3:
					var v3 = property.vector3Value;
					return new float[] { v3[0], v3[1], v3[2] };
				case SerializedPropertyType.Vector4:
					var v4 = property.vector4Value;
					return new float[] { v4[0], v4[1], v4[2], v4[3] };
				case SerializedPropertyType.Quaternion:
					var q = property.quaternionValue;
					return new float[] { q[0], q[1], q[2], q[3] };
				case SerializedPropertyType.Color:
					var c = property.colorValue;
					return new float[] { c[0], c[1], c[2], c[3] };
				default:
					return null;
			}
		}
	}
}