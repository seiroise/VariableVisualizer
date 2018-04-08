using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace Seiro.VariableVisualizer {

	[System.Serializable]
	public class PropertyLogger {
		
		public GameObject mGameObject;

		public Component[] mHasComponents;
		public string[] mHasComponentNames;

		public int mSelectedIndexOfComponent = -1;
		public SerializedObject mSerializedComponent;

		public string[] mHasPropertyPaths;
		public string[] mHasPropertyNames;

		public int mSelectedIndexOfProperty = -1;
		public SerializedProperty mSerializedProperty;

		public VariableLogger mLogger;

		public void OnEnable() {
			// エディター上での実行じなどに破棄されてしまうので再取得
			if(mSelectedIndexOfComponent != -1 && mSelectedIndexOfProperty != -1) {
				mSerializedComponent = new SerializedObject(mHasComponents[mSelectedIndexOfComponent]);
				GetProperties(mSerializedComponent, out mHasPropertyPaths, out mHasPropertyNames);
				mSerializedProperty = mSerializedComponent.FindProperty(mHasPropertyPaths[mSelectedIndexOfProperty]);
			}
		}

		public void OnInspectorUpdate() {
			if(mSerializedProperty == null || mSelectedIndexOfProperty == -1) {
				return;
			}

			if(mLogger == null) {
				mLogger = new VariableLogger(mSerializedProperty, 100);
			} else {
				UpdateSerialized();
				mLogger.SetLoggingProperty(mSerializedProperty);
			}
			mLogger.LogData();
		}

		public void OnGUI(float width) {
			EditorGUILayout.BeginHorizontal();

			// GameObject
			EditorGUI.BeginChangeCheck();
			mGameObject = (GameObject)EditorGUILayout.ObjectField(mGameObject, typeof(GameObject), true);
			if(EditorGUI.EndChangeCheck()) {
				GetComponents(mGameObject, out mHasComponents, out mHasComponentNames);
				mSelectedIndexOfComponent = -1;
			}
			if(mGameObject == null) {
				return;
			}

			// Components
			EditorGUI.BeginChangeCheck();
			mSelectedIndexOfComponent = EditorGUILayout.Popup(mSelectedIndexOfComponent, mHasComponentNames);
			if(EditorGUI.EndChangeCheck()) {
				// mHasComponentsがnullの場合は取得するような処理を挟んだ方がいいのかな？
				mSerializedComponent = new SerializedObject(mHasComponents[mSelectedIndexOfComponent]);
				GetProperties(mSerializedComponent, out mHasPropertyPaths, out mHasPropertyNames);
				mSelectedIndexOfProperty = -1;
			}
			if(mSelectedIndexOfComponent == -1) {
				return;
			}

			// Properties
			EditorGUI.BeginChangeCheck();
			mSelectedIndexOfProperty = EditorGUILayout.Popup(mSelectedIndexOfProperty, mHasPropertyNames);
			if(EditorGUI.EndChangeCheck()) {
				// mSerializedComponentがnullの場合は取得するような処理を挟んだ方がいいのかな？
				mSerializedProperty = mSerializedComponent.FindProperty(mHasPropertyPaths[mSelectedIndexOfProperty]);
				mLogger = new VariableLogger(mSerializedProperty, 100);
			}

			if(mSelectedIndexOfProperty == -1) {
				return;
			}

			EditorGUILayout.EndHorizontal();

			if(mLogger == null) {
				return;
			}
			var lastRect = GUILayoutUtility.GetLastRect();
			var graphRect = new Rect(lastRect.x + 50, lastRect.y + 20, width - 58, 40);
			mLogger.DrawGraph(graphRect);
		}

		void UpdateSerialized() {
			if(mSerializedProperty == null) {
				return;
			}
			// 自作のコンポーネントのみ実行じに参照が外れるらしい。
			if(mHasComponents[mSelectedIndexOfComponent] == null) {
				mHasComponents = mGameObject.GetComponents<Component>();
			}
			mSerializedComponent = new SerializedObject(mHasComponents[mSelectedIndexOfComponent]);
			mSerializedProperty = mSerializedComponent.FindProperty(mHasPropertyPaths[mSelectedIndexOfProperty]);
		}

		void TestLog() {
			Debug.Log("mGameObject: " + mGameObject);
			Debug.Log("mHasComponents: " + mHasComponents);
			Debug.Log("mSerializedComponent: " + mSerializedComponent);
			Debug.Log("mSelectedIndexOfComponent: " + mSelectedIndexOfComponent);
			Debug.Log("mSerializedProperty: " + mSerializedProperty);
			Debug.Log("mSelectedIndexOfProperty: " + mSelectedIndexOfProperty);
		}

		static void GetComponents(GameObject gameObject, out Component[] components, out string[] componentNames) {
			if(gameObject == null) {
				components = null;
				componentNames = null;
				return;
			} else {
				components = gameObject.GetComponents<Component>();
				componentNames = new string[components.Length];
				for(var i = 0; i < componentNames.Length; ++i) {
					componentNames[i] = components[i].GetType().Name;
				}
			}
		}

		static void GetProperties(SerializedObject serializedObject, out string[] propertyPaths, out string[] propertyNames) {
			if(serializedObject == null) {
				propertyPaths = null;
				propertyNames = null;
			} else {
				var iter = serializedObject.GetIterator();
				var pathList = new List<string>();
				var nameList = new List<string>();

				if(iter.Next(true)) {
					do {
						var property = serializedObject.FindProperty(iter.propertyPath);
						var name = GetDisplayName(property);
						if(string.IsNullOrEmpty(name)) {
							continue;
						}
						pathList.Add(iter.propertyPath);
						nameList.Add(name);
					} while(iter.Next(false));
				}

				propertyPaths = pathList.ToArray();
				propertyNames = nameList.ToArray();
			}
		}

		static string GetDisplayName(SerializedProperty sp) {
			var str = ToSimpleName(sp.propertyType);
			if(string.IsNullOrEmpty(str)) {
				return null;
			}

			var builder = new StringBuilder();
			builder.Append(str);
			builder.Append("/");
			builder.Append(sp.name);

			return builder.ToString();
		}

		static string ToSimpleName(SerializedPropertyType type) {
			switch(type) {
				case SerializedPropertyType.Integer: return "int";
				case SerializedPropertyType.Float: return "float";
				case SerializedPropertyType.Vector3: return "Vector3";
				case SerializedPropertyType.Vector2: return "Vector2";
				case SerializedPropertyType.Quaternion: return "Quaternion";
				default: return null;
			}
		}
	}

	public class VariableVisualizer : EditorWindow {

		public PropertyLogger[] mLoggers;
		public Vector2 mScrollPos;

		[MenuItem("Window/Seiro/VariableVisualizer")]
		static void Open() {
			var window = GetWindow<VariableVisualizer>();
			window.titleContent = new GUIContent("VariableVisualizer");
		}

		void OnEnable() {
			if(mLoggers == null) {
				mLoggers = new PropertyLogger[1];
			}

			for(var i = 0; i < mLoggers.Length; ++i) {
				mLoggers[i].OnEnable();
			}
		}

		void OnInspectorUpdate() {
			if(Application.isPlaying) {
				for(var i = 0; i < mLoggers.Length; ++i) {
					mLoggers[i].OnInspectorUpdate();
				}
				Repaint();
			}
		}

		void OnGUI() {
			// 配列を頑張って描画するための何かしら。
			// 以下参考
			// https://forum.unity.com/threads/display-array-in-custom-editor-window.380871/

			mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);

			var serializedThis = new SerializedObject(this);
			var serializedArray = serializedThis.FindProperty("mLoggers");
			serializedThis.Update();
			EditorGUILayout.PropertyField(serializedArray, true);
			serializedThis.ApplyModifiedProperties();

			for(var i = 0; i < mLoggers.Length; ++i) {
				mLoggers[i].OnGUI(EditorGUIUtility.currentViewWidth);

				// ここ他にいい書き方ないです？
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();

			EditorGUILayout.EndScrollView();
		}
	}
}