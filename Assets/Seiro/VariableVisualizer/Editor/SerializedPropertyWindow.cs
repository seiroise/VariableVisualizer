using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System;

namespace Seiro.VariableVisualizer {

	public class SerializedPropertyWindow : EditorWindow {

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

		[MenuItem("Window/Seiro/SerializedPropertyWindow")]
		static void Open() {
			var window = GetWindow<SerializedPropertyWindow>();
			window.titleContent = new GUIContent("SerializedPropertyWindow");
		}
/*
		void OnDisable() {
			// Debug.Log("OnDisable");
			// TestLog();
		}
*/
		void OnEnable() {
			// Debug.Log("OnEnable");

			// エディター上での実行じなどに破棄されてしまうので再取得
			if(mSelectedIndexOfComponent != -1 && mSelectedIndexOfProperty != -1) {
				mSerializedComponent = new SerializedObject(mHasComponents[mSelectedIndexOfComponent]);
				GetProperties(mSerializedComponent, out mHasPropertyPaths, out mHasPropertyNames);
				mSerializedProperty = mSerializedComponent.FindProperty(mHasPropertyPaths[mSelectedIndexOfProperty]);
			}

			// TestLog();
		}

		void OnInspectorUpdate() {
			// Debug.Log("OnInspectorUpdate");

			if(!Application.isPlaying) {
				return;
			}
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
			Repaint();
		}

		void OnGUI() {
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
			var graphRect = new Rect(lastRect.x + 50, lastRect.y + 20, position.width - 58, position.height - 24);
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
/*
		void InitializeIfNeeded() {
			if(mInitialized) {
				return;
			}
			Initialize();
		}

		void Initialize() {

			// Debug.Log("A");

			if(mGameObject == null) {
				return;
			}

			// Debug.Log("B");

			if(mSelectedComponentNamesIndex <= 0) {
				return;
			}
			GetComponents();
			mSerializedComponent = new SerializedObject(mHasComponents[mSelectedComponentNamesIndex]);

			// Debug.Log("C");

			if(mSelectedPropertyNamesIndex <= 0) {
				return;
			}
			mSerializedProperty = mSerializedComponent.FindProperty(mHasPropertyNames[mSelectedPropertyNamesIndex]);

			// Debug.Log(mSerializedProperty);
			mLogger = new PropertyLogger(mSerializedProperty, 100);

			mInitialized = true;
		}

		void Update() {
			if(!TryGetProperty(out mSerializedProperty)) {
				return;
			}
			if(mLogger == null) {
				mLogger = new PropertyLogger(mSerializedProperty, 100);
			} else {
				mLogger.UpdateProperty(mSerializedProperty);
			}
			mLogger.LogData();
			OnGUI();
		}

		void OnGUI() {

			InitializeIfNeeded();

			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
			mGameObject = EditorGUILayout.ObjectField(mGameObject, typeof(GameObject), true) as GameObject;
			if(EditorGUI.EndChangeCheck()) {
				if(mGameObject == null) {
					return;
				}
			}

			EditorGUI.BeginChangeCheck();
			mSelectedComponentNamesIndex = EditorGUILayout.Popup(mSelectedComponentNamesIndex, mHasComponentNames);
			if(EditorGUI.EndChangeCheck()) {
				if(mSelectedComponentNamesIndex <= 0) {
					return;
				}
				if(mHasComponents == null) {
					GetComponents();
				}

				var com = mHasComponents[mSelectedComponentNamesIndex];
				mSerializedComponent = new SerializedObject(com);
			}

			EditorGUI.BeginChangeCheck();
			mSelectedPropertyNamesIndex = EditorGUILayout.Popup(mSelectedPropertyNamesIndex, mHasPropertyDisplayNames);
			if(EditorGUI.EndChangeCheck()) {
				if(mSelectedPropertyNamesIndex <= 0) {
					return;
				}
				if(mHasPropertyDisplayNames == null) {
					GetPropertyDisplayNames(mSerializedComponent, out mHasPropertyNames, out mHasPropertyDisplayNames);
				}
				var propName = mHasPropertyNames[mSelectedPropertyNamesIndex];
				mSerializedProperty = mSerializedComponent.FindProperty(propName);
			}

			EditorGUILayout.EndHorizontal();

			var lastRect = GUILayoutUtility.GetLastRect();
			var graphRect = new Rect(lastRect.x + 50, lastRect.y + 20, position.width - 50, 200);
			mLogger.DrawGraph(graphRect);
		}
*/
/*
		bool TryGetProperty(out SerializedProperty property) {
			property = null;

			if(mGameObject == null) {
				return false;
			}

			if(mSelectedComponentNamesIndex <= 0) {
				return false;
			}
			GetComponents();
			mSerializedComponent = new SerializedObject(mHasComponents[mSelectedComponentNamesIndex]);

			if(mSelectedPropertyNamesIndex <= 0) {
				return false;
			}
			property = mSerializedComponent.FindProperty(mHasPropertyNames[mSelectedPropertyNamesIndex]);

			return true;
		}

		static string[] GetPropertyNames(SerializedObject so) {
			var iter = so.GetIterator();
			var list = new List<string>();

			if(iter.Next(true)) {
				do {
					list.Add(iter.propertyPath);
				} while(iter.Next(false));
			}
			return list.ToArray();
		}

		static void GetPropertyDisplayNames(SerializedObject so, out string[] names, out string[] displayNames) {
			var iter = so.GetIterator();
			var nameList = new List<string>();
			var displayNamelist = new List<string>();

			if(iter.Next(true)) {
				do {
					var sp = so.FindProperty(iter.propertyPath);
					var str = ToCategorizedName(sp);
					if(string.IsNullOrEmpty(str)) {
						continue;
					}
					nameList.Add(iter.propertyPath);
					displayNamelist.Add(str);
				} while(iter.Next(false));
			}

			names = nameList.ToArray();
			displayNames = displayNamelist.ToArray();
		}
*/
	}
}