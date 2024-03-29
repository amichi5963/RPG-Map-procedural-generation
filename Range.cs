using UnityEngine;
using System.Collections;

public class Range
{

	public Position Start { get; set; }
	public Position End { get; set; }

	public int GetWidthX()
	{
		return End.X - Start.X + 1;
	}

	public int GetWidthY()
	{
		return End.Y - Start.Y + 1;
	}

	public Range(Position start, Position end)
	{
		Start = start;
		End = end;
	}

	public Range(int startX, int startY, int endX, int endY) : this(new Position(startX, startY), new Position(endX, endY)) { }

	public Range() : this(0, 0, 0, 0) { }

	public override string ToString()
	{
		return string.Format("{0} => {1}", Start, End);
	}

	// 座標が範囲内に含まれるかどうかを判定する関数
	public bool ContainsPosition(Position position)
	{
		return position.X >= Start.X && position.X <= End.X &&
			   position.Y >= Start.Y && position.Y <= End.Y;
	}
}