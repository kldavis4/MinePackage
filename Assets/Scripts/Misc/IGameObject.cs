public interface IPrefab
{
    World World { get; set; }
    void CheckIfMovedOutsideTheWorld();
}