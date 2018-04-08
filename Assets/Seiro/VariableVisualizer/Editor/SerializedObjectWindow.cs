using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace Seiro.VariableVisualizer {


	public class SerializedObjectWindow : EditorWindow {

		Object mObj;
		Vector2 mScrollPos;
		string mResult;

		string[] mComponentsPath;

		[MenuItem("Window/Seiro/SerializedObjectWindow")]
		static void Open() {
			var window = GetWindow<SerializedObjectWindow>();
			window.titleContent = new GUIContent("SerializedObjectWindow");
		}

		void OnGUI() {
			EditorGUI.BeginChangeCheck();
			mObj = EditorGUILayout.ObjectField(mObj, typeof(Object), true);
			if(EditorGUI.EndChangeCheck()) {
				if(mObj == null) {
					mResult = null;
				} else {
					var so = new SerializedObject(mObj);
					var sp = so.GetIterator();
					var builder = new StringBuilder();

					while(sp.Next(true)) {
						builder.AppendLine(sp.propertyPath);
					}
					mResult = builder.ToString();
				}
			}

			if(string.IsNullOrEmpty(mResult)) {
				return;
			}



			mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
			EditorGUILayout.TextArea(mResult);
			EditorGUILayout.EndScrollView();
		}
	}
}