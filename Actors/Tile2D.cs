using System;
using DenshiMahjong.Mahjong;
using Godot;

public class Tile2D : Control
{
	public event Action OnTileClicked;
	
	private bool _is_ready = false;
	private Tile _data;

	public Tile Data
	{
		get => _data;
		set
		{
			_data = value;
			UpdateUI();
		}
	}

	private Label _value, _kind;

	public override void _Ready()
	{
		_value = GetNode("Value") as Label;
		_kind = GetNode("Kind") as Label;
		_is_ready = true;
		UpdateUI();
	}

	private void UpdateUI()
	{
		var str = Data.ToString();
		var parts = str.Split("/");
		if (_is_ready)
		{
			_kind.Text = parts[0];
			_value.Text = parts[1];
		}
	}
	
	private void onInput(object @event)
	{
		if (@event is InputEventMouseButton)
		{
			var e = @event as InputEventMouseButton;
			if (e.ButtonIndex == 1 && e.IsPressed())
			{
				OnTileClicked?.Invoke();
			}
		}
	}
}
