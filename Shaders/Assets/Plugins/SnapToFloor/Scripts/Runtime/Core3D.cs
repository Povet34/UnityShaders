﻿using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace NKStudio
{
    public partial class SnapToFloor
    {
        /// <summary>
        /// 선택한 객체를 땅에 스냅을 시킵니다.
        /// </summary>
        /// <param name="transform">스냅시킬 대상</param>
        private static void SnapToFloor3D(Transform transform)
        {
            int siblingIndex = transform.GetSiblingIndex();

            Transform selectObjectParent = transform.parent;

            //자식 오브젝트가 있는 자식의 매쉬 필터를 가져온다.
            SkinnedMeshRenderer[] skinnedMesh = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

            //자식 개수를 가져옵니다.
            int childCount = transform.childCount;

            //매쉬 필터를 가져온다.
            MeshFilter mfSelf = transform.GetComponent<MeshFilter>();

            //자식을 가지고 있는지 체크
            bool hasChild = childCount > 0;
            bool hasSkinnedMesh = skinnedMesh.Length > 0;
            bool hasMfSelf = mfSelf;
            Transform tr = transform;

            //매쉬 렌더러를 가지고 있고, 자식이 있을 경우
            if (hasMfSelf && childCount > 0)
            {
                GameObject go = new GameObject();
                go.transform.position = transform.position;
                transform.SetParent(go.transform);

                tr = go.transform;
            }

            //스키니드가 아니고, 자식이 있는 경우 컴바인한다.
            if (!hasSkinnedMesh && hasChild)
            {
                // 우리가 부모-자식 계층을 끊고 부모 계층을 다시 얻을 때 때로는 scale이 약간 달라지므로 끊기전에 scale을 저장한다.
                Vector3 oldScaleAsChild = tr.localScale;

                // 부모 오브젝트 계층안에 있으면 트랜스폼에 영향이 가므로, 부모 계층을 잠시 끊는다.
                int positionInParentHierarchy = tr.GetSiblingIndex();
                Transform parent = tr.parent;
                tr.parent = null;

                // 덕분에 새로 결합된 메시는 자식과 같은 세계 공간에서 동일한 위치와 크기를 갖게 됩니다.:
                Quaternion oldRotation = tr.rotation;
                Vector3 oldPosition = tr.position;
                Vector3 oldScale = tr.localScale;
                tr.rotation = Quaternion.identity;
                tr.position = Vector3.zero;
                tr.localScale = Vector3.one;

                //기존에 매쉬 필터가 없으면 해당 오브젝트에 추가하고,
                //이미 있으면 새로운 오브젝트를 만들어서 거기에 추가한다.
                mfSelf = tr.gameObject.AddComponent<MeshFilter>();

                //컴바인 시스템 동작
                CombineSystem(tr);

                // 변환 값을 다시 가져옵니다.
                tr.rotation = oldRotation;
                tr.position = oldPosition;
                tr.localScale = oldScale;

                // 상위 및 동일한 계층 위치를 다시 가져옵니다.:
                tr.parent = parent;
                tr.SetSiblingIndex(positionInParentHierarchy);

                //스케일 값을 자식으로 다시 설정:
                tr.localScale = oldScaleAsChild;
            }
            else
            {
                //스키니드가 아니고, 자식 오브젝트가 없는 경우
                if (!hasSkinnedMesh && !hasMfSelf)
                {
                    // 그냥 단순 Transform에 대한 지형 찾기
                    if (STFSettings.Instance.SnapToFloorType == ESnapToFloorType.NavMesh)
                    {
                        var sourcePosition = tr.position + Vector3.down * Height;
                        bool isHit = NavMesh.SamplePosition(sourcePosition, out var hit, Height, NavMesh.AllAreas);

                        if (isHit)
                        {
                            var pos = tr.position;
                            pos.y = hit.position.y;
                            tr.position = pos;
                            return;
                        }
                    }
                    else if (STFSettings.Instance.SnapToFloorType == ESnapToFloorType.Collider)
                    {
                        var ray = new Ray(tr.position, Vector3.down);
                        RaycastHit[] hits = Physics.RaycastAll(ray, Height);
                        foreach (var hit in hits)
                        {
                            //자기 자신의 콜라이더를 맞춘 경우 pass : 예외 처리
                            if (hit.collider.gameObject == tr.gameObject)
                                continue;
                            var pos = tr.position;
                            pos.y = hit.point.y;
                            tr.position = pos;
                            return;
                        }
                    }

                    if (Application.systemLanguage == SystemLanguage.Korean)
                        Assert.IsNotNull(mfSelf, "바닥이 감지 되지 않습니다.");
                    else
                        Assert.IsNotNull(mfSelf, "The floor is not detected.");
                }
            }

            #region 매쉬의 버텍스에 대한 월드 계산 위치

            Vector2 minMaxByX = GetMinMaxRangeByVertex3D(ResultType.X, tr, mfSelf);
            Vector3 vx1 = Vector3.zero;
            vx1.x = minMaxByX.x;

            Vector3 vx2 = Vector3.zero;
            vx2.x = minMaxByX.y;

            Vector2 minMaxByZ = GetMinMaxRangeByVertex3D(ResultType.Z, tr, mfSelf);
            Vector3 vz1 = Vector3.zero;
            vz1.x = minMaxByZ.x;

            Vector3 vz2 = Vector3.zero;
            vz2.x = minMaxByZ.y;

            float footYPosition = GetMinYVertex3D(tr);

            #endregion

            float distanceX = Vector3.Distance(vx1, vx2);
            float distanceZ = Vector3.Distance(vz1, vz2);

            float startPositionX = minMaxByX.x;
            float startPositionZ = minMaxByZ.x;

            //간격에 따른 알맞는 간격을 계산한다.
            float intervalValueX = CalculateSeparationByDistance(distanceX);
            float intervalValueZ = CalculateSeparationByDistance(distanceZ);

            //간격에 알맞는 알갱이를 가져옴
            int numberOfGrainsX = CalculatePointCount(distanceX, intervalValueX);
            int numberOfGrainsZ = CalculatePointCount(distanceZ, intervalValueZ);
            int nowNumberOfGrainsX = numberOfGrainsX + 1;
            int nowNumberOfGrainsZ = numberOfGrainsZ + 1;

            Vector3 position = tr.position;
            float? moveY = null;

            //원하는 알갱이 수 만큼 반복한다
            //- 원래는 내가 원하는 간격을 제시하면 그것에 맞는 알갱이를 뿌린다.
            for (int i = 0; i < nowNumberOfGrainsX; i++)
            {
                for (int j = 0; j < nowNumberOfGrainsZ; j++)
                {
                    //그려낼 위치에서 사이간격에 맞춰 그려냄
                    float xx = startPositionX + intervalValueX * i;
                    float zz = startPositionZ + intervalValueZ * j;

                    if (STFSettings.Instance.SnapToFloorType == ESnapToFloorType.NavMesh)
                    {
                        Vector3 drawPosition = new Vector3(xx, footYPosition, zz);
                        bool isHit = NavMesh.SamplePosition(drawPosition, out var hit, Height, NavMesh.AllAreas);

                        if (isHit)
                        {
                            if (moveY == null)
                                moveY = hit.distance;
                            else
                            {
                                if (moveY > hit.distance)
                                    moveY = hit.distance;
                            }
                        }
                    }
                    else if (STFSettings.Instance.SnapToFloorType == ESnapToFloorType.Collider)
                    {
                        Vector3 drawPosition = new Vector3(xx, footYPosition, zz);

                        //각각의 오브젝트의 위치에서 아래 방향으로 Ray를 쏜다.
                        RaycastHit[] hits = Physics.RaycastAll(drawPosition, Vector3.down, Height);

                        //각각 hit정보 확인
                        foreach (RaycastHit hit in hits)
                        {
                            //자기 자신의 콜라이더를 맞춘 경우 pass : 예외 처리
                            if (hit.collider.gameObject == tr.gameObject)
                                continue;

                            if (moveY == null)
                                moveY = hit.distance;
                            else
                            {
                                if (moveY > hit.distance)
                                    moveY = hit.distance;
                            }
                        }
                    }
                }
            }

            position.y -= moveY ?? 0f;
            //hit된 위치로 이동시킨다.
            tr.position = position;

            switch (hasMfSelf)
            {
                case true when childCount > 0:
                    transform.parent = selectObjectParent;
                    transform.SetSiblingIndex(siblingIndex);
                    DestroyImmediate(tr.gameObject);
                    break;
                case false:
                    DestroyImmediate(mfSelf);
                    break;
            }
        }
    }
}