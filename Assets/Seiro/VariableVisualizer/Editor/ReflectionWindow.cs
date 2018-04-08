using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System;
using System.Reflection;
using System.IO;

namespace Seiro.VariableVisualizer {

	public class ReflectionWindow : EditorWindow {

		string mTypeName;
		string mResult;
		Vector2 mScrollPos;

		[MenuItem("Window/Seiro/ReflectionWindow")]
		static void Open() {
			var window = GetWindow<ReflectionWindow>();
			window.titleContent = new GUIContent("Reflection Window");
		}

		void OnGUI() {
			EditorGUI.BeginChangeCheck();
			mTypeName = EditorGUILayout.TextField(mTypeName);
			if(!EditorGUI.EndChangeCheck()) {
				mResult = Example(mTypeName);
			}

			if(string.IsNullOrEmpty(mTypeName)) {
				return;
			}

			mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
			EditorGUILayout.TextArea(mResult);
			EditorGUILayout.EndScrollView();
		}

		static string Example(string typeName) {
			var builder = new StringBuilder();
			var type = GetType(typeName);

			if(type == null) {
				return string.Empty;
			}

			foreach(var n in type.GetMethods()) {
				builder.AppendLine(ToText(n));
			}

			var bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
			foreach(var n in type.GetMethods(bindingAttr)) {
				builder.AppendLine(ToText(n));
			}
			return builder.ToString();
		}

		static Type GetType(string typeName) {
			if(string.IsNullOrEmpty(typeName)) {
				return null;
			}

			var type = Type.GetType(typeName);

			if(type != null) {
				return type;
			}

			if(typeName.Contains(".")) {
				var assemblyString = typeName.Substring(0, typeName.IndexOf('.'));

				Assembly assembly;

				try {
					assembly = Assembly.Load(assemblyString);
				} catch(FileNotFoundException) {
					return null;
				}

				if(assembly == null) {
					return null;
				}

				type = assembly.GetType(typeName);
				if(type != null) {
					return type;
				}
			}

			var executingAssembly = Assembly.GetExecutingAssembly();
			var referencedAssemblies = executingAssembly.GetReferencedAssemblies();

			foreach(var assemblyName in referencedAssemblies) {
				var assembly = Assembly.Load(assemblyName);
				if(assembly == null) {
					continue;
				}
				type = assembly.GetType(typeName);
				if(type != null) {
					return type;
				}
			}
			return null;
		}

		static string ToText(MethodInfo methodInfo) {
			var builder = new StringBuilder();

			if(methodInfo.IsPublic) {
				builder.Append("public ");
			} else if(methodInfo.IsPrivate) {
				builder.Append("private ");
			}

			if(methodInfo.IsStatic) {
				builder.Append("static ");
			}

			builder.Append(ToSimpleName(methodInfo.ReturnType));
			builder.Append(" ");
			builder.Append(methodInfo.Name);
			builder.Append("(");

			var parameters = methodInfo.GetParameters();
			for(int i = 0; i < parameters.Length; ++i) {
				var p = parameters[i];
				var isLast = parameters.Length - 1 <= i;
				builder.Append(ToSimpleName(p.ParameterType));
				builder.Append(" ");
				builder.Append(p.Name);
				builder.Append(isLast ? "" : ", ");
			}

			builder.Append(")");
			return builder.ToString();
		}

		static string ToSimpleName(Type type) {
			var str = type.ToString();
			switch(str) {
				case "System.Void"		: return "void";
				case "System.Boolean"	: return "bool";
				case "System.Int32"		: return "int";
				case "System.Single"	: return "float";
				case "System.String"	: return "string";
			}

			return str;
		}
	}
}