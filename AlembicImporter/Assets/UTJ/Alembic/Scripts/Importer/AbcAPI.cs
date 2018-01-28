using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTJ.Alembic
{
    public enum aiAspectRatioMode
    {
        CurrentResolution = 0,
        DefaultResolution = 1,
        CameraAperture
    };

    public enum aiNormalsMode
    {
        ReadFromFile = 0,
        ComputeIfMissing,
        AlwaysCompute,
        Ignore
    }

    public enum aiTangentsMode
    {
        None = 0,
        Compute,
    }

    public enum aiTopologyVariance
    {
        Constant,
        Homogeneous, // vertices are variant, topology is constant
        Heterogeneous, // both vertices and topology are variant
    }

    public enum aiPropertyType
    {
        Unknown,

        // scalar types
        Bool,
        Int,
        UInt,
        Float,
        Float2,
        Float3,
        Float4,
        Float4x4,

        // array types
        BoolArray,
        IntArray,
        UIntArray,
        FloatArray,
        Float2Array,
        Float3Array,
        Float4Array,
        Float4x4Array,

        ScalarTypeBegin = Bool,
        ScalarTypeEnd = Float4x4,

        ArrayTypeBegin = BoolArray,
        ArrayTypeEnd = Float4x4Array,
    };

    public struct aiConfig
    {
        public aiNormalsMode normalsMode;
        public aiTangentsMode tangentsMode;
        public float aspectRatio;
        public Bool swapHandedness;
        public Bool swapFaceWinding;
        public Bool interpolateSamples;
        public Bool turnQuadEdges;
        public float vertexMotionScale;
        public int splitUnit;

        public void SetDefaults()
        {
            swapHandedness = true;
            swapFaceWinding = false;
            normalsMode = aiNormalsMode.ComputeIfMissing;
            tangentsMode = aiTangentsMode.None;
            aspectRatio = -1.0f;
            interpolateSamples = true;
            turnQuadEdges = false;
            vertexMotionScale = 1.0f;
            splitUnit = 65000;
        }
    }

    public struct aiSampleSelector
    {
        public ulong requestedIndex;
        public double requestedTime;
        public int requestedTimeIndexType;
    }

    public struct aiMeshSummary
    {
        public aiTopologyVariance topologyVariance;
        public Bool hasVelocities;
        public Bool hasNormals;
        public Bool hasTangents;
        public Bool hasUV0;
        public Bool hasUV1;
        public Bool hasColors;
        public Bool constantPoints;
        public Bool constantVelocities;
        public Bool constantNormals;
        public Bool constantTangents;
        public Bool constantUV0;
        public Bool constantUV1;
        public Bool constantColors;
    }

    public struct aiMeshSampleSummary
    {
        public int splitCount;
        public int submeshCount;
        public int vertexCount;
        public int indexCount;
        public Bool topologyChanged;
    }

    public struct aiMeshSplitSummary
    {
        public int submeshCount;
        public int submeshOffset;
        public int vertexCount;
        public int vertexOffset;
        public int indexCount;
        public int indexOffset;
    }

    public struct aiSubmeshSummary
    {
        public int splitIndex;
        public int submeshIndex;
        public int indexCount;
    }

    public struct aiPolyMeshData
    {
        public IntPtr positions;
        public IntPtr velocities;
        public IntPtr normals;
        public IntPtr tangents;
        public IntPtr uv0;
        public IntPtr uv1;
        public IntPtr colors;
        public IntPtr indices;

        public int vertexCount;
        public int indexCount;

        public Vector3 center;
        public Vector3 size;
    }

    public struct aiSubmeshData
    {
        public IntPtr indices;
    }

    public struct aiXFormData
    {
        public Vector3 translation;
        public Quaternion rotation;
        public Vector3 scale;
        public Bool inherits;
    }

    public struct aiCameraData
    {
        public float nearClippingPlane;
        public float farClippingPlane;
        public float fieldOfView;   // in degree. vertical one
        public float aspectRatio;

        public float focusDistance; // in cm
        public float focalLength;   // in mm
        public float aperture;      // in cm. vertical one
    }

    public struct aiPointsSummary
    {
        public Bool hasVelocity;
        public Bool positionIsConstant;
        public Bool idIsConstant;
        public int peakCount;
        public ulong minID;
        public ulong maxID;
        public Vector3 boundsCenter;
        public Vector3 boundsExtents;
    };

    public struct aiPointsData
    {
        public IntPtr positions;
        public IntPtr velocities;
        public IntPtr ids;
        public int count;

        public Vector3 boundsCenter;
        public Vector3 boundsExtents;
    }

    public struct aiPropertyData
    {
        public IntPtr data;
        public int size;
        aiPropertyType type;
    }


    public struct aiContext
    {
        public IntPtr self;
        public static implicit operator bool(aiContext v) { return v.self != IntPtr.Zero; }

        public static aiContext Create(int uid) { return aiCreateContext(uid); }
        public static void DestroyByPath(string path) { aiClearContextsWithPath(path); }

        public void Destroy() { aiDestroyContext(self); self = IntPtr.Zero; }
        public bool Load(string path) { return aiLoad(self, path); }
        public void SetConfig(ref aiConfig conf) { aiSetConfig(self, ref conf); }
        public void UpdateSamples(float time) { aiUpdateSamples(self, time); }

        public aiObject topObject { get { return aiGetTopObject(self); } }
        public float startTime { get { return aiGetStartTime(self); } }
        public float endTime { get { return aiGetEndTime(self); } }
        public int frameCount { get { return aiGetFrameCount(self); } }

        #region internal
        [DllImport("abci")] public static extern void aiClearContextsWithPath(string path);
        [DllImport("abci")] public static extern aiContext aiCreateContext(int uid);
        [DllImport("abci")] public static extern void aiDestroyContext(IntPtr ctx);
        [DllImport("abci")] static extern Bool aiLoad(IntPtr ctx, string path);
        [DllImport("abci")] static extern void aiSetConfig(IntPtr ctx, ref aiConfig conf);
        [DllImport("abci")] static extern float aiGetStartTime(IntPtr ctx);
        [DllImport("abci")] static extern float aiGetEndTime(IntPtr ctx);
        [DllImport("abci")] static extern int aiGetFrameCount(IntPtr ctx);
        [DllImport("abci")] static extern aiObject aiGetTopObject(IntPtr ctx);
        [DllImport("abci")] static extern void aiUpdateSamples(IntPtr ctx, float time);
        #endregion
    }

    public struct aiObject
    {
        public IntPtr self;
        public static implicit operator bool(aiObject v) { return v.self != IntPtr.Zero; }

        public string name { get { return Marshal.PtrToStringAnsi(aiGetNameS(self)); } }
        public bool enabled { set { aiSetEnabled(self, value); } }
        public int childCount { get { return aiGetNumChildren(self); } }
        public aiObject GetChild(int i) { return aiGetChild(self, i); }

        public aiSchema AsXform() { return aiGetXForm(self); }
        public aiSchema AsCamera() { return aiGetCamera(self); }
        public aiSchema AsPoints() { return aiGetPoints(self); }
        public aiSchema AsPolyMesh() { return aiGetPolyMesh(self); }

        public void EachChild(Action<aiObject> act)
        {
            int n = childCount;
            for (int ci = 0; ci < n; ++ci)
                act.Invoke(GetChild(ci));
        }

        #region internal
        [DllImport("abci")] static extern int aiGetNumChildren(IntPtr obj);
        [DllImport("abci")] static extern aiObject aiGetChild(IntPtr obj, int i);
        [DllImport("abci")] static extern void aiSetEnabled(IntPtr obj, Bool v);
        [DllImport("abci")] static extern IntPtr aiGetNameS(IntPtr obj);

        [DllImport("abci")] static extern aiSchema aiGetXForm(IntPtr obj);
        [DllImport("abci")] static extern aiSchema aiGetCamera(IntPtr obj);
        [DllImport("abci")] static extern aiSchema aiGetPoints(IntPtr obj);
        [DllImport("abci")] static extern aiSchema aiGetPolyMesh(IntPtr obj);
        #endregion
    }

    public struct aiSchema
    {
        public IntPtr self;
        public static implicit operator bool(aiSchema v) { return v.self != IntPtr.Zero; }
        public static explicit operator aiXform(aiSchema v) { var tmp = default(aiXform); tmp.self = v.self; return tmp; }
        public static explicit operator aiCamera(aiSchema v) { var tmp = default(aiCamera); tmp.self = v.self; return tmp; }
        public static explicit operator aiPolyMesh(aiSchema v) { var tmp = default(aiPolyMesh); tmp.self = v.self; return tmp; }
        public static explicit operator aiPoints(aiSchema v) { var tmp = default(aiPoints); tmp.self = v.self; return tmp; }

        public bool isConstant { get { return aiSchemaIsConstant(self); } }
        public bool dirty { get { return aiSchemaIsDirty(self); } }
        public aiSample sample { get { return aiSchemaGetSample(self); } }


        public aiSample UpdateSample(ref aiSampleSelector ss) { return aiSchemaUpdateSample(self, ref ss); }
        public void MarkForceUpdate() { aiSchemaMarkForceUpdate(self); }


        #region internal
        [DllImport("abci")] static extern aiSample aiSchemaUpdateSample(IntPtr schema, ref aiSampleSelector ss);

        [DllImport("abci")] static extern Bool aiSchemaIsConstant(IntPtr schema);
        [DllImport("abci")] static extern Bool aiSchemaIsDirty(IntPtr schema);
        [DllImport("abci")] static extern aiSample aiSchemaGetSample(IntPtr schema);
        [DllImport("abci")] static extern void aiSchemaMarkForceUpdate(IntPtr schema);

        [DllImport("abci")] static extern int aiSchemaGetNumProperties(IntPtr schema);
        [DllImport("abci")] static extern aiProperty aiSchemaGetPropertyByIndex(IntPtr schema, int i);
        [DllImport("abci")] static extern aiProperty aiSchemaGetPropertyByName(IntPtr schema, string name);
        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct aiXform
    {
        [FieldOffset(0)] public IntPtr self;
        [FieldOffset(0)] public aiSchema schema;
        public static implicit operator bool(aiXform v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSchema(aiXform v) { aiSchema tmp; tmp.self = v.self; return tmp; }

        public aiXformSample sample { get { return aiSchemaGetSample(self); } }

        #region internal
        [DllImport("abci")] static extern aiXformSample aiSchemaGetSample(IntPtr schema);
        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct aiCamera
    {
        [FieldOffset(0)] public IntPtr self;
        [FieldOffset(0)] public aiSchema schema;
        public static implicit operator bool(aiCamera v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSchema(aiCamera v) { aiSchema tmp; tmp.self = v.self; return tmp; }

        public aiCameraSample sample { get { return aiSchemaGetSample(self); } }

        #region internal
        [DllImport("abci")] static extern aiCameraSample aiSchemaGetSample(IntPtr schema);
        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct aiPolyMesh
    {
        [FieldOffset(0)] public IntPtr self;
        [FieldOffset(0)] public aiSchema schema;
        public static implicit operator bool(aiPolyMesh v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSchema(aiPolyMesh v) { aiSchema tmp; tmp.self = v.self; return tmp; }

        public aiPolyMeshSample sample { get { return aiSchemaGetSample(self); } }
        public void GetSummary(ref aiMeshSummary dst) { aiPolyMeshGetSummary(self, ref dst); }

        #region internal
        [DllImport("abci")] static extern void aiPolyMeshGetSummary(IntPtr schema, ref aiMeshSummary dst);
        [DllImport("abci")] static extern aiPolyMeshSample aiSchemaGetSample(IntPtr schema);
        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct aiPoints
    {
        [FieldOffset(0)] public IntPtr self;
        [FieldOffset(0)] public aiSchema schema;
        public static implicit operator bool(aiPoints v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSchema(aiPoints v) { aiSchema tmp; tmp.self = v.self; return tmp; }

        public aiPointsSample sample { get { return aiSchemaGetSample(self); } }
        public bool sort { set { aiPointsSetSort(self, value); } }
        public Vector3 sortBasePosition { set { aiPointsSetSortBasePosition(self, value); } }

        public void GetSummary(ref aiPointsSummary dst) { aiPointsGetSummary(self, ref dst); }

        #region internal
        [DllImport("abci")] static extern aiPointsSample aiSchemaGetSample(IntPtr schema);
        [DllImport("abci")] static extern void aiPointsSetSort(IntPtr schema, Bool v);
        [DllImport("abci")] static extern void aiPointsSetSortBasePosition(IntPtr schema, Vector3 v);
        [DllImport("abci")] static extern void aiPointsGetSummary(IntPtr schema, ref aiPointsSummary dst);
        #endregion
    }


    public struct aiSample
    {
        public IntPtr self;
        public static implicit operator bool(aiSample v) { return v.self != IntPtr.Zero; }
        public static explicit operator aiXformSample(aiSample v) { aiXformSample tmp; tmp.self = v.self; return tmp; }
        public static explicit operator aiCameraSample(aiSample v) { aiCameraSample tmp; tmp.self = v.self; return tmp; }
        public static explicit operator aiPolyMeshSample(aiSample v) { aiPolyMeshSample tmp; tmp.self = v.self; return tmp; }
        public static explicit operator aiPointsSample(aiSample v) { aiPointsSample tmp; tmp.self = v.self; return tmp; }
    }

    public struct aiXformSample
    {
        public IntPtr self;
        public static implicit operator bool(aiXformSample v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSample(aiXformSample v) { aiSample tmp; tmp.self = v.self; return tmp; }

        public void GetData(ref aiXFormData dst) { aiXFormGetData(self, ref dst); }

        #region internal
        [DllImport("abci")] public static extern void aiXFormGetData(IntPtr sample, ref aiXFormData data);
        #endregion
    }

    public struct aiCameraSample
    {
        public IntPtr self;
        public static implicit operator bool(aiCameraSample v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSample(aiCameraSample v) { aiSample tmp; tmp.self = v.self; return tmp; }

        public void GetData(ref aiCameraData dst) { aiCameraGetData(self, ref dst); }

        #region internal
        [DllImport("abci")] public static extern void aiCameraGetData(IntPtr sample, ref aiCameraData dst);
        #endregion
    }

    public struct aiPolyMeshSample
    {
        public IntPtr self;
        public static implicit operator bool(aiPolyMeshSample v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSample(aiPolyMeshSample v) { aiSample tmp; tmp.self = v.self; return tmp; }

        public void GetSummary(ref aiMeshSampleSummary dst) { aiPolyMeshGetSampleSummary(self, ref dst); }
        public void GetSplitSummary(int splitIndex, ref aiMeshSplitSummary dst) { aiPolyMeshGetSplitSummary(self, splitIndex, ref dst); }
        public void GetSubmeshSummary(int splitIndex, int submeshIndex, ref aiSubmeshSummary dst) { aiPolyMeshGetSubmeshSummary(self, splitIndex, submeshIndex, ref dst); }
        public void FillVertexBuffer(int splitIndex, ref aiPolyMeshData dst) { aiPolyMeshFillVertexBuffer(self, splitIndex, ref dst); }
        public void FillSubmeshIndices(int splitIndex, int submeshIndex, ref aiSubmeshData dst) { aiPolyMeshFillSubmeshIndices(self, splitIndex, submeshIndex, ref dst); }

        #region internal
        [DllImport("abci")] static extern void aiPolyMeshGetSampleSummary(IntPtr sample, ref aiMeshSampleSummary dst);
        [DllImport("abci")] static extern int aiPolyMeshGetSplitSummary(IntPtr sample, int splitIndex, ref aiMeshSplitSummary dst);
        [DllImport("abci")] static extern void aiPolyMeshGetSubmeshSummary(IntPtr sample, int splitIndex, int submeshIndex, ref aiSubmeshSummary dst);
        [DllImport("abci")] static extern void aiPolyMeshFillVertexBuffer(IntPtr sample, int splitIndex, ref aiPolyMeshData dst);
        [DllImport("abci")] static extern void aiPolyMeshFillSubmeshIndices(IntPtr sample, int splitIndex, int submeshIndex, ref aiSubmeshData dst);
        #endregion
    }

    public struct aiPointsSample
    {
        public IntPtr self;
        public static implicit operator bool(aiPointsSample v) { return v.self != IntPtr.Zero; }
        public static implicit operator aiSample(aiPointsSample v) { aiSample tmp; tmp.self = v.self; return tmp; }

        public void CopyData(ref aiPointsData dst) { aiPointsCopyData(self, ref dst); }

        #region internal
        [DllImport("abci")] static extern void aiPointsCopyData(IntPtr sample, ref aiPointsData dst);
        #endregion
    }


    public struct aiProperty
    {
        public IntPtr self;
        public static implicit operator bool(aiProperty v) { return v.self != IntPtr.Zero; }

        #region internal
        [DllImport("abci")] static extern IntPtr aiPropertyGetNameS(IntPtr prop);
        [DllImport("abci")] static extern aiPropertyType aiPropertyGetType(IntPtr prop);
        [DllImport("abci")] static extern void aiPropertyGetData(IntPtr prop, aiPropertyData oData);
        #endregion
    }

    public partial class AbcAPI
    {
        [DllImport("abci")] public static extern            aiSampleSelector aiTimeToSampleSelector(float time);
        [DllImport("abci")] public static extern void       aiCleanup();

    }

    public class AbcUtils
    {
    #if UNITY_EDITOR
        
        static MethodInfo s_GetBuiltinExtraResourcesMethod;

        public static Material GetDefaultMaterial()
        {
            if (s_GetBuiltinExtraResourcesMethod == null)
            {
                BindingFlags bfs = BindingFlags.NonPublic | BindingFlags.Static;
                s_GetBuiltinExtraResourcesMethod = typeof(EditorGUIUtility).GetMethod("GetBuiltinExtraResource", bfs);
            }
            return (Material)s_GetBuiltinExtraResourcesMethod.Invoke(null, new object[] { typeof(Material), "Default-Material.mat" });
        }

    #endif
    }
}
