// Generated automatically by MacroCodeGenerator (from "Packages/com.yousing.holograms/Editor/Private/Private.ms")

#if !YOUSING_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Build;
using UnityEngine;using YouSingStudio.Holograms;

namespace YouSingStudio.Private.Editor {
	/// <summary>
	/// <seealso cref="AssemblyDefinitionImporterInspector"/><br/>
	/// <seealso cref="AssemblyDefinitionReferenceImporterInspector"/>
	/// </summary>
	[ScriptedImporter(0,".asmlnk")]
	public class AssemblyLinkerImporter
		:ScriptedImporter
	{
		#region Nested Types

		[System.Serializable]
		public class Assembly {
			public string name;
			public string type;
			public string define;
			public string file;
			public string text;
		}

		public class Context {
			public List<Assembly> assemblies;
			public List<string> defines;
			public List<string> types;
		}

		#endregion Nested Types

		#region Fields

		[System.NonSerialized]public Context context=new Context();
		[System.NonSerialized]public bool dirty;

		#endregion Fields

		#region Methods

		protected virtual void Link(Assembly value) {
			if(value!=null) {
				//
				System.Type type=null;bool empty=string.IsNullOrEmpty(value.name);
				if(!empty) {context.types.Add($"{{0}},{value.name}");}
				for(int i=0,imax=context.types?.Count??0;i<imax;++i) {
					type=System.Type.GetType(string.Format(context.types[i],value.type));
					if(type!=null) {break;}
				}
				//
				if(!string.IsNullOrEmpty(value.define)) {
					if(type==null) {
						context.defines.Remove(value.define);
					}else if(context.defines.IndexOf(value.define)<0) {
						context.defines.Add(value.define);
					}
				}
				//
				if(type!=null&&!empty
					&&type.Assembly.FullName.StartsWith("Assembly-CSharp")
				) {
					string fn=value.file,tn,it;;
					if(string.IsNullOrEmpty(fn)||!File.Exists(fn)) {
						if(!(fn?.EndsWith(".cs")??false)) {tn=type.Name;}
						else {tn=Path.GetFileNameWithoutExtension(fn);}
						//
						var tmp=AssetDatabase.FindAssets(tn+" t:MonoScript");
						for(int i=0,imax=tmp?.Length??0;i<imax;++i) {
							it=AssetDatabase.GUIDToAssetPath(tmp[i]);
							if(Path.GetFileNameWithoutExtension(it)==tn) {
								//
								var cmd=fn?.Split('/');
								for(int j=0,jmax=cmd?.Length??0;j<jmax;++j) {
									if(cmd[j]=="..") {it=Path.GetDirectoryName(it);}
									else {it=Path.Combine(it,cmd[j]);}
								}
								//
								fn=it;break;
							}
						}
					}
					if(!string.IsNullOrEmpty(fn)) {
						dirty=true;
						File.WriteAllText(
							Path.Combine(Path.GetDirectoryName(fn),value.name)
							+".asmdef",$"{{\r\n\t\"name\":\"{value.name}\"{value.text}\r\n}}"
						);
					}else {
						Debug.LogError($"Failed to create {value.name}.asmdef");
					}
				}
			}
		}

		public override void OnImportAsset(AssetImportContext ctx) {
			string text;
			NamedBuildTarget target=NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			//
			dirty=false;
			context.defines=new List<string>(PlayerSettings.GetScriptingDefineSymbols(target).Split(';'));
				JsonUtility.FromJsonOverwrite(text=File.ReadAllText(ctx.assetPath),context);
				if((context.types?.Count??0)<=0) {
					context.types=new List<string>{
						"{0}",
						"{0},Assembly-CSharp",
						"{0},Assembly-CSharp-Editor",
						"{0},Assembly-CSharp-firstpass",
						"{0},Assembly-CSharp-Editor-firstpass"
					};
				}
				context.assemblies?.ForEach(Link);
			PlayerSettings.SetScriptingDefineSymbols(target,string.Join(';',context.defines));
			if(dirty) {AssetDatabase.Refresh();}
			//
			TextAsset asset=new TextAsset(text);ctx.AddObjectToAsset("main obj",asset);ctx.SetMainObject(asset);
		}

		#endregion Methods
	}
}
#endif
