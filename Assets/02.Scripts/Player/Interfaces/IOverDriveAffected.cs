namespace _02.Scripts.Player.Interfaces
{
    // 오버드라이브 영향 받는 인터페이스
    public interface IOverDriveAffected
    {
        bool IsOverDriveActive { get; set; }
    }
}