using UniversalVR.Adapters; using UniversalVR.Models;
namespace UniversalVR.LongArms;
public sealed class LongArmManager {
 readonly GameAdapterManager _adapters; public ArmProfile Profile{get;}=new();
 public float MinLength=>.5f; public float MaxLength=>3f;
 public LongArmManager(GameAdapterManager a)=>_adapters=a;
 public void SetBoth(float v){v=Math.Clamp(v,MinLength,MaxLength);Profile.LeftLength=Profile.RightLength=v;Apply();}
 public void SetLeft(float v){Profile.LeftLength=Math.Clamp(v,MinLength,MaxLength);Apply();}
 public void SetRight(float v){Profile.RightLength=Math.Clamp(v,MinLength,MaxLength);Apply();}
 public void SetOffsets(Vector3Value l,Vector3Value r){Profile.LeftOffset=l;Profile.RightOffset=r;Apply();}
 public void Reset(){Profile.Reset();Apply();}
 public bool Apply()=>_adapters.ActiveAdapter?.SetArmProfile(Profile)==true;
}
