namespace UniversalVR.Models;
public sealed class ArmProfile {
 public float LeftLength {get;set;}=1f; public float RightLength {get;set;}=1f;
 public Vector3Value LeftOffset {get;set;}=new(); public Vector3Value RightOffset {get;set;}=new();
 public void Reset(){LeftLength=RightLength=1f;LeftOffset=new();RightOffset=new();}
}
public sealed class Vector3Value {
 public float X{get;set;} public float Y{get;set;} public float Z{get;set;}
 public Vector3Value(){} public Vector3Value(float x,float y,float z){X=x;Y=y;Z=z;}
}
