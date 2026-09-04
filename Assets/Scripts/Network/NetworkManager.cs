// NetworkManager.cs — 멀티(2인 코옵) 골격 (문서 01-4, 06-5).
// Netcode for GameObjects 호스트-클라이언트. 패키지 미설치 환경에서도 컴파일되도록 인터페이스만 정의.
// 실제 구현(W12~14)에서 Unity.Netcode 참조 후 NetworkBehaviour로 교체.
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Network
{
    /// 동기화 채널 추상화. 이동=Unreliable, 판정=Reliable (문서 06-5).
    public interface INetChannel
    {
        void SendUnreliable(byte[] data);
        void SendReliable(byte[] data);
    }

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        public NetMode Mode { get; private set; } = NetMode.Single;
        public int RunSeed { get; private set; }
        public bool IsHost => Mode != NetMode.CoopClient;

        // ── 세션 (Unity Lobby 방 코드 — 문서 06-5) ──
        public void StartSingle()
        {
            Mode = NetMode.Single;
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
        }

        public void HostCoop()
        {
            Mode = NetMode.CoopHost;
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
            // TODO(W12): NetworkManager.Singleton.StartHost(); 시드 브로드캐스트
        }

        public void JoinCoop(string roomCode)
        {
            Mode = NetMode.CoopClient;
            // TODO(W12): Lobby 접속 → StartClient(); 호스트로부터 RunSeed 수신
        }

        /// 호스트 권한: 전투/드롭/제단 판정은 호스트만 확정 후 브로드캐스트.
        public bool HasAuthority => IsHost;

        /// 래그 보상: 200ms 예측+보정 (문서 06-5). 실제 보간은 동기화 컴포넌트에서.
        public float predictionMs = 200f;
    }

    /// 동기화 컴포넌트 골격 (실제는 NetworkBehaviour 상속).
    public class SyncMerge : MonoBehaviour { /* 합체/분리 상태 동기화 */ }
    public class SyncCombat : MonoBehaviour { /* 데미지 결과 브로드캐스트 */ }
    public class SyncDungeon : MonoBehaviour { /* 시드/방 진행 동기화 */ }
}
