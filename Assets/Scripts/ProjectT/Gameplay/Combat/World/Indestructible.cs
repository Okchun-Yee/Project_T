using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectT.Gameplay.Combat.World
{
    /// <summary>
    /// 파괴 불가능 오브젝트 표시 컴포넌트 (Marker)
    /// </summary>
    public class Indestructible : MonoBehaviour
    {
        public bool IsIndestructible => true;
    }
}
