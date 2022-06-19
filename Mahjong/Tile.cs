using System;

namespace DenshiMahjong.Mahjong
{
	public class Tile : IComparable
	{
		public enum Kind
		{
			Character,
			Bamboo,
			Circle,
			Wind,
			Dragon
		}

		public Kind kind;
		public int value;

		public Tile(Kind kind, int value)
		{
			this.kind = kind;
			this.value = value;
		}

		public override string ToString()
		{
			switch (kind)
			{
				case Kind.Bamboo:
					return $"B/{value}";
				case Kind.Character:
					return $"C/{value}";
				case Kind.Circle:
					return $"P/{value}";
				case Kind.Dragon:
					return $"D/{((Dragon) value).ToString()[0]}";
				case Kind.Wind:
					return $"W/{((Wind) value).ToString()[0]}";
			}

			return "UNK";
		}

		public int CompareTo(object other)
		{
			if (other is Tile)
			{
				Tile otherTile = (Tile) other;
				if (kind == otherTile.kind)
				{
					return value - otherTile.value;
				}
				else
				{
					return (int) kind - (int) otherTile.kind;
				}
			}
			else
			{
				throw new ArgumentException("Object is not a Tile");
			}
		}
		
		public bool IsHonor => kind == Kind.Dragon || kind == Kind.Wind;
		public bool IsTerminal => IsHonor || value == 1 || value == 9;

		public Tile Next
		{
			get
			{
				var wrap = 10;
				var wrapTo = 1;
				switch (kind)
				{
					case Kind.Dragon:
						wrap = Enum.GetValues(typeof(Dragon)).Length;
						wrapTo = (int) Dragon.White;
						break;
					case Kind.Wind:
						wrap = Enum.GetValues(typeof(Wind)).Length;
						wrapTo = (int) Wind.East;
						break;
				}

				var nextValue = value + 1;
				if (nextValue >= wrap)
				{
					nextValue = wrapTo;
				}

				return new Tile(kind, nextValue);
			}
		}
	}

	public enum Dragon : int
	{
		White,
		Green,
		Red
	}

	public enum Wind : int
	{
		East,
		South,
		West,
		North
	}
}
