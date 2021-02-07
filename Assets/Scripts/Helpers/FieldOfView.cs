using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FieldOfView : MonoBehaviour
{
    [HideInInspector]
    public float viewRadius;
    [HideInInspector]
    public float viewAngle;

    public float meshResolution = 0.5f;
    public int edgeResolveIteration = 10;
    public float edgeDistanceThreashold = 0.5f;

    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    void Start(){
        viewMesh = new Mesh();
        viewMesh.name = "ViewMesh";
        viewMeshFilter.mesh = viewMesh;
    }

    void LateUpdate(){
        DrawFieldOfView();
    }

    void DrawFieldOfView(){
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i<=stepCount; i++){
            float angle = transform.eulerAngles.y - viewAngle /2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            
            if (i>0){
                bool edgeDistanceThreasholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDistanceThreashold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThreasholdExceeded)){
                    EdgeInfo edge = FindEdge(oldViewCast,newViewCast);
                    if (edge.pointA != Vector3.zero){
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero){
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2)*3];

        vertices[0] = Vector3.zero;
        for (int i = 0;i<vertexCount-1;i++){
            vertices[i+1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount -2){
                triangles[i*3] = 0;
                triangles[i*3 + 1] = i+1;
                triangles[i*3 + 2] = i+2;
            }
            
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast){
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0;i<edgeResolveIteration;i++){
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDistanceThreasholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDistanceThreashold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThreasholdExceeded){
                minAngle = angle;
                minPoint = newViewCast.point;
            }else{
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint,maxPoint);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool global){
        if (!global){
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees*Mathf.Deg2Rad));
    }

    private ViewCastInfo ViewCast(float globalAngle){
        Vector3 dir = DirFromAngle(globalAngle,true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position,dir,out hit,viewRadius,ArenaHelper.Instance.ObstaclesLayerMask | ArenaHelper.Instance.AgentsLayerMask)){
            return new ViewCastInfo(true,hit.point,hit.distance,globalAngle);
        }else{
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius,globalAngle);
        }
    }

    public struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool h, Vector3 p, float d, float a){
            hit = h;
            point = p;
            dst = d;
            angle = a;
        }
    }

    public struct EdgeInfo{
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 A, Vector3 B){
            pointA = A;
            pointB = B;
        }
    }
}
