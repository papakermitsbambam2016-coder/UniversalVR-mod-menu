using UniversalVR.Adapters; using UniversalVR.Adapters.BONELAB; using UniversalVR.Adapters.GorillaTag;
using UniversalVR.Adapters.ScaryBaboon; using UniversalVR.LongArms; using UniversalVR.Menu;
namespace UniversalVR;
public sealed class UniversalVRRuntime {
 public const string Version="0.1.0"; public GameAdapterManager Adapters{get;}=new();
 public MenuModel Menu{get;}=new(); public LongArmManager LongArms{get;}
 public UniversalVRRuntime(){LongArms=new LongArmManager(Adapters);}
 public void RegisterDefaultAdapters(){Adapters.Register(new BonelabAdapter());Adapters.Register(new GorillaTagAdapter());Adapters.Register(new ScaryBaboonAdapter());}
 public bool Initialize(){RegisterDefaultAdapters();return Adapters.TryInitialize();} public void Update()=>Adapters.Update(); public void Shutdown()=>Adapters.Shutdown();
}
