/* <!-- Macro.Table Replaces
#if UNITY_EDITOR,#if true
using UnityEditor;,
EditorPrefs,PlayerPrefs
TextureScale,//
EditorUtility.DisplayDialog,//
EditorStyles.centeredGreyMiniLabel,GUI.skin.label
EditorStyles.label,GUI.skin.label
EditorStyles.miniLabel,GUI.skin.label
EditorStyles.wordWrappedMiniLabel,GUI.skin.label

using SimpleJSON;,using Newtonsoft.Json;using Newtonsoft.Json.Linq;
JSONNode,JObject
JSON.Parse,JObject.Parse
JSONArray,JArray
.AsArray,as JArray
.AsBool,.Value<bool>()
.AsInt,.Value<int>()

private string accessTokenKey,public static string accessTokenKey
List<SketchfabRequest> _,public List<SketchfabRequest> _
if (node != null && node[0]["tag_name"] != null),var jt=node?[0]?.SelectToken("tag_name")??null;if(jt!=null)
node[0]["tag_name"],jt.Value<string>()
if (response["access_token"] != null),var jt=response?.SelectToken("access_token")??null;if(jt!=null)
response["access_token"],jt.Value<string>()
elt["url"],elt["url"].Value<string>()
userData["username"]&#44; userData["displayName"]&#44; userData["account"],userData["username"].Value<string>()&#44;userData["displayName"].Value<string>()&#44;userData["account"].Value<string>()
 Macro.End --> */

// <!-- Macro.File $(FileName)/SketchfabApi.cs
/* <!-- Macro.Copy FILE
:Release/$(FileName)/Scripts/$(GeneratedFileName).cs
 Macro.End --> */
/* <!-- Macro.Replace Replaces
 Macro.End --> */
// Macro.End -->

// <!-- Macro.File $(FileName)/SketchfabPlugin.cs
/* <!-- Macro.Copy FILE
:Release/$(FileName)/Scripts/$(GeneratedFileName).cs
 Macro.End --> */
/* <!-- Macro.Replace Replaces
 Macro.End --> */
// Macro.End -->

// <!-- Macro.File $(FileName)/SketchfabLogger.cs
/* <!-- Macro.Copy FILE
:Release/$(FileName)/Scripts/$(GeneratedFileName).cs
 Macro.End --> */
/* <!-- Macro.Replace Replaces
 Macro.End --> */
// Macro.End -->

// <!-- Macro.File $(FileName)/SketchfabUI.cs
/* <!-- Macro.Copy FILE
:Release/$(FileName)/Scripts/$(GeneratedFileName).cs
 Macro.End --> */
/* <!-- Macro.Replace Replaces
 Macro.End --> */
// Macro.End -->