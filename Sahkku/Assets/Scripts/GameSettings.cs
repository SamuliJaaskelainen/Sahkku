using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public enum PieceModel
    {
        Wood,
        Bone
    }

    public enum Player
    {
        One,
        Two
    }

    public static bool singlePlayer = true;
    public static bool evenOdds = false;
    public static Player startingPlayer = Player.One;
    public static PieceModel p1Model = PieceModel.Wood;
    public static PieceModel p2Model = PieceModel.Wood;
    public static PieceModel kingModel = PieceModel.Wood;
    public static bool muteSounds = false;
    public static bool muteMusic = false;
}
