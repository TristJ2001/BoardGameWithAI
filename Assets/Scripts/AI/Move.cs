using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
   public Vector2Int JumpPosition { get; set; } = Vector2Int.zero;
   public Vector2Int PlayerPosition { get; set; } = Vector2Int.zero;
   public int JumpPiece, PlayerPiece;
   public Move(int jumpPiece, int playerPiece, Vector2Int jumpPosition, Vector2Int playerPosition)
   {
      JumpPosition = jumpPosition;
      PlayerPosition = playerPosition;
      JumpPiece = jumpPiece;
      PlayerPiece = playerPiece;
   }
   
   public override bool Equals(object obj)
   {
      Move other = (Move) obj;

      if(other.JumpPosition.x != JumpPosition.x){
         return false;
      }

      if(other.JumpPosition.y != JumpPosition.y){
         return false;
      }
      
      if(other.PlayerPosition.x != PlayerPosition.x){
         return false;
      }

      if(other.PlayerPosition.y != PlayerPosition.y){
         return false;
      }

      return true;
   }
   
   public override int GetHashCode()
   {
      return base.GetHashCode();
   }
}
